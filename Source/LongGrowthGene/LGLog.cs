using Verse;

namespace LongGrowthGene
{
    /// <summary>The C# version of System.out.println(): everything goes to the RimWorld dev log.</summary>
    internal static class LGLog
    {
        private const string Prefix = "[LongGrowthGene] ";

        public static void Info(string msg)
        {
            Log.Message(Prefix + msg);
        }

        public static void Verbose(string msg)
        {
            if (LongGrowthConfig.VerboseLogging)
            {
                Log.Message(Prefix + "(v) " + msg);
            }
        }

        public static void Warn(string msg)
        {
            Log.Warning(Prefix + msg);
        }

        public static void Error(string msg)
        {
            Log.Error(Prefix + msg);
        }
    }
}
