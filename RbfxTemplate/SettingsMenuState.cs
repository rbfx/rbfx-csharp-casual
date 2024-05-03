using Urho3DNet;

namespace RbfxTemplate
{
    public class SettingsMenuState : RmlUIStateBase
    {
        public SettingsMenuState(UrhoPluginApplication app) : base(app, "UI/Options.rml")
        {
            Settings = app.Settings;
        }

        public GameSettings Settings { get; private set; }

        public override void OnDataModelInitialized(GameRmlUIComponent menuComponent)
        {
            menuComponent.BindDataModelEvent("Apply", OnApply);
            menuComponent.BindDataModelEvent("Cancel", OnCancel);
            menuComponent.BindDataModelProperty("master", val => val.Set(Settings.MasterVolume),
                val => Settings.MasterVolume = val.Float);
            menuComponent.BindDataModelProperty("music", val => val.Set(Settings.MusicVolume),
                val => Settings.MusicVolume = val.Float);
            menuComponent.BindDataModelProperty("effects", val => val.Set(Settings.EffectVolume),
                val => Settings.EffectVolume = val.Float);
        }

        public override void Activate(StringVariantMap bundle)
        {
            Settings = Application.Settings;

            var audio = Context.GetSubsystem<Audio>();

            base.Activate(bundle);
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        private void OnCancel(VariantList obj)
        {
            Application.ResetSettings();

            Application.HandleBackKey();
        }

        private void OnApply(VariantList obj)
        {
            Settings.Apply(Context);
            Application.SaveSettings();

            Application.HandleBackKey();
        }
    }
}