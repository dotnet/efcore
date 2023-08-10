// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindFunctionsQueryInMemoryTest : NorthwindFunctionsQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
{
    public NorthwindFunctionsQueryInMemoryTest(
        NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture,
#pragma warning disable IDE0060 // Remove unused parameter
        ITestOutputHelper testOutputHelper)
#pragma warning restore IDE0060 // Remove unused parameter
        : base(fixture)
    {
        //TestLoggerFactory.TestOutputHelper = testOutputHelper;
    }

    public override Task Byte_Parse_Non_Numeric_Bad_Format(bool async)
        => Assert.ThrowsAsync<FormatException>(
            () => base.Byte_Parse_Non_Numeric_Bad_Format(async));

    public override Task Byte_Parse_Greater_Than_Max_Value_Overflows(bool async)
        => Assert.ThrowsAsync<OverflowException>(
            () => base.Byte_Parse_Greater_Than_Max_Value_Overflows(async));

    public override Task Byte_Parse_Negative_Overflows(bool async)
        => Assert.ThrowsAsync<OverflowException>(
            () => base.Byte_Parse_Negative_Overflows(async));

    public override Task Byte_Parse_Decimal_Bad_Format(bool async)
        => Assert.ThrowsAsync<FormatException>(
            () => base.Byte_Parse_Decimal_Bad_Format(async));

    public override Task Decimal_Parse_Non_Numeric_Bad_Format(bool async)
        => Assert.ThrowsAsync<FormatException>(
            () => base.Decimal_Parse_Non_Numeric_Bad_Format(async));

    public override Task Double_Parse_Non_Numeric_Bad_Format(bool async)
        => Assert.ThrowsAsync<FormatException>(
            () => base.Double_Parse_Non_Numeric_Bad_Format(async));

    public override Task Short_Parse_Non_Numeric_Bad_Format(bool async)
        => Assert.ThrowsAsync<FormatException>(
            () => base.Short_Parse_Non_Numeric_Bad_Format(async));

    public override Task Short_Parse_Greater_Than_Max_Value_Overflows(bool async)
        => Assert.ThrowsAsync<OverflowException>(
            () => base.Short_Parse_Greater_Than_Max_Value_Overflows(async));

    public override Task Short_Parse_Decimal_Bad_Format(bool async)
        => Assert.ThrowsAsync<FormatException>(
            () => base.Short_Parse_Decimal_Bad_Format(async));

    public override Task Int_Parse_Non_Numeric_Bad_Format(bool async)
        => Assert.ThrowsAsync<FormatException>(
            () => base.Int_Parse_Non_Numeric_Bad_Format(async));

    public override Task Int_Parse_Decimal_Bad_Format(bool async)
        => Assert.ThrowsAsync<FormatException>(
            () => base.Int_Parse_Decimal_Bad_Format(async));

    public override Task Long_Parse_Non_Numeric_Bad_Format(bool async)
        => Assert.ThrowsAsync<FormatException>(
            () => base.Long_Parse_Non_Numeric_Bad_Format(async));

    public override Task Long_Parse_Decimal_Bad_Format(bool async)
        => Assert.ThrowsAsync<FormatException>(
            () => base.Long_Parse_Decimal_Bad_Format(async));
}
