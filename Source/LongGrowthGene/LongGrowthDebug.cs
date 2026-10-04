using System.Collections.Generic;
using System.Text;
using LudeonTK;
using RimWorld;
using Verse;

namespace LongGrowthGene
{
    /// <summary>
    /// Dev-mode tools. Turn on Dev mode in the game options, then open the Debug actions menu
    /// (the bug icon at the top of the screen) and look for the "Long Growth" category.
    /// Tools marked (click) let you click a pawn on the map.
    /// </summary>
    [HasDebugOutput]
    public static class LongGrowthDebug
    {
        [DebugAction("Long Growth", "Give gene (click)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void GiveGene(Pawn p)
        {
            if (p.genes == null)
            {
                Say(p, "This pawn has no gene tracker (not a Biotech humanlike).");
                return;
            }
            if (Gene_LongGrowth.For(p) != null)
            {
                Say(p, "Already has the gene.");
                return;
            }
            GeneDef def = DefDatabase<GeneDef>.GetNamedSilentFail(LongGrowthStartup.GeneDefName);
            if (def == null)
            {
                LGLog.Error("GeneDef not found.");
                return;
            }
            p.genes.AddGene(def, false);
            Say(p, "Gave long growth gene.");
        }

        [DebugAction("Long Growth", "Remove gene (click)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void RemoveGene(Pawn p)
        {
            Gene_LongGrowth g = Gene_LongGrowth.For(p);
            if (g == null)
            {
                Say(p, "No gene to remove.");
                return;
            }
            p.genes.RemoveGene(g);
            Say(p, "Removed long growth gene.");
        }

        [DebugAction("Long Growth", "+1 day (click)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AddOneDay(Pawn p)
        {
            AddDays(p, 1);
        }

        [DebugAction("Long Growth", "+10 days (click)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AddTenDays(Pawn p)
        {
            AddDays(p, 10);
        }

        [DebugAction("Long Growth", "+100 days (click)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AddHundredDays(Pawn p)
        {
            AddDays(p, 100);
        }

        [DebugAction("Long Growth", "Max out (click)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void MaxOut(Pawn p)
        {
            Gene_LongGrowth g = Gene_LongGrowth.For(p);
            if (g == null)
            {
                Say(p, "No gene.");
                return;
            }
            float target = LongGrowthConfig.CapStats;
            if (LongGrowthConfig.CapCapacities > target) target = LongGrowthConfig.CapCapacities;
            if (LongGrowthConfig.CapLowerIsBetter > target) target = LongGrowthConfig.CapLowerIsBetter;
            if (LongGrowthConfig.CapNeedRates > target) target = LongGrowthConfig.CapNeedRates;
            if (LongGrowthConfig.ResistanceFullAtPercent > target) target = LongGrowthConfig.ResistanceFullAtPercent;

            float missing = target - g.RawPercent;
            if (missing <= 0f || LongGrowthConfig.GainPerDay <= 0f)
            {
                Say(p, "Already at max.");
                return;
            }
            int days = (int)(missing / LongGrowthConfig.GainPerDay) + 1;
            g.AddDays(days);
            Say(p, "Maxed out: now day " + g.DaysGrown + ", " + g.RawPercent.ToString("F0") + "%.");
        }

        [DebugAction("Long Growth", "Reset to day 0 (click)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ResetDay(Pawn p)
        {
            Gene_LongGrowth g = Gene_LongGrowth.For(p);
            if (g == null)
            {
                Say(p, "No gene.");
                return;
            }
            g.ResetGrowth();
            Say(p, "Reset to day 0.");
        }

        [DebugAction("Long Growth", "Log status (click)", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void LogStatus(Pawn p)
        {
            Gene_LongGrowth g = Gene_LongGrowth.For(p);
            if (g == null)
            {
                Say(p, "No gene.");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Status for " + p.LabelShort + ": day " + g.DaysGrown + ", raw growth " + g.RawPercent.ToString("F1") + "%");

            sb.AppendLine("-- Capacities --");
            foreach (string name in LongGrowthRules.CapacityDefNames)
            {
                PawnCapacityDef cap = DefDatabase<PawnCapacityDef>.GetNamedSilentFail(name);
                if (cap == null)
                {
                    sb.AppendLine(name + ": (def missing)");
                    continue;
                }
                sb.AppendLine(name + ": " + p.health.capacities.GetLevel(cap).ToString("P0"));
            }

            sb.AppendLine("-- Stats --");
            foreach (StatRule rule in LongGrowthRules.StatRules)
            {
                StatDef stat = DefDatabase<StatDef>.GetNamedSilentFail(rule.DefName);
                if (stat == null)
                {
                    sb.AppendLine(rule.DefName + ": (def missing)");
                    continue;
                }
                sb.AppendLine(rule.DefName + ": " + p.GetStatValue(stat).ToString("F3") + "   [" + LongGrowthMath.Describe(rule, g) + "]");
            }

            Log.Message("[LongGrowthGene] " + sb);
            Messages.Message("Long Growth status for " + p.LabelShort + " written to the dev log.", p, MessageTypeDefOf.NeutralEvent, false);
        }

        [DebugAction("Long Growth", "Log all gene pawns", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void LogAll()
        {
            List<Pawn> pawns = PawnsFinder.AllMapsWorldAndTemporary_Alive;
            int count = 0;
            for (int i = 0; i < pawns.Count; i++)
            {
                Gene_LongGrowth g = Gene_LongGrowth.For(pawns[i]);
                if (g != null)
                {
                    count++;
                    LGLog.Info(pawns[i].LabelShort + ": day " + g.DaysGrown + ", " + g.RawPercent.ToString("F0") + "%");
                }
            }
            LGLog.Info("Pawns with the gene: " + count);
        }

        private static void AddDays(Pawn p, int days)
        {
            Gene_LongGrowth g = Gene_LongGrowth.For(p);
            if (g == null)
            {
                Say(p, "No gene. Use 'Give gene' first.");
                return;
            }
            g.AddDays(days);
            Say(p, "Day " + g.DaysGrown + ", " + g.RawPercent.ToString("F0") + "%.");
        }

        private static void Say(Pawn p, string text)
        {
            LGLog.Info(p.LabelShort + ": " + text);
            Messages.Message("Long Growth: " + text, p, MessageTypeDefOf.NeutralEvent, false);
        }
    }
}
