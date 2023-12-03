using Urho3DNet;

namespace RbfxTemplate.GameStates
{
    public abstract class StateBase
    {
        private readonly GameState _game;

        public GameState Game => _game;

        public StateBase(GameState game)
        {
            _game = game;
        }

        public virtual void HandleTouchBegin(int touchId, IntVector2 intVector2)
        {
            
        }

        public virtual void HandleTouchMove(int touchId, IntVector2 intVector2)
        {
            
        }

        public virtual void HandleTouchEnd(int touchId, IntVector2 intVector2)
        {
            
        }

        public virtual void HandleMouseMove(IntVector2 intVector2, int buttons, int qualifiers)
        {
            
        }

        public virtual void HandleMouseDown(int button, IntVector2 inputMousePosition, int buttons, int qualifiers)
        {
            
        }

        public virtual void HandleMouseUp(int button, IntVector2 inputMousePosition, int buttons, int qualifiers)
        {
            
        }

        public virtual void Activate()
        {
            
        }

        public virtual void Deactivate()
        {
            
        }

        public virtual void Update(float timeStep)
        {
        }
    }
}
