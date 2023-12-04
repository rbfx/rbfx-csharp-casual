using Urho3DNet;

namespace RbfxTemplate.GameStates
{
    public class PickState: StateBase
    {
        private readonly Node _pointer;
        private Tile _hintTile = null;
        private float _hintTime;

        public PickState(GameState game, Node pointer) : base(game)
        {
            _pointer = pointer;
            _pointer.SetDeepEnabled(false);
        }

        public override void HandleMouseDown(int button, IntVector2 inputMousePosition, int buttons, int qualifiers)
        {
            if (button != 1)
            {
                return;
            }

            var tile = Game.PickTile(inputMousePosition);
            if (tile != null)
            {
                Game.DragTile(tile, null);
                _pointer.SetDeepEnabled(false);
                _hintTile = null;
            }
        }

        public override void Update(float timeStep)
        {
            if (_hintTile != null)
            {
                _hintTime += timeStep*0.5f;
                if (_hintTime >= 1.0f)
                    _hintTime = 0;

                var a = _hintTile.Node.WorldPosition;
                var b = _hintTile.ValidLink.Node.WorldPosition;
                _pointer.WorldPosition = a.Lerp(b,_hintTime);
            }
        }

        public override void HandleTouchBegin(int touchId, IntVector2 touchPosition)
        {
            var tile = Game.PickTile(touchPosition);
            if (tile != null)
            {
                Game.DragTile(tile, touchId);
                _pointer.SetDeepEnabled(false);
                _hintTile = null;
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