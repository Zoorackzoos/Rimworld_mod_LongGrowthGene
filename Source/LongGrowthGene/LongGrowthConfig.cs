namespace LongGrowthGene
{
    /// <summary>
    /// Every tunable number lives here. Edit and rebuild.
    /// "Percent" is the growth value: it starts at StartPercent and rises by GainPerDay each in-game day.
    /// Each group stops growing at its own cap.
    /// </summary>
    public static class LongGrowthConfig
    {
        // ---- Growth curve ----
        public const float StartPercent = 80f;   // percent on day 0
        public const float GainPerDay = 1f;      // percentage points gained per in-game day

        // ---- Caps (in percent) per group ----
        public const float CapCapacities = 500f;     // consciousness, sight, hearing, moving, manipulation, ...
        public const float CapStats = 500f;          // higher-is-better stats (carrying capacity, melee hit, ...)
        public const float CapLowerIsBetter = 500f;  // mental break threshold, aiming time, ranged cooldown (value = x 100/percent)
        public const float CapNeedRates = 500f;      // hunger / sleep / recreation drain rate (value = x 100/percent)

        // ---- Resistances (flat bonuses that grow from 0 at day 0 up to the max) ----
        public const float ResistanceMaxBonus = 0.60f;        // +0.60 armor rating sharp/blunt and toxic resistance (= 60%)
        public const float ResistanceFullAtPercent = 500f;    // percent value at which resistances reach the max above
        public const float TemperatureMaxBonusC = 40f;        // widens comfortable temperature range by up to this many degrees C each way

        // ---- Debugging ----
        // true = extra [LongGrowthGene] lines in the dev log (Log.Message). Set false when you are done testing.
        public static bool VerboseLogging = true;
    }
}
