using Urho3DNet;

namespace RbfxTemplate.GameStates
{
    public class PickState : StateBase
    {
        private readonly Node _pointer;
        private Tile _hintTile;
        private float _hintTime;

        public PickState(GameState game, Node pointer) : base(game)
        {
            _pointer = pointer;
            _pointer.SetDeepEnabled(false);
        }

        public override void StartInteraction(InteractionKey interactionKey, IntVector2 interactionPosition)
        {
            var tile = Game.PickTile(interactionPosition);
            if (tile != null)
            {
                Game.DragTile(tile, interactionKey);
                _pointer.SetDeepEnabled(false);
                _hintTile = null;
            }
        }

        public override void Update(float timeStep)
        {
            if (_hintTile != null)
            {
                _hintTime += timeStep * 0.5f;
                if (_hintTime >= 1.0f)
                    _hintTime = 0;

                var a = _hintTile.Node.WorldPosition;
                var b = _hintTile.ValidLink.Node.WorldPosition;
                _pointer.WorldPosition = a.Lerp(b, _hintTime);
            }
        }

        public void ShowHint(Tile tile)
        {
            _hintTile = tile;
            _pointer.SetDeepEnabled(true);
            _hintTime = 0.0f;
        }
    }
}