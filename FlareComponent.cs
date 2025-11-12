using UnityEngine;
using System.Collections;

namespace RaiRai.HiddenCaches
{
    internal class FlareComponent : MonoBehaviour
    {
        // Start is no longer a coroutine.
        public void Start()
        {
            // This will run our new, simple, one-time search for effects.
            BundleLoader.PopulateComponents();

            // If, after the search, the assets are still null, stop.
            if (BundleLoader.material == null)
            {
                // We no longer log an error here, because BundleLoader already did.
                return;
            }

            // --- The rest of the code is identical and will now work ---

            Color chosenColor = (Plugin.configColor?.Value ?? new Color(1f, 0.375f, 0f)) * 2f;

            Light lightObject = this.GetOrAddComponent<Light>();
            lightObject.Reset();
            lightObject.color = chosenColor;
            lightObject.range = 3f;
            lightObject.enabled = Plugin.configLight?.Value ?? true;

            // Only add the audio source if a clip was successfully found
            if (BundleLoader.audioClip != null)
            {
                AudioSource audioSource = this.GetOrAddComponent<AudioSource>();
                audioSource.clip = BundleLoader.audioClip;
                audioSource.loop = true;
                audioSource.maxDistance = 8;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                audioSource.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
                audioSource.spatialBlend = 1f;
                audioSource.volume = 0.182f;
                audioSource.enabled = Plugin.configAudio?.Value ?? true;
                audioSource.Play();
            }

            ParticleSystem particleSystem = this.GetOrAddComponent<ParticleSystem>();
            ParticleSystem.MainModule mainModule = particleSystem.main;
            ParticleSystem.EmissionModule emissionModule = particleSystem.emission;
            emissionModule.rateOverTime = 15f;
            mainModule.gravityModifier = -0.01f;
            mainModule.maxParticles = 150;
            particleSystem.time = 0.4902f;
            mainModule.startColor = new Color(1, 1, 1, 0.2f);
            mainModule.startLifetime = 10;
            mainModule.startSpeed = 3;
            mainModule.startSize = 1;
            mainModule.scalingMode = ParticleSystemScalingMode.Shape;
            mainModule.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetimeModule = particleSystem.colorOverLifetime;
            colorOverLifetimeModule.enabled = true;
            if (BundleLoader.particleSystem != null)
            {
                colorOverLifetimeModule.color = BundleLoader.particleSystem.colorOverLifetime.color;
            }

            ParticleSystem.ShapeModule shapeModule = particleSystem.shape;
            shapeModule.radius = 0.01f;

            ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule = particleSystem.textureSheetAnimation;
            textureSheetAnimationModule.enabled = true;
            textureSheetAnimationModule.numTilesX = 8;
            textureSheetAnimationModule.numTilesY = 8;

            ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityModule = particleSystem.limitVelocityOverLifetime;
            limitVelocityModule.enabled = true;
            limitVelocityModule.dampen = 1f;
            limitVelocityModule.limitMultiplier = 0.4f;

            mainModule.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;

            ParticleSystemRenderer particleSystemRenderer = this.GetOrAddComponent<ParticleSystemRenderer>();
            particleSystemRenderer.enableGPUInstancing = false;
            particleSystemRenderer.maxParticleSize = 20f;
            particleSystemRenderer.receiveShadows = true;
            particleSystemRenderer.material = BundleLoader.material!;
            particleSystemRenderer.material.SetColor("_LocalMinimalAmbientLight", new Color(1f, 1f, 1f, 1f));
            particleSystemRenderer.material.SetColor("_TintColor", chosenColor);
            particleSystemRenderer.enabled = Plugin.configSmoke?.Value ?? true;

            particleSystem.Play();
        }
    }
}