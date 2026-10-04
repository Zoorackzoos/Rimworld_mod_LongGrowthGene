using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace LongGrowthGene
{
    [StaticConstructorOnStartup]
    public static class LongGrowthStartup
    {
        public const string GeneDefName = "LG_LongGrowth";

        static LongGrowthStartup()
        {
            LGLog.Info("Starting up.");

            try
            {
                Harmony harmony = new Harmony("zoorackzoos.longgrowthgene");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                int patched = 0;
                foreach (MethodBase m in harmony.GetPatchedMethods())
                {
                    patched++;
                    LGLog.Info("Harmony patched: " + m.DeclaringType + "." + m.Name);
                }
                if (patched == 0)
                {
                    LGLog.Error("Harmony patched NOTHING. Capacities will not scale.");
                }
            }
            catch (Exception e)
            {
                LGLog.Error("Harmony patching failed: " + e);
            }

            try
            {
                AttachStatParts();
            }
            catch (Exception e)
            {
                LGLog.Error("Attaching stat parts failed: " + e);
            }

            if (DefDatabase<GeneDef>.GetNamedSilentFail(GeneDefName) == null)
            {
                LGLog.Error("GeneDef '" + GeneDefName + "' was not found. Is Biotech active and the XML loaded?");
            }
            else
            {
                LGLog.Info("GeneDef '" + GeneDefName + "' found.");
            }
        }

        private static void AttachStatParts()
        {
            int attached = 0;
            int missing = 0;

            foreach (StatRule rule in LongGrowthRules.StatRules)
            {
                StatDef stat = DefDatabase<StatDef>.GetNamedSilentFail(rule.DefName);
                if (stat == null)
                {
                    missing++;
                    LGLog.Warn("StatDef '" + rule.DefName + "' does not exist in this game; skipped. (Normal for DLC-only stats if that DLC is off.)");
                    continue;
                }

                if (stat.parts == null)
                {
                    stat.parts = new List<StatPart>();
                }

                bool already = false;
                for (int i = 0; i < stat.parts.Count; i++)
                {
                    if (stat.parts[i] is StatPart_LongGrowth)
                    {
                        already = true;
                        break;
                    }
                }
                if (already)
                {
                    continue;
                }

                LGLog.Verbose("Stat " + rule.DefName + ": minValue=" + stat.minValue + ", maxValue=" + stat.maxValue + ", mode=" + rule.Mode);

                // A stat whose maximum is low would cap the bonus. Raise the ceiling for "higher is better" stats.
                if (rule.Mode == RuleMode.Multiply && stat.maxValue < 1000000f)
                {
                    LGLog.Info("Raising maxValue of " + rule.DefName + " from " + stat.maxValue + " to 1000000 so growth above the old cap works.");
                    stat.maxValue = 1000000f;
                }

                StatPart_LongGrowth part = new StatPart_LongGrowth(rule);
                part.Init(stat);
                stat.parts.Add(part);
                attached++;
            }

            LGLog.Info("Attached growth to " + attached + " stats (" + missing + " missing/skipped).");
        }
    }
}
