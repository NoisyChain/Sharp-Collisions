using FixMath.NET;

namespace SharpCollisions
{
    public static class SharpTime
    {
        private static int _ticksPerSecond;
        private static int _substeps;
        private static int _frame;

        public static int TicksPerSecond => _ticksPerSecond;
        public static int Substeps => _substeps;
        public static int CurrentFrame => _frame;
        public static int SubTicksPerSecond => _ticksPerSecond * _substeps;
        public static Fix64 UnscaledDeltaTime => Fix64.One / (Fix64)_ticksPerSecond;
        public static Fix64 DeltaTime => UnscaledDeltaTime * TimeScale;
        public static Fix64 UnscaledSubDelta => Fix64.One / (Fix64)SubTicksPerSecond;
        public static Fix64 SubDelta => UnscaledSubDelta * TimeScale;
        public static Fix64 TimeScale;

        public static void Set(int tps, int steps)
        {
            _frame = 0;
            _ticksPerSecond = tps;
            _substeps = steps;
            TimeScale = Fix64.One;
        }

        public static void AdvanceFrame() => _frame++;
    }
}
