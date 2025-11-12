using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EFT.Interactive;

namespace RaiRai.HiddenCaches
{
    [BepInPlugin("com.rairai.hiddencaches.eft", "HiddenCaches", "1.3.1")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource? Log { get; private set; }

        internal static ConfigEntry<Color>? configColor;
        internal static ConfigEntry<bool>? configAudio;
        internal static ConfigEntry<bool>? configLight;
        internal static ConfigEntry<bool>? configSmoke;

        private const bool defaultEnabled = true;

        // Reuse a single MaterialPropertyBlock to avoid creating materials or allocations
        private static readonly MaterialPropertyBlock s_property_block = new MaterialPropertyBlock();
        private static readonly int TintColorId = Shader.PropertyToID("_TintColor");
        private const int BatchSize = 50; // number of items to process before yielding a frame

        private void Awake()
        {
            Log = base.Logger;
            Log?.LogInfo("Loading plugin HiddenCaches...");
            try
            {
                InitConfig();

                new CachePatch().Enable();
                // UPDATED: Re-enabled the RaidEndPatch for proper cleanup.
                new RaidEndPatch().Enable();
            }
            catch (Exception ex)
            {
                Log?.LogError(ex.ToString());
            }
            Log?.LogInfo("Loaded plugin HiddenCaches!");
        }

        private void InitConfig()
        {
            configAudio = Config.Bind("Toggles", "Audio", defaultEnabled, new ConfigDescription("Enable sound effect.", null, new ConfigurationManagerAttributes { Order = 4 }));
            configLight = Config.Bind("Toggles", "Light", defaultEnabled, new ConfigDescription("Enable light effect.", null, new ConfigurationManagerAttributes { Order = 3 }));
            configSmoke = Config.Bind("Toggles", "Smoke", defaultEnabled, new ConfigDescription("Enable smoke effect.", null, new ConfigurationManagerAttributes { Order = 2 }));
            configColor = Config.Bind("Color", "Color", new Color(1.0f, 0.375f, 0.0f), new ConfigDescription("Color of the effects.", null, new ConfigurationManagerAttributes { Order = 1 }));

            Config.Bind("Color", "Apply", "", new ConfigDescription("Apply color and toggle changes in-raid.", null, new ConfigurationManagerAttributes { Order = 0, HideDefaultButton = true, CustomDrawer = new Action<ConfigEntryBase>(ApplyDrawer) }));
        }
        private void ApplyDrawer(ConfigEntryBase configEntry)
        {
            if (GUILayout.Button("Apply Changes In-Raid", GUILayout.ExpandWidth(true)))
            {
                if (CachePatch.hiddenCacheList != null)
                {
                    // Compute chosen color once and multiply by scalar efficiently
                    Color chosenColor = configColor!.Value * 2f;
                    StartCoroutine(UpdateColors(chosenColor));
                }
            }
        }

        private IEnumerator UpdateColors(Color chosenColor)
        {
            var list = CachePatch.hiddenCacheList;
            if (list == null) yield break;

            int processed = 0;

            for (int i = 0; i < list.Count; i++)
            {
                LootableContainer container = list[i];
                if (container == null) continue;

                // Audio
                if (container.TryGetComponent<AudioSource>(out var audioSource))
                {
                    bool shouldEnableAudio = configAudio!.Value;
                    audioSource.enabled = shouldEnableAudio;
                    if (shouldEnableAudio && !audioSource.isPlaying)
                    {
                        audioSource.Play();
                    }
                }

                // Light
                if (container.TryGetComponent<Light>(out var light))
                {
                    // Only assign if different to avoid unnecessary work
                    if (light.color != chosenColor)
                        light.color = chosenColor;

                    light.enabled = configLight!.Value;
                }

                // Particle system and renderer
                if (container.TryGetComponent<ParticleSystem>(out var particleSystem) && container.TryGetComponent<ParticleSystemRenderer>(out var psRenderer))
                {
                    psRenderer.enabled = configSmoke!.Value;

                    // Use MaterialPropertyBlock to avoid instancing the renderer materials
                    s_property_block.Clear();
                    s_property_block.SetColor(TintColorId, chosenColor);
                    psRenderer.SetPropertyBlock(s_property_block);

                    if (configSmoke!.Value && !particleSystem.isPlaying)
                        particleSystem.Play();
                }

                processed++;
                if (processed >= BatchSize)
                {
                    processed = 0;
                    // yield to next frame to avoid hitches when many containers exist
                    yield return null;
                }
            }

            yield return null;
        }
    }
}