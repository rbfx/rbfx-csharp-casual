using Urho3DNet;

namespace RbfxTemplate
{
    [ObjectFactory]
    [Preserve(AllMembers = true)]
    public class AcceptPrivacyPolicyState : RmlUIStateBase
    {
        protected readonly SharedPtr<Scene> _scene;
        private readonly Viewport _viewport;

        public AcceptPrivacyPolicyState(UrhoPluginApplication app) : base(app, "UI/PrivacyPolicy.rml")
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
            component.BindDataModelEvent("PrivacyPolicy", OnPrivacyPolicy);
            component.BindDataModelEvent("Accept", OnAccept);
        }

        public void OnPrivacyPolicy(VariantList variantList)
        {
            //TODO: Replace with actual privacy policy
            Urho3D.OpenURL("https://app.freeprivacypolicy.com/wizard/privacy-policy?step=2");
        }

        public void OnAccept(VariantList variantList)
        {
            Application.AcceptPrivacyPolicy();
        }

        public void OnContinue(VariantList variantList)
        {
            Application.ContinueGame();
        }
    }
}