// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class OperatorsQuerySqliteTest : OperatorsQueryTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    public override async Task Bitwise_and_on_expression_with_like_and_null_check_being_compared_to_false()
    {
        await base.Bitwise_and_on_expression_with_like_and_null_check_being_compared_to_false();

        AssertSql("");
    }

    public override async Task Complex_predicate_with_bitwise_and_modulo_and_negation()
    {
        await base.Complex_predicate_with_bitwise_and_modulo_and_negation();

        AssertSql("");
    }

    public override async Task Complex_predicate_with_bitwise_and_arithmetic_operations()
    {
        await base.Complex_predicate_with_bitwise_and_arithmetic_operations();

        AssertSql("");
    }

    public override async Task Or_on_two_nested_binaries_and_another_simple_comparison()
    {
        await base.Or_on_two_nested_binaries_and_another_simple_comparison();

        AssertSql(
            """
SELECT "o"."Id" AS "Id1", "o0"."Id" AS "Id2", "o1"."Id" AS "Id3", "o2"."Id" AS "Id4", "o3"."Id" AS "Id5"
FROM "OperatorEntityString" AS "o"
CROSS JOIN "OperatorEntityString" AS "o0"
CROSS JOIN "OperatorEntityString" AS "o1"
CROSS JOIN "OperatorEntityString" AS "o2"
CROSS JOIN "OperatorEntityInt" AS "o3"
WHERE ("o"."Value" = 'A' AND "o"."Value" IS NOT NULL AND "o0"."Value" = 'A' AND "o0"."Value" IS NOT NULL) | ("o1"."Value" = 'B' AND "o1"."Value" IS NOT NULL AND "o2"."Value" = 'B' AND "o2"."Value" IS NOT NULL) AND "o3"."Value" = 2
ORDER BY "o"."Id", "o0"."Id", "o1"."Id", "o2"."Id", "o3"."Id"
""");
    }

    public override async Task Projection_with_not_and_negation_on_integer()
    {
        await base.Projection_with_not_and_negation_on_integer();

        AssertSql(
            """
SELECT ~(-(-("o1"."Value" + "o"."Value" + 2))) % (-("o0"."Value" + "o0"."Value") - "o"."Value")
FROM "OperatorEntityLong" AS "o"
CROSS JOIN "OperatorEntityLong" AS "o0"
CROSS JOIN "OperatorEntityLong" AS "o1"
ORDER BY "o"."Id", "o0"."Id", "o1"."Id"
""");
    }

    public override async Task Negate_on_column(bool async)
    {
        await base.Negate_on_column(async);

        AssertSql(
            """
SELECT "o"."Id"
FROM "OperatorEntityInt" AS "o"
WHERE "o"."Id" = -"o"."Value"
""");
    }

    public override async Task Double_negate_on_column()
    {
        await base.Double_negate_on_column();

        AssertSql(
            """
SELECT "o"."Id"
FROM "OperatorEntityInt" AS "o"
WHERE -(-"o"."Value") = "o"."Value"
""");
    }

    public override async Task Negate_on_binary_expression(bool async)
    {
        await base.Negate_on_binary_expression(async);

        AssertSql(
            """
SELECT "o"."Id" AS "Id1", "o0"."Id" AS "Id2"
FROM "OperatorEntityInt" AS "o"
CROSS JOIN "OperatorEntityInt" AS "o0"
WHERE -"o"."Value" = -("o"."Id" + "o0"."Value")
""");
    }

    public override async Task Negate_on_like_expression(bool async)
    {
        await base.Negate_on_like_expression(async);

        AssertSql(
            """
SELECT "o"."Id"
FROM "OperatorEntityString" AS "o"
WHERE "o"."Value" NOT LIKE 'A%' OR "o"."Value" IS NULL
""");
    }

    public override async Task Concat_and_json_scalar(bool async)
    {
        await base.Concat_and_json_scalar(async);

        AssertSql(
            """
SELECT "o"."Id", "o"."Owned"
FROM "Owner" AS "o"
WHERE 'Foo' || ("o"."Owned" ->> 'SomeProperty') = 'FooBar'
LIMIT 2
""");
    }
}
