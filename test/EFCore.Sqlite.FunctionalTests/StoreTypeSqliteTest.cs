// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Numerics;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore;

public class StoreTypeSqliteTest : StoreTypeRelationalTestBase
{
    #region Disable ulong for JSON partial updates

    protected override async Task TestExecuteUpdateWithinJsonToParameter<T>(
        ContextFactory<DbContext> contextFactory,
        T value,
        T otherValue,
        Func<T, T, bool> comparer)
    {
        if (typeof(T) == typeof(ulong))
        {
            // See #36689
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                base.TestExecuteUpdateWithinJsonToParameter(contextFactory, value, otherValue, comparer));
            Assert.Equal(SqliteStrings.ExecuteUpdateJsonPartialUpdateDoesNotSupportUlong, exception.Message);
            return;
        }

        await base.TestExecuteUpdateWithinJsonToParameter(contextFactory, value, otherValue, comparer);
    }

    protected override async Task TestExecuteUpdateWithinJsonToConstant<T>(
        ContextFactory<DbContext> contextFactory,
        T value,
        T otherValue,
        Func<T, T, bool> comparer)
    {
        if (typeof(T) == typeof(ulong))
        {
            // See #36689
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                base.TestExecuteUpdateWithinJsonToConstant(contextFactory, value, otherValue, comparer));
            Assert.Equal(SqliteStrings.ExecuteUpdateJsonPartialUpdateDoesNotSupportUlong, exception.Message);
            return;
        }

        await base.TestExecuteUpdateWithinJsonToConstant(contextFactory, value, otherValue, comparer);
    }

    protected override async Task TestExecuteUpdateWithinJsonToJsonProperty<T>(
        ContextFactory<DbContext> contextFactory,
        T value,
        T otherValue,
        Func<T, T, bool> comparer)
    {
        if (typeof(T) == typeof(ulong))
        {
            // See #36689
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                base.TestExecuteUpdateWithinJsonToJsonProperty(contextFactory, value, otherValue, comparer));
            Assert.Equal(SqliteStrings.ExecuteUpdateJsonPartialUpdateDoesNotSupportUlong, exception.Message);
            return;
        }

        await base.TestExecuteUpdateWithinJsonToJsonProperty(contextFactory, value, otherValue, comparer);
    }

    protected override async Task TestExecuteUpdateWithinJsonToNonJsonColumn<T>(
        ContextFactory<DbContext> contextFactory,
        T value,
        T otherValue,
        Func<T, T, bool> comparer)
    {
        if (typeof(T) == typeof(ulong))
        {
            // See #36689
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                base.TestExecuteUpdateWithinJsonToNonJsonColumn(contextFactory, value, otherValue, comparer));
            Assert.Equal(SqliteStrings.ExecuteUpdateJsonPartialUpdateDoesNotSupportUlong, exception.Message);
            return;
        }

        if (typeof(T) == typeof(string)
            || typeof(T) == typeof(bool)
            || typeof(T).GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INumber<>)))
        {
            await base.TestExecuteUpdateWithinJsonToNonJsonColumn(contextFactory, value, otherValue, comparer);
        }
        else
        {
            // See #36688 for supporting this for relational types other than string/numeric/bool
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.TestExecuteUpdateWithinJsonToNonJsonColumn(contextFactory, value, otherValue, comparer));
            Assert.Equal(RelationalStrings.ExecuteUpdateCannotSetJsonPropertyToNonJsonColumn, exception.Message);
        }
    }

    #endregion Disable ulong for JSON partial updates

    protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
}
