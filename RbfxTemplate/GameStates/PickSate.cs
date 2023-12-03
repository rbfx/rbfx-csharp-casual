using Urho3DNet;

namespace RbfxTemplate.GameStates
{
    public class PickSate: StateBase
    {
        public PickSate(GameState game) : base(game)
        {
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
            }
        }

        public override void HandleTouchBegin(int touchId, IntVector2 touchPosition)
        {
            var tile = Game.PickTile(touchPosition);
            if (tile != null)
            {
                Game.DragTile(tile, touchId);
            }
        }
    }
}