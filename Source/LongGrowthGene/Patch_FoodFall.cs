using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace LongGrowthGene
{
    /// <summary>
    /// Hunger has no StatDef for pawns, so scale the food fall rate directly.
    /// Lower is better: value x 100 / percent (same rule as sleep and recreation).
    /// </summary>
    [HarmonyPatch(typeof(Need_Food), nameof(Need_Food.FoodFallPerTickAssumingCategory))]
    public static class Patch_FoodFall
    {
        private static int loggedHits;

        // ___pawn (three underscores) injects Need's pawn field, whatever its visibility
        public static void Postfix(Pawn ___pawn, ref float __result)
        {
            Gene_LongGrowth gene = Gene_LongGrowth.For(___pawn);
            if (gene == null)
            {
                return;
            }

            float before = __result;
            float percent = Mathf.Max(LongGrowthMath.Percent(gene, LongGrowthConfig.CapNeedRates), 1f);
            __result = before * 100f / percent;

            if (loggedHits < 10)
            {
                loggedHits++;
                LGLog.Verbose("Food patch hit #" + loggedHits + ": " + gene.PawnName()
                              + " fall/tick " + before.ToString("F8") + " -> " + __result.ToString("F8"));
            }
        }
    }
}