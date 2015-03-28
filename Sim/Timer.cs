using System.Diagnostics;

namespace Sim
{
    public class Timer
    {
        private static Stopwatch _globalStopwatch = Stopwatch.StartNew();
        private static Stopwatch _stopwatch = Stopwatch.StartNew();

        public static double ElapsedSeconds { get; private set; }

        public static long GetTime()
        {
            return _globalStopwatch.ElapsedMilliseconds;
        }

        public static void Update()
        {        
            ElapsedSeconds = _stopwatch.Elapsed.TotalSeconds;// ((float)elapsedMs / 1000000);
            _stopwatch.Restart();
        }
    }
}
    