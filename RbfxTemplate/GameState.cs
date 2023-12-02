using System.Collections.Generic;
using System.IO;
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
        private readonly Node _link;

        public GameState(UrhoPluginApplication app) : base(app, "UI/GameScreen.rml")
        {
            MouseMode = MouseMode.MmFree;
            IsMouseVisible = true;

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

            _link = _scene.Ptr.GetChild("Link", true);

            Vector3 pos = Vector3.Zero;
            foreach (var tileMaterial in _tileMaterials)
            {
                var tile = _scene.Ptr.CreateChild();
                var prefabReference = tile.CreateComponent<PrefabReference>();
                prefabReference.SetPrefab(Context.ResourceCache.GetResource<PrefabResource>("Objects/Tile.prefab"));
                prefabReference.Inline(PrefabInlineFlag.None);
                var model = tile.GetComponent<StaticModel>(true);
                model.SetMaterial(tileMaterial.Value);
                tile.Position = pos;
                pos += Vector3.Forward;
            }

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

            _scene.Ptr.IsUpdateEnabled = true;



            base.Activate(bundle);
        }

        private void HandleMouseMove(StringHash arg1, VariantMap arg2)
        {
            if (!_link.IsEnabled)
                return;

            var x = arg2[E.MouseMove.X].Int;
            var y = arg2[E.MouseMove.Y].Int;
            var ray = _viewport.GetScreenRay(x, y);
            var normal = Vector3.Up;
            var d = normal.DotProduct(ray.Direction);
            float t = -(normal.DotProduct(ray.Origin) + 0.0f) / d;
            var a = _link.GetChild("A", true);
            var b = _link.GetChild("B", true);
            b.Position = ray.Origin + ray.Direction * t;
            a.LookAt(b.Position);
            b.LookAt(a.Position);
        }

        private void HandleMouseUp(StringHash arg1, VariantMap arg2)
        {
            _link.IsEnabled = false;
        }

        private void HandleMouseDown(StringHash arg1, VariantMap arg2)
        {
            _link.IsEnabled = true;
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
    }
}