// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class SqliteVersionConditionAttribute : Attribute, ITestCondition
{
    private Version? _min;
    private Version? _max;
    private Version? _skip;

    public string? Min
    {
        get => _min?.ToString();
        set => _min = value is null ? null : new Version(value);
    }

    public string? Max
    {
        get => _max?.ToString();
        set => _max = value is null ? null : new Version(value);
    }

    public string? Skip
    {
        get => _skip?.ToString();
        set => _skip = value is null ? null : new Version(value);
    }

    private static Version? Current
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
            return ValueTask.FromResult(false);
        }

        if (_min == null
            && _max == null)
        {
            return ValueTask.FromResult(true);
        }

        if (_min == null)
        {
            return ValueTask.FromResult(Current <= _max);
        }

        return ValueTask.FromResult(_max == null ? Current >= _min : Current <= _max && Current >= _min);
    }

    private string? _skipReason;

    public string SkipReason
    {
        set => _skipReason = value;
        get => _skipReason
            ?? $"Test only runs for SQLite versions >= {Min ?? "Any"} and <= {Max ?? "Any"}"
            + (Skip == null ? "" : "and skipping on " + Skip);
    }
}
