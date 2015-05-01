// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace EntityFramework.Microbenchmarks.Core
{
    public class RunResult
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
            IterationCounters = new List<IterationCounter>(iterationCounters);
            ReportedException = null;
        }

        public string TestName { get; set; }
        public long WorkingSet { get; set; }
        public Exception ReportedException { get; set; }
        public ICollection<IterationCounter> IterationCounters { get; set; }
        public long ElapsedMillis { get; set; }

        public bool Successful
        {
            get { return ReportedException == null; }
        }
    }
}
