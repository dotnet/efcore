namespace Microbenchmarks.Core
{
    public class IterationCounterBase
    {
        public long WorkingSet { get; set; }
    }
    public class IterationCounter : IterationCounterBase
    {
        public long ElapsedMillis { get; set; }
    }

    public class ThreadedIterationCounter : IterationCounterBase
    {
        public long RequestsPerSecond { get; set; }
    }
}
