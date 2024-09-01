using Urho3DNet;

namespace RbfxTemplate.GameStates
{
    public class DragState : StateBase
    {
        InteractionKey? _interactionKey;

        private Tile _tile;

        public DragState(GameState game) : base(game)
        {
        }

        public override void Activate()
        {
            if (_tile != null)
            {
                _tile.Link.IsEnabled = true;
            }

            base.Activate();
        }

        public override void EndInteraction(InteractionKey interactionKey, IntVector2 interactionPosition)
        {
            if (interactionKey != _interactionKey)
                return;

            CompleteLink(interactionPosition);
            _interactionKey = null;
        }

        public override void CancelInteraction(InteractionKey interactionKey, IntVector2 interactionPosition)
        {
            if (interactionKey != _interactionKey)
                return;

            _interactionKey = null;
            Game.StartPicking();
        }

        public override void UpdateInteraction(InteractionKey interactionKey, IntVector2 interactionPosition)
        {
            if (interactionKey != _interactionKey)
                return;

            MoveLink(interactionPosition);
        }

        public override void Update(float timeStep)
        {
        }

        public void TrackTile(Tile tile, InteractionKey interactionKey)
        {
            _tile = tile;
            _interactionKey = interactionKey;
            tile.LinkTo(null);
            var material = Context.ResourceCache.GetResource<Material>("Materials/White.material");
            tile.Link.FindComponent<AnimatedModel>(ComponentSearchFlag.Default | ComponentSearchFlag.Disabled).SetMaterial(material);
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