using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using Urho3DNet;

namespace RbfxTemplate
{
    [ObjectFactory]
    [Preserve(AllMembers = true)]
    public class MainMenuState : RmlUIStateBase
    {
        protected readonly SharedPtr<Scene> _scene;
        private readonly Viewport _viewport;

        public MainMenuState(UrhoPluginApplication app) : base(app, "UI/MainMenu.rml")
        {
            _scene = Context.CreateObject<Scene>();
            _scene.Ptr.CreateComponent<Octree>();
            var skybox = _scene.Ptr.CreateComponent<Skybox>();
            skybox.SetModel(ResourceCache.GetResource<Model>("Models/Box.mdl"));
            skybox.SetMaterial(ResourceCache.GetResource<Material>("Materials/Skyplane.xml"));
            _viewport = Context.CreateObject<Viewport>();
            _viewport.Camera = _scene.Ptr.CreateComponent<Camera>();
            _viewport.Scene = _scene;
            SetViewport(0, _viewport);
        }

        public override void OnDataModelInitialized(GameRmlUIComponent component)
        {
            component.BindDataModelProperty("is_game_played", _ => _.Set(Application?.IsGameRunning == true),
                _ => { });
            component.BindDataModelProperty("game_title", _ => _.Set("Awesome game"), _ => { });
            component.BindDataModelEvent("Continue", OnContinue);
            component.BindDataModelEvent("NewGame", OnNewGame);
            component.BindDataModelEvent("Settings", OnSettings);
            component.BindDataModelEvent("Exit", OnExit);
            component.BindDataModelEvent("Discord", OnDiscord);
        }

        public void OnNewGame(VariantList variantList)
        {
            Application.ToNewGame();
        }

        public void OnSettings(VariantList variantList)
        {
            Application.ToSettings();
        }

        public void OnExit(VariantList variantList)
        {
            Application.Quit();
        }

        public void OnContinue(VariantList variantList)
        {
            Application.ContinueGame();
        }

        private void OnDiscord(VariantList obj)
        {
            Urho3D.OpenURL("https://discord.gg/46aKYFQj7W");
        }
    }
}