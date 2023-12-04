using System;
using System.Globalization;
using Urho3DNet;

namespace RbfxTemplate
{
    /// <summary>
    ///     This class represents an Urho3D plugin application.
    /// </summary>
    [LoadablePlugin]
    [Preserve(AllMembers = true)]
    public class UrhoPluginApplication : PluginApplication
    {
        /// <summary>
        ///     PrivacyPolicyAccepted identifier
        /// </summary>
        private static readonly FileIdentifier privacyPolicyAcceptedFileId_ =
            new FileIdentifier("conf", "PrivacyPolicyAccepted");

        /// <summary>
        ///     Safe pointer to game screen.
        /// </summary>
        private SharedPtr<GameState> _gameState;

        /// <summary>
        ///     Safe pointer to menu screen.
        /// </summary>
        private SharedPtr<MainMenuState> _mainMenuState;

        /// <summary>
        ///     Application state manager.
        /// </summary>
        private StateStack _stateStack;

        public UrhoPluginApplication(Context context) : base(context)
        {
        }

        /// <summary>
        ///     Gets a value indicating whether the game is running.
        /// </summary>
        public bool IsGameRunning => _gameState;

        public override bool IsMain()
        {
            return true;
        }


        /// <summary>
        ///     Transition to game
        /// </summary>
        public void ToNewGame()
        {
            _gameState?.Dispose();
            _gameState = new GameState(this);
            _stateStack.Push(_gameState);
        }

        /// <summary>
        ///     Transition to game
        /// </summary>
        public void ContinueGame()
        {
            if (_gameState) _stateStack.Push(_gameState);
            ;
        }

        public void Quit()
        {
            Context.Engine.Exit();
        }

        public void HandleBackKey()
        {
            if (_stateStack.State == _mainMenuState.Ptr)
            {
                if (IsGameRunning)
                    ContinueGame();
                else
                    Quit();
            }
            else
            {
                _stateStack.Pop();
            }
        }

        /// <summary>
        ///     Mark privacy policy as accepted and go to main menu.
        /// </summary>
        public void AcceptPrivacyPolicy()
        {
            // Create file marker that privacy policy was accepted.
            Context.VirtualFileSystem.WriteAllText(privacyPolicyAcceptedFileId_,
                DateTime.Now.ToString(CultureInfo.InvariantCulture));

            // Crate end enqueue main menu screen.
            _mainMenuState = _mainMenuState ?? new MainMenuState(this);
            _stateStack.Switch(_mainMenuState);
        }


        protected override void Load()
        {
            Context.RegisterFactories(GetType().Assembly);
        }

        protected override void Unload()
        {
            Context.RemoveFactories(GetType().Assembly);
        }

        protected override void Start(bool isMain)
        {
            _stateStack = new StateStack(Context.GetSubsystem<StateManager>());

            // Setup state manager.
            var stateManager = Context.GetSubsystem<StateManager>();
            stateManager.FadeInDuration = 0.1f;
            stateManager.FadeOutDuration = 0.1f;

            //StringList stringList = new StringList();
            //Context.VirtualFileSystem.Scan(stringList, new FileIdentifier("", "Images/Emoji"), "*.png", ScanFlag.ScanFiles);
            //foreach (var imageName in stringList)
            //{
            //    Context.ResourceCache.BackgroundLoadResource(nameof(Texture2D), "Images/Emoji/" + imageName);
            //}

            // Setup end enqueue splash screen.
            using (SharedPtr<SplashScreen> splash = new SplashScreen(Context))
            {
                splash.Ptr.Duration = 1.0f;
                splash.Ptr.BackgroundImage = Context.ResourceCache.GetResource<Texture2D>("Images/Background.png");
                splash.Ptr.ForegroundImage = Context.ResourceCache.GetResource<Texture2D>("Images/Splash.png");
                stateManager.EnqueueState(splash);
            }

            if (!Context.VirtualFileSystem.Exists(privacyPolicyAcceptedFileId_))
            {
                _stateStack.Push(new AcceptPrivacyPolicyState(this));
            }
            else
            {
                // Crate end enqueue main menu screen.
                _mainMenuState = _mainMenuState ?? new MainMenuState(this);
                _stateStack.Push(_mainMenuState);
            }

            base.Start(isMain);
        }

        protected override void Stop()
        {
            _mainMenuState?.Dispose();
            _gameState?.Dispose();

            base.Stop();
        }

        protected override void Suspend(Archive output)
        {
            base.Suspend(output);
        }

        protected override void Resume(Archive input, bool differentVersion)
        {
            base.Resume(input, differentVersion);
        }
    }
}