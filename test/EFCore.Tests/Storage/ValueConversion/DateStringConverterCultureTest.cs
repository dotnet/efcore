// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

// th-TH uses the Thai Buddhist calendar by default (year + 543), which exposes converters that format
// with the current culture but parse with the invariant culture.
public class DateStringConverterCultureTest
{
    [Fact, UseCulture("th-TH")]
    public void DateOnly_to_string_round_trips_under_non_Gregorian_culture()
    {
        var converter = new DateOnlyToStringConverter();
        var toProvider = converter.ConvertToProviderExpression.Compile();
        var fromProvider = converter.ConvertFromProviderExpression.Compile();

        var value = new DateOnly(1973, 9, 3);
        var stored = toProvider(value);

        Assert.Equal("1973-09-03", stored);
        Assert.Equal(value, fromProvider(stored));
    }

    [Fact, UseCulture("th-TH")]
    public void DateTime_to_string_round_trips_under_non_Gregorian_culture()
    {
        var converter = new DateTimeToStringConverter();
        var toProvider = converter.ConvertToProviderExpression.Compile();
        var fromProvider = converter.ConvertFromProviderExpression.Compile();

        var value = new DateTime(1973, 9, 3, 12, 34, 56);
        var stored = toProvider(value);

        Assert.Equal("1973-09-03 12:34:56", stored);
        Assert.Equal(value, fromProvider(stored));
    }

    [Fact, UseCulture("th-TH")]
    public void DateTimeOffset_to_string_round_trips_under_non_Gregorian_culture()
    {
        var converter = new DateTimeOffsetToStringConverter();
        var toProvider = converter.ConvertToProviderExpression.Compile();
        var fromProvider = converter.ConvertFromProviderExpression.Compile();

        var value = new DateTimeOffset(1973, 9, 3, 12, 34, 56, TimeSpan.FromHours(2));
        var stored = toProvider(value);

        Assert.Equal("1973-09-03 12:34:56+02:00", stored);
        Assert.Equal(value, fromProvider(stored));
    }
}
