// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class DateTimeOffsetTranslationsInMemoryTest(BasicTypesQueryInMemoryFixture fixture)
    : DateTimeOffsetTranslationsTestBase<BasicTypesQueryInMemoryFixture>(fixture)
{
    // new DateTimeOffset(DateTime) with Unspecified kind uses the local timezone offset in .NET, which can overflow
    // for dates near year boundaries (e.g., year 0001 with a negative UTC offset). Databases treat this as UTC.
    public override Task Ctor_DateTime()
        => Task.CompletedTask;
}
