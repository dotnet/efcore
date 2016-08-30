// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    [TraitDiscoverer("Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities.SqlServerConditionTraitDiscoverer", "Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests")]
    public class SqlServerConditionAttribute : Attribute, ITestCondition, ITraitAttribute
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
                if (Conditions.HasFlag(SqlServerCondition.SupportsMemoryOptimized))
                {
                    isMet &= TestEnvironment.GetFlag(nameof(SqlServerCondition.SupportsMemoryOptimized)) ?? true;
                }
                if (Conditions.HasFlag(SqlServerCondition.IsSqlAzure))
                {
                    isMet &= TestEnvironment.DefaultConnection.Contains("database.windows.net");
                }
                if (Conditions.HasFlag(SqlServerCondition.IsNotSqlAzure))
                {
                    isMet &= !TestEnvironment.DefaultConnection.Contains("database.windows.net");
                }
                return isMet;
            }
        }

        public string SkipReason =>
            // ReSharper disable once UseStringInterpolation
            string.Format("The test SQL Server does not meet these conditions: '{0}'",
                string.Join(", ", Enum.GetValues(typeof(SqlServerCondition))
                    .Cast<Enum>()
                    .Where(f => Conditions.HasFlag(f))
                    .Select(f => Enum.GetName(typeof(SqlServerCondition), f))));
    }

    [Flags]
    public enum SqlServerCondition
    {
        SupportsSequences = 1 << 0,
        SupportsOffset = 1 << 1,
        IsSqlAzure = 1 << 2,
        IsNotSqlAzure = 1 << 3,
        SupportsMemoryOptimized = 1 << 4
    }
}
