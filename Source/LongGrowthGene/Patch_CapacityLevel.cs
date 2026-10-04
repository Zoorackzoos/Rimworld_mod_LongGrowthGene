using HarmonyLib;
using RimWorld;
using Verse;

namespace LongGrowthGene
{
    /// <summary>
    /// Capacities (consciousness, sight, ...) are not stats, so they are scaled here:
    /// after the game computes a capacity level, multiply it by the gene's percentage.
    /// </summary>
    [HarmonyPatch(typeof(PawnCapacityUtility), nameof(PawnCapacityUtility.CalculateCapacityLevel))]
    public static class Patch_CapacityLevel
    {
        private static int loggedHits;

        public static void Postfix(HediffSet diffSet, PawnCapacityDef capacity, ref float __result)
        {
            if (diffSet == null || capacity == null || __result <= 0f)
            {
                return;
            }
            if (!LongGrowthRules.CapacityDefNames.Contains(capacity.defName))
            {
                return;
            }
            Gene_LongGrowth gene = Gene_LongGrowth.For(diffSet.pawn);
            if (gene == null)
            {
                return;
            }

            float before = __result;
            __result = before * LongGrowthMath.CapacityMultiplier(gene);

            if (loggedHits < 10)
            {
                loggedHits++;
                LGLog.Verbose("Capacity patch hit #" + loggedHits + ": " + gene.PawnName() + " " + capacity.defName
                    + " " + before.ToString("F2") + " -> " + __result.ToString("F2"));
            }
        }
    }
}
