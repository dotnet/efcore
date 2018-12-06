// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class SpatialiteRequiredAttribute : Attribute, ITestCondition
    {
        private static readonly Lazy<bool> _loaded
            = new Lazy<bool>(
                () =>
                {
                    using (var connection = new SqliteConnection("Data Source=:memory:"))
                    {
                        connection.Open();
                        connection.EnableExtensions();

                        return SpatialiteLoader.TryLoad(connection);
                    }
                });

        public bool IsMet
            => _loaded.Value;

        public string SkipReason
            => "mod_spatialite not found. Install it to run this test.";
    }
}
