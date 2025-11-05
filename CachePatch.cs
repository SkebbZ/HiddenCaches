using SPT.Reflection.Patching;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;

namespace RaiRai.HiddenCaches
{
    public class CachePatch : ModulePatch
    {
        internal static IEnumerable<EFT.Interactive.LootableContainer> hiddenCacheList;

        // --- UPDATED: Define the constant Template IDs for the caches ---
        private const string BarrelCacheTemplateId = "5d6d2bb386f774785b07a77a";
        private const string GroundCacheTemplateId = "5d6d2b5486f774785c2ba8ea";

        protected override MethodBase GetTargetMethod()
        {
            return typeof(EFT.GameWorld).GetMethod("OnGameStarted", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        private static void AddComponentToCaches()
        {
            Plugin.Log.LogInfo("Finding hidden caches by their Template ID...");

            if (hiddenCacheList != null)
            {
                hiddenCacheList = null;
            }

            // --- UPDATED: The Where clause now checks the Template ID instead of the object name. ---
            // This is a much more reliable and future-proof method.
            hiddenCacheList = UnityEngine.Object.FindObjectsOfType<EFT.Interactive.LootableContainer>()
                .Where(container => container.Template == BarrelCacheTemplateId || container.Template == GroundCacheTemplateId);

            // Convert to a list to safely get the count and iterate.
            var foundCaches = hiddenCacheList.ToList();
            hiddenCacheList = foundCaches; // Assign the list back to the static variable for the config manager.
            int count = foundCaches.Count;

            if (count == 0)
            {
                Plugin.Log.LogWarning("Found 0 hidden caches. This is unexpected. The Template IDs may have changed.");
            }
            else
            {
                Plugin.Log.LogInfo($"Found {count} hidden caches. Applying flare component...");
                foreach (var lootableContainer in foundCaches)
                {
                    lootableContainer.GetOrAddComponent<FlareComponent>();
                }
                Plugin.Log.LogInfo("Finished applying flare component to caches.");
            }
        }
    }
}