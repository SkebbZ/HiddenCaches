using SPT.Reflection.Patching;
using Comfort.Common;
using EFT;
using System.Reflection;
using HarmonyLib;

namespace RaiRai.HiddenCaches
{
    internal class RaidEndPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(EFT.Player), "OnDestroy");
        }

        [PatchPrefix]
        private static void Cleanup(EFT.Player __instance)
        {
            // UPDATED: Use the null-conditional operator (?.) to safely access MainPlayer.
            // This prevents the NullReferenceException if GameWorld has already been destroyed.
            if (Singleton<GameWorld>.Instance?.MainPlayer?.Id == __instance.Id)
            {
                Plugin.Log?.LogInfo("Raid ended. Cleaning up HiddenCaches assets.");
                BundleLoader.material = null;
                BundleLoader.audioClip = null;
                BundleLoader.particleSystem = null;

                CachePatch.hiddenCacheList = null;
            }
        }
    }
}