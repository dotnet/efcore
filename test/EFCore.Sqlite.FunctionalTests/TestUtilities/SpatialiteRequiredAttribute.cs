// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class SpatialiteRequiredAttribute : Attribute, ITestCondition
{
    private static readonly Lazy<bool> _loaded
        = new(
            () =>
            {
                using var connection = new SqliteConnection("Data Source=:memory:");
                return SpatialiteLoader.TryLoad(connection);
            });

    public ValueTask<bool> IsMetAsync()
        => new(_loaded.Value);

    public string SkipReason
        => "mod_spatialite not found. Install it to run this test.";
}
