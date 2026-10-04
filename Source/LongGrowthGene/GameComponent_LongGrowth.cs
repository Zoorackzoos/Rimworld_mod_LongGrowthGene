using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace LongGrowthGene
{
    /// <summary>
    /// Every 250 ticks, checks every pawn with the gene and refreshes capacity caches when a new day started.
    /// (Found and created automatically by RimWorld.)
    /// </summary>
    public class GameComponent_LongGrowth : GameComponent
    {
        private bool errorLogged;

        public GameComponent_LongGrowth(Game game)
        {
        }

        public override void GameComponentTick()
        {
            if (Find.TickManager.TicksGame % 250 != 0)
            {
                return;
            }

            try
            {
                List<Pawn> pawns = PawnsFinder.AllMapsWorldAndTemporary_Alive;
                for (int i = 0; i < pawns.Count; i++)
                {
                    Gene_LongGrowth gene = Gene_LongGrowth.For(pawns[i]);
                    if (gene != null)
                    {
                        gene.Refresh(false);
                    }
                }
            }
            catch (Exception e)
            {
                if (!errorLogged)
                {
                    errorLogged = true;
                    LGLog.Error("Daily refresh failed (logged once): " + e);
                }
            }
        }
    }
}
