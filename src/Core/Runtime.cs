using System.Diagnostics;

namespace sodoffmmo.Core;
public static class Runtime {
    static Runtime() {
        var currentProcess = Process.GetCurrentProcess();
        lastSystemTime = (long)(DateTime.Now - currentProcess.StartTime).TotalMilliseconds;
        currentProcess.Dispose();
        stopwatch = new Stopwatch(); stopwatch.Start();
    }

    private static long lastSystemTime;
    private static Stopwatch stopwatch;

    public static long CurrentRuntime {
        get {
            return stopwatch.ElapsedMilliseconds + lastSystemTime;
        }
    }
}
