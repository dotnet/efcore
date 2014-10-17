// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microbenchmarks.Core
{
    public abstract class RunResultBase
    {
        public string TestName;
        public long WorkingSet;
        public Exception ReportedException;
        public ICollection<IterationCounterBase> IterationCounters;
        public long ElapsedMillis;

        public bool Successful
        {
            get { return ReportedException == null; }
        }
    }

    public class RunResult : RunResultBase
    {
        public RunResult(string testName, long elapsedMillis, long workingSet)
        {
            TestName = testName;
            ElapsedMillis = elapsedMillis;
            WorkingSet = workingSet;
            IterationCounters = null;
            ReportedException = null;
        }

        public RunResult(string testName, Exception exception)
        {
            TestName = testName;
            ElapsedMillis = 0;
            WorkingSet = 0;
            IterationCounters = null;
            ReportedException = exception;
        }

        public RunResult(string testName, long elapsedMillis, long workingSet, IEnumerable<IterationCounter> iterationCounters)
        {
            TestName = testName;
            ElapsedMillis = elapsedMillis;
            WorkingSet = workingSet;
            IterationCounters = new List<IterationCounterBase>(iterationCounters);
            ReportedException = null;
        }
    }

    public class ThreadedRunResult : RunResultBase
    {
        public long RequestsPerSecond;

        public ThreadedRunResult(string testName, long requestsPerSecond, long workingSet)
        {
            TestName = testName;
            RequestsPerSecond = requestsPerSecond;
            WorkingSet = workingSet;
            IterationCounters = null;
            ReportedException = null;
        }

        public ThreadedRunResult(string testName, Exception exception)
        {
            TestName = testName;
            RequestsPerSecond = 0;
            WorkingSet = 0;
            IterationCounters = null;
            ReportedException = exception;
        }

        public ThreadedRunResult(string testName, long requestsPerSecond, long workingSet, IEnumerable<ThreadedIterationCounter> iterationCounters)
        {
            TestName = testName;
            RequestsPerSecond = requestsPerSecond;
            WorkingSet = workingSet;
            IterationCounters = new List<IterationCounterBase>(iterationCounters);
            ReportedException = null;
        }
    }
}
