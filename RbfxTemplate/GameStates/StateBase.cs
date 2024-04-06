using Urho3DNet;

namespace RbfxTemplate.GameStates
{
    /// <summary>
    /// Base class for game state.
    /// </summary>
    public abstract class StateBase
    {
        /// <summary>
        /// Construct state.
        /// </summary>
        /// <param name="game">Game state.</param>
        protected StateBase(GameState game)
        {
            Game = game;
        }

        /// <summary>
        /// Game application state.
        /// </summary>
        public GameState Game { get; }

        /// <summary>
        /// Engine context.
        /// </summary>
        public Context Context => Game.Context;

        /// <summary>
        /// Activate game state.
        /// </summary>
        public virtual void Activate()
        {
        }

        /// <summary>
        /// Deactivate game state.
        /// </summary>
        public virtual void Deactivate()
        {
        }

        /// <summary>
        /// Handle state update.
        /// </summary>
        /// <param name="timeStep">Update time step in seconds.</param>
        public virtual void Update(float timeStep)
        {
        }

        /// <summary>
        /// Start interaction.
        /// </summary>
        /// <param name="interactionKey">Interaction key.</param>
        /// <param name="interactionPosition">Interaction position.</param>
        public virtual void StartInteraction(InteractionKey interactionKey, IntVector2 interactionPosition)
        {
        }

        /// <summary>
        /// End interaction.
        /// </summary>
        /// <param name="interactionKey">Interaction key.</param>
        /// <param name="interactionPosition">Interaction position.</param>
        public virtual void EndInteraction(InteractionKey interactionKey, IntVector2 interactionPosition)
        {
            
        }

        /// <summary>
        /// Cancel interaction.
        /// </summary>
        /// <param name="interactionKey">Interaction key.</param>
        /// <param name="interactionPosition">Interaction position.</param>
        public virtual void CancelInteraction(InteractionKey interactionKey, IntVector2 interactionPosition)
        {
        }

        /// <summary>
        /// Update interaction.
        /// </summary>
        /// <param name="interactionKey">Interaction key.</param>
        /// <param name="interactionPosition">Interaction position.</param>
        public virtual void UpdateInteraction(InteractionKey interactionKey, IntVector2 interactionPosition)
        {
        }

        /// <summary>
        /// Handle direction pad.
        /// </summary>
        /// <param name="direction">Direction pad position. Only left, right, up or down are valid arguments here.</param>
        public virtual void HandleDirectionPad(HatPosition direction)
        {
        }
    }
}