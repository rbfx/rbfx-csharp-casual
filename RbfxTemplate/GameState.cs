using System.Collections.Generic;
using System.IO;
using RbfxTemplate.GameStates;
using Urho3DNet;

namespace RbfxTemplate
{
    [ObjectFactory]
    [Preserve(AllMembers = true)]
    public sealed class GameState : RmlUIStateBase
    {
        private readonly SharedPtr<Scene> _scene;
        private readonly UrhoPluginApplication _app;
        private readonly Node _cameraNode;
        private readonly Viewport _viewport;

        private Dictionary<string, Material> _tileMaterials = new Dictionary<string, Material>();

        private PickSate _pickSate;
        private DragSate _dragSate;
        private StateBase _state;

        public StateBase State
        {
            get
            {
                return _state;
            }
            set
            {
                if (_state != value)
                {
                    _state?.Deactivate();
                    _state = value;
                    _state?.Activate();
                }
            }
        }

        private PhysicsRaycastResult _raycastResult;

        public GameState(UrhoPluginApplication app) : base(app, "UI/GameScreen.rml")
        {
            MouseMode = MouseMode.MmFree;
            IsMouseVisible = true;

            _raycastResult = new PhysicsRaycastResult();

            _app = app;
            _scene = Context.CreateObject<Scene>();
            _scene.Ptr.LoadXML("Scenes/Scene.scene");

            _cameraNode = _scene.Ptr.GetChild("Main Camera");
            _viewport = Context.CreateObject<Viewport>();
            _viewport.Camera = _cameraNode?.GetComponent<Camera>();
            _viewport.Scene = _scene;
            SetViewport(0, _viewport);
            _scene.Ptr.IsUpdateEnabled = false;

            StringList stringList = new StringList();
            Context.VirtualFileSystem.Scan(stringList, new FileIdentifier("","Materials/Emoji"), "*.material", ScanFlag.ScanFiles);

            foreach (var matName in stringList)
            {
                _tileMaterials[Path.GetFileNameWithoutExtension(matName)] =
                    Context.ResourceCache.GetResource<Material>("Materials/Emoji/" + matName);
            }

            Vector3 pos = Vector3.Zero;
            var tilePrefab = Context.ResourceCache.GetResource<PrefabResource>("Objects/Tile.prefab");
            foreach (var tileMaterial in _tileMaterials)
            {
                var tile = _scene.Ptr.InstantiatePrefab(tilePrefab);
                //var prefabReference = tile.CreateComponent<PrefabReference>();
                //prefabReference.SetPrefab();
                //prefabReference.Inline(PrefabInlineFlag.None);
                var model = tile.GetComponent<StaticModel>(true);
                model.SetMaterial(tileMaterial.Value);
                tile.Position = pos;
                pos += Vector3.Forward;
            }

            _pickSate = new PickSate(this);
            _dragSate = new DragSate(this);
            State = _pickSate;

            Deactivate();
        }

        public override void OnDataModelInitialized(GameRmlUIComponent component)
        {
        }

        public override void Activate(StringVariantMap bundle)
        {
            SubscribeToEvent(E.KeyUp, HandleKeyUp);

            SubscribeToEvent(E.MouseButtonDown, HandleMouseDown);
            SubscribeToEvent(E.MouseButtonUp, HandleMouseUp);
            SubscribeToEvent(E.MouseMove, HandleMouseMove);

            SubscribeToEvent(E.TouchBegin, HandleTouchBegin);
            SubscribeToEvent(E.TouchEnd, HandleTouchEnd);
            SubscribeToEvent(E.TouchMove, HandleTouchMove);

            _scene.Ptr.IsUpdateEnabled = true;



            base.Activate(bundle);
        }

        private void HandleTouchBegin(VariantMap args)
        {
            var touchId = args[E.TouchBegin.TouchID].Int;
            var x = args[E.TouchBegin.X].Int;
            var y = args[E.TouchBegin.Y].Int;
            _state.HandleTouchBegin(touchId, new IntVector2(x,y));
        }

        private void HandleTouchEnd(VariantMap args)
        {
            var touchId = args[E.TouchEnd.TouchID].Int;
            var x = args[E.TouchEnd.X].Int;
            var y = args[E.TouchEnd.Y].Int;
            _state.HandleTouchEnd(touchId, new IntVector2(x, y));
        }

        private void HandleTouchMove(VariantMap args)
        {
            var touchId = args[E.TouchMove.TouchID].Int;
            var x = args[E.TouchMove.X].Int;
            var y = args[E.TouchMove.Y].Int;
            _state.HandleTouchMove(touchId, new IntVector2(x, y));
        }

        private void HandleMouseMove(VariantMap args)
        {
            var buttons = args[E.MouseMove.Buttons].Int;
            var qualifiers = args[E.MouseMove.Qualifiers].Int;
            var x = args[E.MouseMove.X].Int;
            var y = args[E.MouseMove.Y].Int;
            _state.HandleMouseMove(new IntVector2(x, y), buttons, qualifiers);

        }

        private void HandleMouseUp(StringHash arg1, VariantMap args)
        {
            var buttons = args[E.MouseButtonUp.Buttons].Int;
            var qualifiers = args[E.MouseButtonUp.Qualifiers].Int;
            var button = args[E.MouseButtonUp.Button].Int;
            _state.HandleMouseUp(button, Context.Input.MousePosition, buttons, qualifiers);
        }

        private void HandleMouseDown(StringHash arg1, VariantMap args)
        {
            var buttons = args[E.MouseButtonDown.Buttons].Int;
            var qualifiers = args[E.MouseButtonDown.Qualifiers].Int;
            var button = args[E.MouseButtonDown.Button].Int;
            _state.HandleMouseDown(button, Context.Input.MousePosition, buttons, qualifiers);
        }

        public override void Deactivate()
        {
            _scene.Ptr.IsUpdateEnabled = false;
            UnsubscribeFromEvent(E.KeyUp);
            base.Deactivate();
        }

        protected override void Dispose(bool disposing)
        {
            _scene?.Dispose();

            base.Dispose(disposing);
        }

        private void HandleKeyUp(VariantMap args)
        {
            var key = (Key)args[E.KeyUp.Key].Int;
            switch (key)
            {
                case Key.KeyEscape:
                case Key.KeyBackspace:
                    _app.HandleBackKey();
                    return;
            }
        }
        public Ray GetScreenRay(IntVector2 inputMousePosition)
        {
            return _viewport.GetScreenRay(inputMousePosition.X, inputMousePosition.Y);
        }

        public Tile PickTile(IntVector2 inputMousePosition)
        {
            var ray = GetScreenRay(inputMousePosition);
            var physics = _scene.Ptr.GetComponent<PhysicsWorld>();
            physics.RaycastSingle(_raycastResult, ray, 100);
            if (_raycastResult.Body != null)
            {
                return _raycastResult.Body.Node.GetComponent<Tile>() ?? _raycastResult.Body.Node.GetParentComponent<Tile>(true);
            }
            return null;
        }

        public void DragTile(Tile tile, int? touchId)
        {
            _dragSate.TrackTile(tile, touchId);
            State = _dragSate;

        }

        public void LinkTiles(Tile pickTile)
        {
            State = _pickSate;
        }
    }
}