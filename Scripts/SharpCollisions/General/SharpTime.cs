using FixMath.NET;

namespace SharpCollisions
{
    public static class SharpTime
    {
        private static int _ticksPerSecond;
        private static int _substeps;

        public static int TicksPerSecond => _ticksPerSecond;
        public static int Substeps => _substeps;
        public static int SubTicksPerSecond => _ticksPerSecond * _substeps;
        public static Fix64 DeltaTime => Fix64.One / (Fix64)_ticksPerSecond;
        public static Fix64 SubDelta => Fix64.One / (Fix64)SubTicksPerSecond;
        public static Fix64 TimeScale = Fix64.One;

        public static void Set(int tps, int steps)
        {
            _ticksPerSecond = tps;
            _substeps = steps;
        }
    }
}
