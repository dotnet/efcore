// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microbenchmarks.Core
{
    public struct RunResult
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

        public RunResult(string testName, long elapsedMillis, long workingSet, ICollection<IterationCounter> iterationCounters)
        {
            TestName = testName;
            ElapsedMillis = elapsedMillis;
            WorkingSet = workingSet;
            IterationCounters = iterationCounters;
            ReportedException = null;
        }

        public bool Successful
        {
            get { return ReportedException == null; }
        }

        public string TestName;
        public long WorkingSet;
        public long ElapsedMillis;
        public Exception ReportedException;
        public ICollection<IterationCounter> IterationCounters;
    }
}
