using System.Diagnostics;

namespace Sim
{
    public class Timer
    {
        private static Stopwatch _globalStopwatch = Stopwatch.StartNew();
        private static Stopwatch _stopwatch = Stopwatch.StartNew();
        private static long elapsedMs;

        public static float ElapsedSeconds { get; private set; }

        public static long GetTime()
        {
            return _globalStopwatch.ElapsedMilliseconds;
        }

        public static void Update()
        {
            elapsedMs = _stopwatch.ElapsedTicks;
            ElapsedSeconds = ((float)elapsedMs/1000000);
            _stopwatch.Restart();
        }
    }
}
    