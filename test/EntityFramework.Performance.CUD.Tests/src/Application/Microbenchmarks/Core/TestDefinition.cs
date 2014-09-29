namespace Microbenchmarks.Core
{
    using System;

    public abstract class TestDefinitionBase
    {
        public string TestName { get; set; }
        public Action Setup { get; set; }
        public Action Cleanup { get; set; }
    }

    public class TestDefinition : TestDefinitionBase
    {
        public int? IterationCount { get; set; }
        public int? WarmupCount { get; set; }
        public Action Run { get; set; }
    }

    public class ThreadedTestDefinition : TestDefinitionBase
    {
        public int? ThreadCount { get; set; }
        public int? WarmupDuration { get; set; }
        public int? TestDuration { get; set; }
        public Action<object> Run { get; set; }
        public Func<object> ThreadStateFactory { get; set; }
    }

    public class AsyncTestDefinition : ThreadedTestDefinition
    {
    }
}
