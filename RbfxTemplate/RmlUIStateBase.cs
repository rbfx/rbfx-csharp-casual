using Urho3DNet;

namespace RbfxTemplate
{
    /// <summary>
    ///     Base class for a game state with RmlUI document.
    /// </summary>
    public abstract class RmlUIStateBase : ApplicationState
    {
        private readonly SharedPtr<Scene> _scene;

        /// <summary>
        ///     Construct RmlUIStateBase.
        /// </summary>
        /// <param name="app">Application instance.</param>
        /// <param name="rmlResource">Path to RmlUI document resource.</param>
        public RmlUIStateBase(UrhoPluginApplication app, string rmlResource) : base(app.Context)
        {
            MouseMode = MouseMode.MmFree;
            IsMouseVisible = true;
            Application = app;

            _scene = Context.CreateObject<Scene>();
            RmlUiComponent = _scene.Ptr.CreateComponent<GameRmlUIComponent>();
            RmlUiComponent.IsEnabled = false;
            RmlUiComponent.State = this;
            RmlUiComponent.SetResource(rmlResource);
            RmlUiComponent.IsEnabled = false;
        }

        /// <summary>
        ///     RmlUI Component for the game state.
        /// </summary>
        public GameRmlUIComponent RmlUiComponent { get; }

        /// <summary>
        ///     Application instance.
        /// </summary>
        public UrhoPluginApplication Application { get; }

        /// <summary>
        ///     Data model initializer.
        /// </summary>
        /// <param name="component">RmlUIComponent to initialize.</param>
        public abstract void OnDataModelInitialized(GameRmlUIComponent component);

        /// <summary>
        ///     Game state activation handler.
        /// </summary>
        /// <param name="bundle">Game state parameters.</param>
        public override void Activate(StringVariantMap bundle)
        {
            RmlUiComponent.IsEnabled = true;
            RmlUiComponent.UpdateProperties();
            SubscribeToEvent(E.KeyUp, HandleKeyUp);
            base.Activate(bundle);
        }

        /// <summary>
        ///     Game state deactivation handler.
        /// </summary>
        public override void Deactivate()
        {
            RmlUiComponent.IsEnabled = false;
            UnsubscribeFromEvent(E.KeyUp);
            base.Deactivate();
        }

        /// <summary>
        ///     Key up handler to navigate back in the game state hierarchy.
        /// </summary>
        /// <param name="args"></param>
        private void HandleKeyUp(VariantMap args)
        {
            var key = (Key)args[E.KeyUp.Key].Int;
            switch (key)
            {
                case Key.KeyEscape:
                case Key.KeyBackspace:
                    Application.HandleBackKey();
                    return;
            }
        }
    }
}