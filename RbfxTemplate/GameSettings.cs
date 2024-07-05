using System;
using Urho3DNet;

namespace RbfxTemplate
{
    /// <summary>
    /// Settings file content.
    /// </summary>
    public class GameSettings
    {
        private static readonly string SOUND_MASTER = "Master";

        private static readonly string SOUND_EFFECT = "Effect";

        private static readonly string SOUND_MUSIC = "Music";

        /// <summary>
        ///     Get or set master volume.
        /// </summary>
        public float MasterVolume { get; set; } = 1.0f;

        /// <summary>
        ///     Get or set music volume.
        /// </summary>
        public float MusicVolume { get; set; } = 1.0f;

        /// <summary>
        ///     Get or set effects volume.
        /// </summary>
        public float EffectVolume { get; set; } = 1.0f;

        /// <summary>
        /// Time when privacy policy was accepted or null if it wasn't.
        /// </summary>
        public bool? PrivacyPolicyAccepted { get; set; }

        /// <summary>
        ///     Apply settings to the application global settings.
        /// </summary>
        /// <param name="context">Application context.</param>
        public void Apply(Context context)
        {
            var audio = context.GetSubsystem<Audio>();

            audio.SetMasterGain(SOUND_MASTER, MasterVolume);
            audio.SetMasterGain(SOUND_MUSIC, MusicVolume);
            audio.SetMasterGain(SOUND_EFFECT, EffectVolume);
        }
    }
}