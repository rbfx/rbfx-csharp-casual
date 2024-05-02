﻿using Urho3DNet;

namespace RbfxTemplate
{
    /// <summary>
    /// Settings file content.
    /// </summary>
    public class GameSettings
    {
        private static readonly string SOUND_MASTER = "Master";

        private static readonly string SOUND_EFFECT = "Effect";

        //static readonly string SOUND_AMBIENT = "Ambient";
        //static readonly string SOUND_VOICE = "Voice";
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