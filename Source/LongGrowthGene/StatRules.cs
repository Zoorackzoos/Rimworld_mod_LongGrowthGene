using System.Collections.Generic;
using UnityEngine;

namespace LongGrowthGene
{
    internal enum RuleMode
    {
        Multiply,          // value x (percent / 100)          higher is better
        MultiplyInverse,   // value x (100 / percent)          lower is better
        AddResistance,     // value + bonus that grows to ResistanceMaxBonus
        AddColdTolerance,  // value - degrees (ComfyTemperatureMin goes down)
        AddHeatTolerance   // value + degrees (ComfyTemperatureMax goes up)
    }

    internal sealed class StatRule
    {
        public readonly string DefName;
        public readonly RuleMode Mode;
        public readonly float Cap;

        public StatRule(string defName, RuleMode mode, float cap)
        {
            DefName = defName;
            Mode = mode;
            Cap = cap;
        }
    }

    internal static class LongGrowthRules
    {
        // Capacities are handled by the Harmony patch (Patch_CapacityLevel).
        public static readonly HashSet<string> CapacityDefNames = new HashSet<string>
        {
            "Consciousness",
            "Sight",
            "Hearing",
            "Moving",
            "Manipulation",
            "Talking",
            "Breathing",
            "BloodFiltration",
            "BloodPumping"
        };

        // Stats are handled by StatPart_LongGrowth, attached at startup (see LongGrowthStartup).
        // Melee DPS is deliberately NOT here.
        public static readonly StatRule[] StatRules = new StatRule[]
        {
            // Higher is better
            new StatRule("PainShockThreshold",   RuleMode.Multiply, LongGrowthConfig.CapStats),
            new StatRule("PsychicSensitivity",   RuleMode.Multiply, LongGrowthConfig.CapStats),
            new StatRule("CarryingCapacity",     RuleMode.Multiply, LongGrowthConfig.CapStats),
            new StatRule("ImmunityGainSpeed",    RuleMode.Multiply, LongGrowthConfig.CapStats),
            new StatRule("InjuryHealingFactor",  RuleMode.Multiply, LongGrowthConfig.CapStats),
            new StatRule("MeleeHitChance",       RuleMode.Multiply, LongGrowthConfig.CapStats),
            new StatRule("MeleeDodgeChance",     RuleMode.Multiply, LongGrowthConfig.CapStats),
            new StatRule("ShootingAccuracyPawn", RuleMode.Multiply, LongGrowthConfig.CapStats),
            new StatRule("SocialImpact",         RuleMode.Multiply, LongGrowthConfig.CapStats),
            new StatRule("ConversionPower",      RuleMode.Multiply, LongGrowthConfig.CapStats),

            // Lower is better
            new StatRule("MentalBreakThreshold", RuleMode.MultiplyInverse, LongGrowthConfig.CapLowerIsBetter),
            new StatRule("AimingDelayFactor",    RuleMode.MultiplyInverse, LongGrowthConfig.CapLowerIsBetter),
            new StatRule("RangedCooldownFactor", RuleMode.MultiplyInverse, LongGrowthConfig.CapLowerIsBetter),

            // Need drain rates (lower is better)
            new StatRule("RestFallRateFactor",   RuleMode.MultiplyInverse, LongGrowthConfig.CapNeedRates),
            new StatRule("JoyFallRateFactor",    RuleMode.MultiplyInverse, LongGrowthConfig.CapNeedRates),

            // Resistances (flat bonuses)
            new StatRule("ArmorRating_Sharp",    RuleMode.AddResistance,    0f),
            new StatRule("ArmorRating_Blunt",    RuleMode.AddResistance,    0f),
            new StatRule("ToxicResistance",      RuleMode.AddResistance,    0f),
            new StatRule("ComfyTemperatureMin",  RuleMode.AddColdTolerance, 0f),
            new StatRule("ComfyTemperatureMax",  RuleMode.AddHeatTolerance, 0f)
        };
    }

    internal static class LongGrowthMath
    {
        public static float Percent(Gene_LongGrowth gene, float cap)
        {
            return Mathf.Min(gene.RawPercent, cap);
        }

        /// <summary>0 on day 0, 1 when ResistanceFullAtPercent is reached.</summary>
        public static float ResistProgress(Gene_LongGrowth gene)
        {
            float span = LongGrowthConfig.ResistanceFullAtPercent - LongGrowthConfig.StartPercent;
            if (span <= 0f)
            {
                return 1f;
            }
            return Mathf.Clamp01((gene.RawPercent - LongGrowthConfig.StartPercent) / span);
        }

        public static float Apply(StatRule rule, Gene_LongGrowth gene, float val)
        {
            switch (rule.Mode)
            {
                case RuleMode.Multiply:
                    return val * Percent(gene, rule.Cap) / 100f;
                case RuleMode.MultiplyInverse:
                    return val * 100f / Mathf.Max(Percent(gene, rule.Cap), 1f);
                case RuleMode.AddResistance:
                    return val + LongGrowthConfig.ResistanceMaxBonus * ResistProgress(gene);
                case RuleMode.AddColdTolerance:
                    return val - LongGrowthConfig.TemperatureMaxBonusC * ResistProgress(gene);
                case RuleMode.AddHeatTolerance:
                    return val + LongGrowthConfig.TemperatureMaxBonusC * ResistProgress(gene);
                default:
                    return val;
            }
        }

        public static string Describe(StatRule rule, Gene_LongGrowth gene)
        {
            string head = "Long growth gene (" + gene.RawPercent.ToString("F0") + "%): ";
            switch (rule.Mode)
            {
                case RuleMode.Multiply:
                    return head + "x" + (Percent(gene, rule.Cap) / 100f).ToString("F2");
                case RuleMode.MultiplyInverse:
                    return head + "x" + (100f / Mathf.Max(Percent(gene, rule.Cap), 1f)).ToString("F2");
                case RuleMode.AddResistance:
                    return head + "+" + (LongGrowthConfig.ResistanceMaxBonus * ResistProgress(gene) * 100f).ToString("F0") + "%";
                case RuleMode.AddColdTolerance:
                    return head + "-" + (LongGrowthConfig.TemperatureMaxBonusC * ResistProgress(gene)).ToString("F1") + " °C";
                case RuleMode.AddHeatTolerance:
                    return head + "+" + (LongGrowthConfig.TemperatureMaxBonusC * ResistProgress(gene)).ToString("F1") + " °C";
                default:
                    return null;
            }
        }

        /// <summary>Multiplier applied to a capacity level (consciousness, sight, ...).</summary>
        public static float CapacityMultiplier(Gene_LongGrowth gene)
        {
            return Percent(gene, LongGrowthConfig.CapCapacities) / 100f;
        }
    }
}
