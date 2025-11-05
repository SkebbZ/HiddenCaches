using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using System.Collections;
using EFT.Interactive;

namespace RaiRai.HiddenCaches
{
    [BepInPlugin("com.rairai.hiddencaches.eft", "HiddenCaches", "1.3.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        internal static ConfigEntry<Color> configColor;
        internal static ConfigEntry<bool> configAudio;
        internal static ConfigEntry<bool> configLight;
        internal static ConfigEntry<bool> configSmoke;

        private const bool enabled = true;

        private void Awake()
        {
            Log = base.Logger;
            Log.LogInfo("Loading plugin HiddenCaches...");
            try
            {
                InitConfig();

                new CachePatch().Enable();
                // UPDATED: Re-enabled the RaidEndPatch for proper cleanup.
                new RaidEndPatch().Enable();
            }
            catch (Exception ex)
            {
                Log.LogError(ex.ToString());
            }
            Log.LogInfo("Loaded plugin HiddenCaches!");
        }

        private void InitConfig()
        {
            configAudio = Config.Bind("Toggles", "Audio", enabled, new ConfigDescription("Enable sound effect.", null, new ConfigurationManagerAttributes { Order = 4 }));
            configLight = Config.Bind("Toggles", "Light", enabled, new ConfigDescription("Enable light effect.", null, new ConfigurationManagerAttributes { Order = 3 }));
            configSmoke = Config.Bind("Toggles", "Smoke", enabled, new ConfigDescription("Enable smoke effect.", null, new ConfigurationManagerAttributes { Order = 2 }));
            configColor = Config.Bind("Color", "Color", new Color(1.0f, 0.375f, 0.0f), new ConfigDescription("Color of the effects.", null, new ConfigurationManagerAttributes { Order = 1 }));

            Config.Bind("Color", "Apply", "", new ConfigDescription("Apply color and toggle changes in-raid.", null, new ConfigurationManagerAttributes { Order = 0, HideDefaultButton = true, CustomDrawer = new Action<ConfigEntryBase>(ApplyDrawer) }));
        }
        private void ApplyDrawer(ConfigEntryBase configEntry)
        {
            if (GUILayout.Button("Apply Changes In-Raid", GUILayout.ExpandWidth(true)))
            {
                if (CachePatch.hiddenCacheList != null)
                {
                    Color chosenColor = new Color(configColor.Value.r * 2, configColor.Value.g * 2, configColor.Value.b * 2);
                    StartCoroutine(UpdateColors(chosenColor));
                }
            }
        }

        private IEnumerator UpdateColors(Color chosenColor)
        {
            foreach (LootableContainer container in CachePatch.hiddenCacheList)
            {
                if (container == null) continue;

                var audioSource = container.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.enabled = configAudio.Value;
                    if (configAudio.Value && !audioSource.isPlaying) audioSource.Play();
                }

                var componentLightObject = container.GetComponent<Light>();
                if (componentLightObject != null)
                {
                    componentLightObject.color = chosenColor;
                    componentLightObject.enabled = configLight.Value;
                }

                var particleSystem = container.GetComponent<ParticleSystem>();
                var componentParticleSysRenderer = container.GetComponent<ParticleSystemRenderer>();
                if (componentParticleSysRenderer != null && particleSystem != null)
                {
                    componentParticleSysRenderer.enabled = configSmoke.Value;
                    componentParticleSysRenderer.material.SetColor("_TintColor", chosenColor);
                    if (configSmoke.Value && !particleSystem.isPlaying) particleSystem.Play();
                }
            }
            yield return null;
        }
    }
}