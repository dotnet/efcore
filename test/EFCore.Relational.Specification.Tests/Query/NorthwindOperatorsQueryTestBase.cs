// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class NorthwindOperatorsQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    protected NorthwindOperatorsQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    protected static ExpressionType[] BinaryArithmeticOperators { get; } =
    {
        ExpressionType.Add,
        ExpressionType.Subtract,
        ExpressionType.Multiply,
        ExpressionType.Divide,
        // TODO Complete...
    };

    [ConditionalTheory]
    [MemberData(nameof(Get_binary_arithmetic_data))]
    public virtual async Task Binary_arithmetic_operators(ExpressionType outer, ExpressionType inner)
    {
        var parameter = Expression.Parameter(typeof(Order), "o");
        var predicate =
            Expression.Lambda<Func<Order, int>>(
                Expression.MakeBinary(
                    outer,
                    Expression.MakeBinary(
                        inner,
                        Expression.Property(parameter, nameof(Order.OrderID)),
                        Expression.Constant(8)),
                    Expression.Constant(9)),
                parameter);

        await AssertQueryScalar(
            async: true,
            ss => ss.Set<Order>().Where(o => o.OrderID == 10248).Select(predicate));
    }

    public static IEnumerable<object[]> Get_binary_arithmetic_data()
        => from op1 in BinaryArithmeticOperators from op2 in BinaryArithmeticOperators select new object[] { op1, op2 };

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Double_negate_on_column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => -(-o.OrderID) == o.OrderID),
            entryCount: 830);

    // TODO: Test associativity (no parentheses on consecutive e.g. add operations)
    // TODO: Test non-associativity of arithmetic operators on floating points aren't associative (because of rounding errors)

    // TODO: Move operator/precedence related here, e.g. NullSemanticsQueryTestBase.Bool_not_equal_nullable_int_HasValue,
    // GearsOfWarTestBase.Negate_on_binary_expression...
}
