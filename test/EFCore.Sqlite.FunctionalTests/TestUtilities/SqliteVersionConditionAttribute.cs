// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class SqliteVersionConditionAttribute : Attribute, ITestCondition
    {
        private Version _min;
        private Version _max;
        private Version _skip;

        public string Min
        {
            get => _min.ToString();
            set => _min = new Version(value);
        }

        public string Max
        {
            get => _max.ToString();
            set => _max = new Version(value);
        }

        public string Skip
        {
            get => _skip.ToString();
            set => _skip = new Version(value);
        }

        private static Version Current
        {
            get
            {
                var connection = new SqliteConnection("Data Source=:memory:;");
                return connection.ServerVersion != null ? new Version(connection.ServerVersion) : null;
            }
        }

        public ValueTask<bool> IsMetAsync()
        {
            if (Current == _skip)
            {
                return new ValueTask<bool>(false);
            }

            if (_min == null
                && _max == null)
            {
                return new ValueTask<bool>(true);
            }

            if (_min == null)
            {
                return new ValueTask<bool>(Current <= _max);
            }

            return new ValueTask<bool>(_max == null ? Current >= _min : Current <= _max && Current >= _min);
        }

        private string _skipReason;

        public string SkipReason
        {
            set => _skipReason = value;
            get => _skipReason
                ?? $"Test only runs for SQLite versions >= {Min ?? "Any"} and <= {Max ?? "Any"}"
                + (Skip == null ? "" : "and skipping on " + Skip);
        }
    }
}
