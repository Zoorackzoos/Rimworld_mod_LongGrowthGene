using RimWorld;
using Verse;

namespace LongGrowthGene
{
    /// <summary>
    /// Attached in code (not XML) to every stat listed in LongGrowthRules.StatRules.
    /// For pawns without the gene it does nothing.
    /// </summary>
    public class StatPart_LongGrowth : StatPart
    {
        private readonly StatRule rule;

        internal StatPart_LongGrowth(StatRule rule)
        {
            this.rule = rule;
        }

        internal void Init(StatDef stat)
        {
            parentStat = stat;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            Gene_LongGrowth gene = Gene_LongGrowth.For(req);
            if (gene == null)
            {
                return;
            }
            val = LongGrowthMath.Apply(rule, gene, val);
        }

        public override string ExplanationPart(StatRequest req)
        {
            Gene_LongGrowth gene = Gene_LongGrowth.For(req);
            if (gene == null)
            {
                return null;
            }
            return LongGrowthMath.Describe(rule, gene);
        }
    }
}
