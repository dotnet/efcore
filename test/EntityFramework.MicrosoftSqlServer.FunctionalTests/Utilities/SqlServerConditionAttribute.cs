// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests.TestUtilities.Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class SqlServerConditionAttribute : Attribute, ITestCondition
    {
        public SqlServerCondition Conditions { get; set; }

        public SqlServerConditionAttribute(SqlServerCondition conditions)
        {
            Conditions = conditions;
        }

        public bool IsMet
        {
            get
            {
                var isMet = true;
                if (Conditions.HasFlag(SqlServerCondition.SupportsSequences))
                {
                    isMet &= TestEnvironment.GetFlag(nameof(SqlServerCondition.SupportsSequences)) ?? true;
                }
                if (Conditions.HasFlag(SqlServerCondition.SupportsOffset))
                {
                    isMet &= TestEnvironment.GetFlag(nameof(SqlServerCondition.SupportsOffset)) ?? true;
                }
                return isMet;
            }
        }

        public string SkipReason =>
            string.Format("The test SQL Server does not meet these conditions: '{0}'"
                , string.Join(", ", Enum.GetValues(typeof(SqlServerCondition))
                    .Cast<Enum>()
                    .Where(f => Conditions.HasFlag(f))
                    .Select(f => Enum.GetName(typeof(SqlServerCondition), f))));
    }

    [Flags]
    public enum SqlServerCondition
    {
        SupportsSequences = 1 << 0,
        SupportsOffset = 1 << 1
    }
}
