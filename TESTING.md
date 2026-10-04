# Long Growth Gene: build and test

## Build (Rider)
1. Open `Source/LongGrowthGene/LongGrowthGene.csproj` in Rider and let it restore NuGet packages.
   If `Krafs.Rimworld.Ref` 1.6 won't restore, use the fallback block in the .csproj (point it at your Assembly-CSharp.dll).
2. Build. The DLL lands in `1.6/Assemblies/LongGrowthGene.dll`.
3. Put the whole `LongGrowthGene` folder in `RimWorld/Mods/`.
4. Mod order: Harmony, Core, Biotech, Long Growth Gene.

## Test in game
1. Options > turn on Development mode. Start a new colony (or load a Biotech save).
2. Open the dev log (button at bottom-left in dev mode). On load you should see `[LongGrowthGene]` lines:
   Harmony patched, "Attached growth to N stats", "GeneDef 'LG_LongGrowth' found".
3. Top of the screen: Debug actions (bug icon) > category **Long Growth**:
   - Give gene (click), then click a colonist
   - **+1 day / +10 days / +100 days (click)** = this is how you advance the "days as a pawn" value
   - Max out, Reset to day 0, Remove gene
   - **Log status (click)** prints every capacity and stat value for that pawn to the log
4. Check results: open the pawn's info card (the "i" button) > Stats, hover a stat; the explanation shows
   "Long growth gene (143%): x1.43". Health tab shows capacities.
5. Natural growth: just let days pass (speed 4). 60,000 ticks = 1 day.

## Where to change numbers
`Source/LongGrowthGene/LongGrowthConfig.cs`: start percent, gain per day, every cap, resistance max (0.60), temperature bonus, verbose logging.
`Source/LongGrowthGene/StatRules.cs`: which stats are affected and whether each is higher-is-better or lower-is-better.

## If something breaks
Copy the red errors and any `[LongGrowthGene]` lines from the dev log. Most likely trouble spots:
- a stat name that doesn't exist (logged as a warning and skipped)
- the capacity patch (logged at startup under "Harmony patched")
