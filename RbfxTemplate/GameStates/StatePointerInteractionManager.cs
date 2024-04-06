using Urho3DNet;

namespace RbfxTemplate.GameStates
{
    /// <summary>
    /// Class to manage touch and mouse interactions.
    ///
    /// Interaction starts when you press mouse button or touch screen.
    /// It ends when you release mouse button or remove finger from screen.
    /// Mouse interaction may be canceled when you press another mouse button.
    /// For mouse interactions the qualifier is "sticky", i.e. it will keep value it had when interaction started even if you release a key.
    /// </summary>
    public class StatePointerInteractionManager
    {
        /// <summary>
        /// Current game state.
        /// </summary>
        private StateBase _state;

        /// <summary>
        /// Get or set current game state.
        /// </summary>
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

        /// <summary>
        /// Key of an active interaction.
        /// </summary>
        private InteractionKey? _activeInteractionKey = null;


        /// <summary>
        /// Last known mouse cursor position.
        /// </summary>
        public IntVector2 LastKnownMousePosition { get; set; }

        /// <summary>
        /// Handle touch begin event.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        public void HandleTouchBegin(VariantMap args)
        {
            var touchId = args[E.TouchBegin.TouchID].Int;
            var x = args[E.TouchBegin.X].Int;
            var y = args[E.TouchBegin.Y].Int;
            _state.StartInteraction(new InteractionKey(touchId), new IntVector2(x, y));
        }

        /// <summary>
        /// Handle touch end event.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        public void HandleTouchEnd(VariantMap args)
        {
            var touchId = args[E.TouchEnd.TouchID].Int;
            var x = args[E.TouchEnd.X].Int;
            var y = args[E.TouchEnd.Y].Int;
            _state.EndInteraction(new InteractionKey(touchId), new IntVector2(x, y));
        }

        /// <summary>
        /// Handle touch move event.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        public void HandleTouchMove(VariantMap args)
        {
            var touchId = args[E.TouchMove.TouchID].Int;
            var x = args[E.TouchMove.X].Int;
            var y = args[E.TouchMove.Y].Int;
            _state.UpdateInteraction(new InteractionKey(touchId), new IntVector2(x, y));
        }

        /// <summary>
        /// Handle mouse move event.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        public void HandleMouseMove(VariantMap args)
        {
            var buttons = (MouseButton)args[E.MouseMove.Buttons].Int;
            var qualifiers = (Qualifier)args[E.MouseMove.Qualifiers].Int;
            var x = args[E.MouseMove.X].Int;
            var y = args[E.MouseMove.Y].Int;
            LastKnownMousePosition = new IntVector2(x, y);
            var interactionKey = new InteractionKey(buttons, _activeInteractionKey?.Qualifiers ?? qualifiers);
            RestartMouseInteractionIfNecessary(interactionKey);
            _state.UpdateInteraction(interactionKey, LastKnownMousePosition);
        }

        /// <summary>
        /// Handle mouse button up event.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        public void HandleMouseUp(VariantMap args)
        {
            var buttons = (MouseButton)args[E.MouseButtonUp.Buttons].Int;
            var qualifiers = (Qualifier)args[E.MouseButtonUp.Qualifiers].Int;
            var button = (MouseButton)args[E.MouseButtonUp.Button].Int;
            _state.EndInteraction(new InteractionKey(buttons | button, _activeInteractionKey?.Qualifiers ?? qualifiers), LastKnownMousePosition);
            _activeInteractionKey = null;
        }

        /// <summary>
        /// Handle mouse button down event.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        public void HandleMouseDown(VariantMap args)
        {
            var buttons = (MouseButton)args[E.MouseButtonDown.Buttons].Int;
            var qualifiers = (Qualifier)args[E.MouseButtonDown.Qualifiers].Int;
            var button = args[E.MouseButtonDown.Button].Int;
            var interactionKey = new InteractionKey(buttons, _activeInteractionKey?.Qualifiers ?? qualifiers);
            RestartMouseInteractionIfNecessary(interactionKey);
            _activeInteractionKey = interactionKey;
            _state.StartInteraction(interactionKey, LastKnownMousePosition);
        }

        /// <summary>
        /// Handle directional pad event.
        /// </summary>
        /// <param name="direction">Dpad direction.</param>
        public void HandleDpad(HatPosition direction)
        {
            _state.HandleDirectionPad(direction);
        }

        /// <summary>
        /// Cancel active events and start new one if mouse event changed.
        /// </summary>
        /// <param name="newKey">New mouse interaction key.</param>
        private void RestartMouseInteractionIfNecessary(InteractionKey newKey)
        {
            if (!_activeInteractionKey.HasValue)
                return;

            var oldKey = _activeInteractionKey.Value;
            if (oldKey.SourceId != newKey.SourceId || oldKey.Qualifiers != newKey.Qualifiers)
            {
                _state.CancelInteraction(oldKey, LastKnownMousePosition);
                _activeInteractionKey = null;
            }
        }

    }
}