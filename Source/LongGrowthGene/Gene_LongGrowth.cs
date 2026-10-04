using RimWorld;
using Verse;

namespace LongGrowthGene
{
    /// <summary>
    /// The gene itself. It only remembers WHEN the pawn got it; the percentage is calculated from that,
    /// so it survives saving, loading and long absences without any per-tick work.
    /// </summary>
    public class Gene_LongGrowth : Gene
    {
        private int startTick = int.MinValue;   // game tick on which growth day 0 began
        private int lastAppliedDay = -1;        // last day number we refreshed caches for

        private static int NowTick
        {
            get { return Current.Game != null ? Find.TickManager.TicksGame : 0; }
        }

        private void EnsureStarted()
        {
            if (startTick == int.MinValue)
            {
                startTick = NowTick;
            }
        }

        /// <summary>Whole in-game days since the pawn got the gene (this is the "days as a pawn" value).</summary>
        public int DaysGrown
        {
            get
            {
                EnsureStarted();
                long diff = (long)NowTick - startTick;
                return diff <= 0 ? 0 : (int)(diff / GenDate.TicksPerDay);
            }
        }

        /// <summary>Uncapped percent: StartPercent + GainPerDay x days. Each stat group applies its own cap.</summary>
        public float RawPercent
        {
            get { return LongGrowthConfig.StartPercent + LongGrowthConfig.GainPerDay * DaysGrown; }
        }

        public override void PostAdd()
        {
            base.PostAdd();
            startTick = NowTick;
            lastAppliedDay = -1;
            LGLog.Info("Gene added to " + PawnName() + " at tick " + startTick + ". Growth starts at " + LongGrowthConfig.StartPercent + "%.");
            Refresh(true);
        }

        public override void PostRemove()
        {
            base.PostRemove();
            LGLog.Info("Gene removed from " + PawnName() + " (was day " + DaysGrown + ", " + RawPercent.ToString("F0") + "%).");
            if (pawn != null && pawn.health != null && pawn.health.capacities != null)
            {
                pawn.health.capacities.Notify_CapacityLevelsDirty();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref startTick, "lgStartTick", int.MinValue);
            Scribe_Values.Look(ref lastAppliedDay, "lgLastAppliedDay", -1);
        }

        /// <summary>Recalculates cached capacity levels when the day number changed (or when forced).</summary>
        public void Refresh(bool force)
        {
            int day = DaysGrown;
            if (!force && day == lastAppliedDay)
            {
                return;
            }
            lastAppliedDay = day;
            if (pawn != null && pawn.health != null && pawn.health.capacities != null)
            {
                pawn.health.capacities.Notify_CapacityLevelsDirty();
            }
            LGLog.Verbose(PawnName() + ": day " + day + ", growth " + RawPercent.ToString("F0") + "%");
        }

        // ---- Used by the debug actions ----

        public void AddDays(int days)
        {
            EnsureStarted();
            startTick -= days * GenDate.TicksPerDay;
            LGLog.Info(PawnName() + ": added " + days + " day(s). Now day " + DaysGrown + ", " + RawPercent.ToString("F0") + "%.");
            Refresh(true);
        }

        public void ResetGrowth()
        {
            startTick = NowTick;
            LGLog.Info(PawnName() + ": growth reset to day 0.");
            Refresh(true);
        }

        public string PawnName()
        {
            return pawn != null ? pawn.LabelShort : "(no pawn)";
        }

        // ---- Lookup helpers ----

        public static Gene_LongGrowth For(Pawn pawn)
        {
            if (pawn == null || pawn.genes == null)
            {
                return null;
            }
            var list = pawn.genes.GenesListForReading;
            for (int i = 0; i < list.Count; i++)
            {
                Gene_LongGrowth g = list[i] as Gene_LongGrowth;
                if (g != null && g.Active)
                {
                    return g;
                }
            }
            return null;
        }

        public static Gene_LongGrowth For(StatRequest req)
        {
            if (!req.HasThing)
            {
                return null;
            }
            return For(req.Thing as Pawn);
        }
    }
}
