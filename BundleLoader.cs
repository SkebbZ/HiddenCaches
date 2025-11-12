using Comfort.Common;
using System;
using UnityEngine;
using EFT;
using System.Collections;
using System.Linq;

namespace RaiRai.HiddenCaches
{
    internal static class BundleLoader
    {
        internal static AudioClip? audioClip;
        internal static Material? material;
        internal static ParticleSystem? particleSystem;

        private static bool _hasBeenPopulated = false;

        // This is now a simple, one-time synchronous method. No more coroutines.
        internal static void PopulateComponents()
        {
            if (_hasBeenPopulated)
            {
                return;
            }

            Plugin.Log?.LogInfo("Attempting to find existing in-game effects to use...");

            // --- STRATEGY: Find an existing Particle System in the scene ---
            // We search for all ParticleSystemRenderers currently active in the game world.
            var allParticleRenderers = UnityEngine.Object.FindObjectsOfType<ParticleSystemRenderer>();
            if (allParticleRenderers.Any())
            {
                // We will grab the first one that has a valid material.
                var renderer = allParticleRenderers.FirstOrDefault(r => r.material != null);
                if (renderer != null)
                {
                    material = renderer.material;
                    particleSystem = renderer.GetComponent<ParticleSystem>();
                    Plugin.Log?.LogInfo($"Successfully borrowed particle effect from in-game object: {renderer.gameObject.name}");
                }
            }

            // --- STRATEGY: Find an existing looping Audio Source in the scene ---
            var allAudioSources = UnityEngine.Object.FindObjectsOfType<AudioSource>();
            if (allAudioSources.Any())
            {
                // We'll try to find a looping sound (like a fire crackle or generator hum)
                var audio = allAudioSources.FirstOrDefault(a => a.clip != null && a.loop);
                if (audio != null)
                {
                    audioClip = audio.clip;
                    Plugin.Log?.LogInfo($"Successfully borrowed looping audio clip from in-game object: {audio.gameObject.name}");
                }
            }

            // Final check
            if (material == null || particleSystem == null)
            {
                Plugin.Log?.LogError("Could not find any suitable particle effects in the current scene.");
            }
            if (audioClip == null)
            {
                Plugin.Log?.LogWarning("Could not find any suitable looping audio clips. Sound effects will be disabled.");
            }

            _hasBeenPopulated = true;
        }
    }
}