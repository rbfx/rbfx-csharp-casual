using Urho3DNet;

namespace RbfxTemplate.GameStates
{
    public class VictoryState : StateBase
    {
        public VictoryState(GameState game) : base(game)
        {
        }

        public override void HandleMouseUp(int button, IntVector2 inputMousePosition, int buttons, int qualifiers)
        {
            base.HandleMouseUp(button, inputMousePosition, buttons, qualifiers);
            //Game.NextLevel();
        }

        public override void HandleTouchEnd(int touchId, IntVector2 intVector2)
        {
            base.HandleTouchEnd(touchId, intVector2);
            //Game.NextLevel();
        }
    }
}