// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore;

public class ValueConvertersEndToEndInMemoryTest(ValueConvertersEndToEndInMemoryTest.ValueConvertersEndToEndInMemoryFixture fixture)
    : ValueConvertersEndToEndTestBase<ValueConvertersEndToEndInMemoryTest.ValueConvertersEndToEndInMemoryFixture>(fixture)
{
    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Query_with_converter_and_null_check(bool async) // Issue #29603
    {
        using (var context = CreateContext())
        {
            var set = context.Set<ConvertingEntity>();
            List<int>? nullList = null;

            if (async)
            {
                await set.CountAsync(p => p.NullableListOfInt != nullList && p.NullableListOfInt!.Count > 0);
                await set.CountAsync(p => p.NullableListOfInt != null && p.NullableListOfInt.Count > 0);
            }
            else
            {
                set.Count(p => p.NullableListOfInt != nullList && p.NullableListOfInt!.Count > 0);
                set.Count(p => p.NullableListOfInt != null && p.NullableListOfInt.Count > 0);
            }
        }
    }

    public class ValueConvertersEndToEndInMemoryFixture : ValueConvertersEndToEndFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
