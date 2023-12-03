using System;
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
        private readonly PrefabResource _tilePrefab;

        private readonly Dictionary<string, Material> _tileMaterials = new Dictionary<string, Material>();

        private readonly PickSate _pickSate;
        private readonly DragSate _dragSate;
        private StateBase _state;
        private readonly Random _random = new Random();
        private readonly List<Tile> _currentTiles = new List<Tile>();

        private int _levelIndex;
        private int _hintsLeft = 1;

        private readonly Tuple<string, string>[] _pairs =
        {
            Tuple.Create("emoji_u1f436", "emoji_u1f431"),
            Tuple.Create("emoji_u1f34e", "emoji_u1f34a"),
            Tuple.Create("emoji_u1f3b5", "emoji_u1f3b6"),
            Tuple.Create("emoji_u1f31e", "emoji_u1f31b"),
            Tuple.Create("emoji_u1f382", "emoji_u1f381"),
            Tuple.Create("emoji_u1f697", "emoji_u1f68c"),
            Tuple.Create("emoji_u1f339", "emoji_u1f33b"),
            Tuple.Create("emoji_u1f3c0", "emoji_u26bd"),
            Tuple.Create("emoji_u1f355", "emoji_u1f354"),
            Tuple.Create("emoji_u1f4da", "emoji_u1f58a"),
            Tuple.Create("emoji_u1f48d", "emoji_u1f490"),
            Tuple.Create("emoji_u1f383", "emoji_u1f47b"),
            Tuple.Create("emoji_u1f384", "emoji_u1f385"),
            Tuple.Create("emoji_u1f386", "emoji_u1f387"),
            Tuple.Create("emoji_u1f308", "emoji_u1f984"),
            Tuple.Create("emoji_u1f5fd", "emoji_u1f5fc"),
            Tuple.Create("emoji_u1f43c", "emoji_u1f428"),
            Tuple.Create("emoji_u1f37f", "emoji_u1f3a5"),
            Tuple.Create("emoji_u1f30e", "emoji_u1f680"),
            Tuple.Create("emoji_u1f60a", "emoji_u1f622")
        };

        private readonly PhysicsRaycastResult _raycastResult;

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

            var stringList = new StringList();
            Context.VirtualFileSystem.Scan(stringList, new FileIdentifier("", "Images/Emoji"), "*.png",
                ScanFlag.ScanFiles);
            var baseMaterial = Context.ResourceCache.GetResource<Material>("Materials/TileMaterial.material");
            foreach (var imageName in stringList)
            {
                var m = baseMaterial.Clone();
                var tex = Context.ResourceCache.GetResource<Texture2D>("Images/Emoji/" + imageName);
                if (tex == null)
                    throw new ExecutionEngineException();
                m.SetTexture("Albedo", tex);
                _tileMaterials[Path.GetFileNameWithoutExtension(imageName)] = m;
            }

            _tilePrefab = Context.ResourceCache.GetResource<PrefabResource>("Objects/Tile.prefab");

            var pointer =
                _scene.Ptr.InstantiatePrefab(
                    Context.ResourceCache.GetResource<PrefabResource>("Objects/Pointer.prefab"));

            _pickSate = new PickSate(this, pointer);
            _dragSate = new DragSate(this);

            NextLevel();

            Deactivate();
        }

        public StateBase State
        {
            get => _state;
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

        public override void OnDataModelInitialized(GameRmlUIComponent component)
        {
            component.BindDataModelProperty("Level", _ => _.Set("Level " + _levelIndex), _ => { });
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

        public override void Update(float timeStep)
        {
            _state.Update(timeStep);
        }

        public override void Deactivate()
        {
            _scene.Ptr.IsUpdateEnabled = false;
            UnsubscribeFromEvent(E.KeyUp);

            UnsubscribeFromEvent(E.MouseButtonDown);
            UnsubscribeFromEvent(E.MouseButtonUp);
            UnsubscribeFromEvent(E.MouseMove);

            UnsubscribeFromEvent(E.TouchBegin);
            UnsubscribeFromEvent(E.TouchEnd);
            UnsubscribeFromEvent(E.TouchMove);

            base.Deactivate();
        }

        public void AddPairs(int numTiles)
        {
            _currentTiles.Clear();
            var leftTiles = new List<Node>();
            var rightTiles = new List<Node>();
            var visited = new HashSet<string>();
            while (leftTiles.Count < numTiles)
            {
                var pair = _pairs[_random.Next(_pairs.Length)];
                if (visited.Contains(pair.Item1) || visited.Contains(pair.Item2))
                    continue;

                visited.Add(pair.Item1);
                visited.Add(pair.Item2);

                Tile t1, t2;
                {
                    var tile = _scene.Ptr.InstantiatePrefab(_tilePrefab);
                    var model = tile.GetComponent<StaticModel>(true);
                    model.SetMaterial(_tileMaterials[pair.Item1]);
                    leftTiles.Add(tile);
                    t1 = tile.GetComponent<Tile>(true);
                }
                {
                    var tile = _scene.Ptr.InstantiatePrefab(_tilePrefab);
                    var model = tile.GetComponent<StaticModel>(true);
                    model.SetMaterial(_tileMaterials[pair.Item2]);
                    rightTiles.Add(tile);
                    t2 = tile.GetComponent<Tile>(true);
                }
                t1.ValidLink = t2;
                t2.ValidLink = t1;

                _currentTiles.Add(t1);
                _currentTiles.Add(t2);
            }

            Randomize(leftTiles);
            Randomize(rightTiles);

            var d = 1.5f;

            var pos = new Vector3(0, 0, (-leftTiles.Count * 0.5f+0.5f)* d);
            for (var index = 0; index < leftTiles.Count; index++)
            {
                leftTiles[index].Position = pos + new Vector3(-d, 0, index*d);
                rightTiles[index].Position = pos + new Vector3(d, 0, index*d);
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
                return _raycastResult.Body.Node.GetComponent<Tile>() ??
                       _raycastResult.Body.Node.GetParentComponent<Tile>(true);
            return null;
        }

        public void DragTile(Tile tile, int? touchId)
        {
            _dragSate.TrackTile(tile, touchId);
            State = _dragSate;
        }

        public void StartPicking()
        {
            State = _pickSate;
            foreach (var tile in _currentTiles)
                if (tile.LinkedTile != tile.ValidLink)
                {
                    if (_hintsLeft > 0)
                    {
                        _pickSate.ShowHint(tile);
                        --_hintsLeft;
                    }

                    return;
                }

            NextLevel();
        }

        protected override void Dispose(bool disposing)
        {
            _scene?.Dispose();

            base.Dispose(disposing);
        }

        private void Randomize(List<Node> rightTiles)
        {
            for (var index = 0; index < rightTiles.Count; index++)
            {
                var j = _random.Next(rightTiles.Count);
                (rightTiles[j], rightTiles[index]) = (rightTiles[index], rightTiles[j]);
            }
        }

        private void HandleTouchBegin(VariantMap args)
        {
            var touchId = args[E.TouchBegin.TouchID].Int;
            var x = args[E.TouchBegin.X].Int;
            var y = args[E.TouchBegin.Y].Int;
            _state.HandleTouchBegin(touchId, new IntVector2(x, y));
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

        private void NextLevel()
        {
            ++_levelIndex;
            RmlUiComponent.UpdateProperties();
            foreach (var tile in _currentTiles) tile.Node.Remove();

            AddPairs(Math.Min(4, _levelIndex));

            StartPicking();
        }
    }
}