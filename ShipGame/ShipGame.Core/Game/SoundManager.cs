#region File Description
//-----------------------------------------------------------------------------
// SoundManager.cs
//
// Copyright (C) MonoGame Foundation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
#endregion

namespace ShipGame
{
    public class SoundManager : IDisposable
    {
        Dictionary<string, SoundEffect> sounds = new();     // list of sound effects
        ContentManager content;                                                     // content manager

        private string[] soundAssets =
        {
            "fire_primary",
            "fire_secondary",
            "menu_cancel",
            "menu_change",
            "menu_select",
            "missile_explode",
            "powerup_get",
            "powerup_spawn",
            "shield_activate",
            "shield_collide",
            "ship_boost",
            "ship_collide",
            "ship_explode",
            "ship_spawn"
        };

        /// <summary>
        /// Create a new sound manager
        /// </summary>
        public SoundManager()
        {
        }

        /// <summary>
        /// Load resources
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            this.content = content;
            foreach (string asset in soundAssets)
            {
                sounds.Add(asset, content.Load<SoundEffect>($"sounds/{asset}"));
            }

        }

        /// <summary>
        /// Free resources
        /// </summary>
        public void UnloadContent()
        {
            foreach (string asset in soundAssets)
            {
                content.UnloadAsset($"sounds/{asset}");
            }
            foreach (SoundEffect sound in sounds.Values)
            {
                content.UnloadAsset(sound.Name);
                sound.Dispose();
            }
            sounds.Clear();
        }

        /// <summary>
        /// Play a sound in 2D
        /// </summary>
        public void PlaySound(String soundName)
        {
            if (sounds.TryGetValue(soundName, out SoundEffect soundEffect))
            {
                soundEffect.Play();
            }
        }

        /// <summary>
        /// Play a sound in 3D at given position 
        /// (just fake 3D using distance attenuation but no stereo)
        /// </summary>
        public void PlaySound3D(String soundName, float distance)
        {
            if (sounds.TryGetValue(soundName, out SoundEffect soundEffect))
            {
                float volume = Math.Max(0.0f, 1.0f - distance / 1000.0f);
                soundEffect.Play(volume, 0.0f, 0.0f);
            }
        }

        #region IDisposable Members

        bool isDisposed = false;
        public bool IsDisposed
        {
            get { return isDisposed; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposing && !isDisposed)
            {
                if (content != null)
                {
                    content.Dispose();
                    content = null;
                }
            }
        }
        #endregion
    }
}
