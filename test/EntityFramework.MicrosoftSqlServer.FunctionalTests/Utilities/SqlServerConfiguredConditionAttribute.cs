// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests.TestUtilities.Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class SqlServerConfiguredConditionAttribute : Attribute, ITestCondition
    {
        public bool IsMet => SqlServerTestStore.IsConfigured;

        public string SkipReason => "No test SQL Server has been configured.";
    }
}
