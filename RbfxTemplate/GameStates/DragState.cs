using Urho3DNet;

namespace RbfxTemplate.GameStates
{
    public class DragState : StateBase
    {
        private Tile _tile;
        private int? _touchId;

        public DragState(GameState game) : base(game)
        {
        }

        public override void Activate()
        {
            _tile.Link.IsEnabled = true;
            base.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        public override void HandleMouseMove(IntVector2 intVector2, int buttons, int qualifiers)
        {
            if (_touchId.HasValue) return;

            MoveLink(intVector2);
        }

        public override void HandleMouseUp(int button, IntVector2 inputMousePosition, int buttons, int qualifiers)
        {
            if (_touchId.HasValue) return;

            CompleteLink(inputMousePosition);
        }

        public override void HandleTouchMove(int touchId, IntVector2 intVector2)
        {
            if (_touchId != touchId) return;

            MoveLink(intVector2);
        }

        public override void HandleTouchEnd(int touchId, IntVector2 intVector2)
        {
            if (_touchId != touchId) return;

            CompleteLink(intVector2);
        }

        public void TrackTile(Tile tile, int? touchId)
        {
            _tile = tile;
            _touchId = touchId;
            tile.LinkTo(null);
            var material = Context.ResourceCache.GetResource<Material>("Materials/White.material");
            tile.Link.GetComponent<AnimatedModel>(true).SetMaterial(material);
        }

        private void MoveLink(IntVector2 intVector2)
        {
            var ray = Game.GetScreenRay(intVector2);
            var normal = Vector3.Up;
            var d = normal.DotProduct(ray.Direction);
            var t = -(normal.DotProduct(ray.Origin) - _tile.Link.Position.Y) / d;
            _tile.LinkTarget.WorldPosition = ray.Origin + ray.Direction * t;
            _tile.LinkSource.LookAt(_tile.LinkTarget.WorldPosition);
            _tile.LinkTarget.LookAt(_tile.LinkSource.WorldPosition);
        }

        private void CompleteLink(IntVector2 inputMousePosition)
        {
            var link = Game.PickTile(inputMousePosition);
            _tile.LinkTo(link);
            Game.StartPicking();
        }
    }
}