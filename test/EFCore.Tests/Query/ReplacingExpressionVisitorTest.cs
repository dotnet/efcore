// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class ReplacingExpressionVisitorTest
{
    // A plain constructor-bound type deliberately NOT covered by the fold: the constructor-parameter-to-property
    // correspondence for arbitrary named types is a convention, not a guarantee (see PositionalDto/TransformingDto
    // below), so ReplacingExpressionVisitor intentionally leaves member access on these unfolded.
    private class PositionalDto(int pickupStatusId, int count)
    {
        public int PickupStatusId { get; } = pickupStatusId;
        public int Count { get; } = count;
    }

    // Demonstrates why the fold can't safely be generalized beyond ValueTuple/Tuple: this legal, unremarkable C#
    // redeclares a positional-looking property with a transforming initializer. Via reflection it is
    // indistinguishable from a genuine passthrough (both are compiler-generated auto-properties), so a name-based
    // fold would silently return the wrong (untransformed) value here.
    private class TransformingDto(int value)
    {
        public int Value { get; } = value + 1;
    }

    [Fact]
    public void Member_access_on_ValueTuple_folds_to_constructor_argument()
    {
        var parameter = Expression.Parameter(typeof(int), "k");
        var newExpression = Expression.New(
            typeof(ValueTuple<int, int>).GetConstructors().Single(),
            parameter, Expression.Constant(1));

        var memberAccess = Expression.Field(newExpression, nameof(ValueTuple<int, int>.Item1));

        var result = ReplacingExpressionVisitor.Replace(parameter, Expression.Constant(42), memberAccess);

        Assert.Equal(Expression.Constant(42).ToString(), result.ToString());
    }

    [Fact]
    public void Member_access_on_Tuple_folds_to_constructor_argument()
    {
        var parameter = Expression.Parameter(typeof(int), "k");
        var newExpression = Expression.New(
            typeof(Tuple<int, int>).GetConstructors().Single(),
            parameter, Expression.Constant(1));

        var memberAccess = Expression.Property(newExpression, nameof(Tuple<int, int>.Item1));

        var result = ReplacingExpressionVisitor.Replace(parameter, Expression.Constant(42), memberAccess);

        Assert.Equal(Expression.Constant(42).ToString(), result.ToString());
    }

    [Fact]
    public void EFProperty_access_on_ValueTuple_folds_to_constructor_argument()
    {
        var parameter = Expression.Parameter(typeof(int), "k");
        var newExpression = Expression.New(
            typeof(ValueTuple<int, int>).GetConstructors().Single(),
            parameter, Expression.Constant(1));

        var efPropertyMethod = typeof(EF).GetMethod(nameof(EF.Property))!.MakeGenericMethod(typeof(int));
        var efPropertyAccess = Expression.Call(
            efPropertyMethod,
            Expression.Convert(newExpression, typeof(object)),
            Expression.Constant(nameof(ValueTuple<int, int>.Item1)));

        var result = ReplacingExpressionVisitor.Replace(parameter, Expression.Constant(42), efPropertyAccess);

        Assert.Equal(Expression.Constant(42).ToString(), result.ToString());
    }

    [Fact]
    public void EFProperty_access_on_Tuple_folds_to_constructor_argument()
    {
        // Unlike ValueTuple, Tuple is a reference type: passing it where EF.Property<T> expects object needs no
        // boxing Convert node, so this exercises the "already unwrapped" branch rather than the boxing-unwrap path
        // the ValueTuple version above covers.
        var parameter = Expression.Parameter(typeof(int), "k");
        var newExpression = Expression.New(
            typeof(Tuple<int, int>).GetConstructors().Single(),
            parameter, Expression.Constant(1));

        var efPropertyMethod = typeof(EF).GetMethod(nameof(EF.Property))!.MakeGenericMethod(typeof(int));
        var efPropertyAccess = Expression.Call(
            efPropertyMethod,
            newExpression,
            Expression.Constant(nameof(Tuple<int, int>.Item1)));

        var result = ReplacingExpressionVisitor.Replace(parameter, Expression.Constant(42), efPropertyAccess);

        Assert.Equal(Expression.Constant(42).ToString(), result.ToString());
    }

    [Fact]
    public void Member_access_on_default_constructed_ValueTuple_does_not_throw()
    {
        // NewExpression.Constructor is null for a value type's parameterless "new T()" -- e.g. the C# compiler
        // produces exactly this NewExpression shape for "() => new ValueTuple<int, int>()". Like the anonymous-type
        // case, Members is also null here, so this must be checked before assuming a ValueTuple/Tuple always has a
        // constructor to fold against (regression test for a null-Constructor dereference).
        var newExpression = Expression.New(typeof(ValueTuple<int, int>));
        Assert.Null(newExpression.Constructor);
        Assert.Null(newExpression.Members);

        var memberAccess = Expression.Field(newExpression, nameof(ValueTuple<int, int>.Item1));
        var unrelatedOriginal = Expression.Parameter(typeof(int), "unused");
        var visited = ReplacingExpressionVisitor.Replace(unrelatedOriginal, Expression.Constant(-1), memberAccess);

        var resultMember = Assert.IsAssignableFrom<MemberExpression>(visited);
        Assert.Equal(nameof(ValueTuple<int, int>.Item1), resultMember.Member.Name);
    }

    [Fact]
    public void EFProperty_access_on_anonymous_type_under_conversion_folds_to_constructor_argument()
    {
        // Confirms the widened unwrap in the EF.Property path (added to support boxed ValueTuple instances) doesn't
        // regress the pre-existing anonymous-type fold when an anonymous type instance happens to be wrapped in an
        // (unnecessary, but legal) Convert-to-object node.
        var parameter = Expression.Parameter(typeof(int), "k");

        // Build an actual anonymous type via a lambda so NewExpression.Members is populated realistically.
        Expression<Func<int, object>> anonFactory = k => new { Value = k };
        var anonNew = (NewExpression)anonFactory.Body;
        var boxedAnon = Expression.Convert(anonNew.Update([parameter]), typeof(object));

        var efPropertyMethod = typeof(EF).GetMethod(nameof(EF.Property))!.MakeGenericMethod(typeof(int));
        var efPropertyAccess = Expression.Call(efPropertyMethod, boxedAnon, Expression.Constant("Value"));

        var result = ReplacingExpressionVisitor.Replace(parameter, Expression.Constant(42), efPropertyAccess);

        Assert.Equal(Expression.Constant(42).ToString(), result.ToString());
    }

    [Fact]
    public void Member_access_on_large_ValueTuple_including_Rest_folds_to_constructor_argument()
    {
        // Exercises the 8-arity ValueTuple (the "Rest"-nested form) to confirm the fixed item1..item7/rest
        // parameter-name contract is honored uniformly, not just for the common small arities.
        var parameters = Enumerable.Range(0, 7).Select(i => Expression.Parameter(typeof(int), $"p{i}")).ToArray();
        var restParameter = Expression.Parameter(typeof(ValueTuple<int>), "rest");
        var ctor = typeof(ValueTuple<int, int, int, int, int, int, int, ValueTuple<int>>).GetConstructors().Single();
        var newExpression = Expression.New(
            ctor, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5],
            parameters[6], restParameter);

        var item7Access = Expression.Field(newExpression, "Item7");
        var item7Result = ReplacingExpressionVisitor.Replace(parameters[6], Expression.Constant(42), item7Access);
        Assert.Equal(Expression.Constant(42).ToString(), item7Result.ToString());

        var restAccess = Expression.Field(newExpression, "Rest");
        var restReplacement = Expression.New(typeof(ValueTuple<int>).GetConstructors().Single(), Expression.Constant(99));
        var restResult = ReplacingExpressionVisitor.Replace(restParameter, restReplacement, restAccess);
        Assert.Equal(restReplacement.ToString(), restResult.ToString());
    }

    [Fact]
    public void Member_access_on_plain_constructor_bound_type_does_not_fold()
    {
        // The core assertion for the narrowed design: an ordinary (non-ValueTuple/Tuple) constructor-bound type
        // must be left unfolded, even though its constructor parameter names conventionally match its property
        // names -- because that correspondence isn't a guarantee for arbitrary user types (see TransformingDto).
        var newExpression = Expression.New(
            typeof(PositionalDto).GetConstructors().Single(),
            Expression.Constant(1), Expression.Constant(2));

        var memberAccess = Expression.Property(newExpression, nameof(PositionalDto.PickupStatusId));
        var unrelatedOriginal = Expression.Parameter(typeof(int), "unused");
        var visited = ReplacingExpressionVisitor.Replace(unrelatedOriginal, Expression.Constant(-1), memberAccess);

        var resultMember = Assert.IsAssignableFrom<MemberExpression>(visited);
        Assert.Equal(nameof(PositionalDto.PickupStatusId), resultMember.Member.Name);
        Assert.Same(newExpression, resultMember.Expression);
    }

    [Fact]
    public void Member_access_on_transforming_constructor_bound_type_does_not_fold_to_wrong_value()
    {
        // Concretely demonstrates the risk the narrowed design avoids: if this type WERE folded by name (as a
        // general-purpose implementation might do), it would incorrectly return the raw constructor argument (41)
        // instead of the real, transformed property value (42). Because it isn't ValueTuple/Tuple, it isn't folded
        // at all, so this can't happen -- the member access is left in place and would be evaluated normally
        // (or fail translation loudly), never silently returning the wrong data.
        var newExpression = Expression.New(
            typeof(TransformingDto).GetConstructors().Single(),
            Expression.Constant(41));

        var memberAccess = Expression.Property(newExpression, nameof(TransformingDto.Value));
        var unrelatedOriginal = Expression.Parameter(typeof(int), "unused");
        var visited = ReplacingExpressionVisitor.Replace(unrelatedOriginal, Expression.Constant(-1), memberAccess);

        Assert.IsAssignableFrom<MemberExpression>(visited);
        Assert.Equal(42, new TransformingDto(41).Value);
    }
}
