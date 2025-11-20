// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;
using static System.Linq.Expressions.Expression;

// ReSharper disable AssignNullToNotNullAttribute

namespace Microsoft.EntityFrameworkCore.Query;

public class ExpressionEqualityComparerTest
{
    [ConditionalFact]
    public void Member_init_expressions_are_compared_correctly()
    {
        var expressionComparer = ExpressionEqualityComparer.Instance;

        var addMethod = typeof(List<string>).GetTypeInfo().GetDeclaredMethod("Add");

        var bindingMessages = ListBind(
            typeof(Node).GetProperty("Messages"),
            ElementInit(addMethod, Constant("Constant1")));

        var bindingDescriptions = ListBind(
            typeof(Node).GetProperty("Descriptions"),
            ElementInit(addMethod, Constant("Constant2")));

        Expression e1 = MemberInit(
            New(typeof(Node)),
            new List<MemberBinding> { bindingMessages });

        Expression e2 = MemberInit(
            New(typeof(Node)),
            new List<MemberBinding> { bindingMessages, bindingDescriptions });

        Assert.NotEqual(expressionComparer.GetHashCode(e1), expressionComparer.GetHashCode(e2));
        Assert.False(expressionComparer.Equals(e1, e2));
        Assert.Equal(expressionComparer.GetHashCode(e1), expressionComparer.GetHashCode(e1));
        Assert.True(expressionComparer.Equals(e1, e1));
    }

    [ConditionalFact]
    public void Default_expressions_are_compared_correctly()
    {
        var expressionComparer = ExpressionEqualityComparer.Instance;

        Expression e1 = Default(typeof(int));
        Expression e2 = Default(typeof(int));
        Expression e3 = Default(typeof(string));

        Assert.Equal(expressionComparer.GetHashCode(e1), expressionComparer.GetHashCode(e2));
        Assert.NotEqual(expressionComparer.GetHashCode(e1), expressionComparer.GetHashCode(e3));
        Assert.True(expressionComparer.Equals(e1, e2));
        Assert.False(expressionComparer.Equals(e1, e3));
        Assert.Equal(expressionComparer.GetHashCode(e1), expressionComparer.GetHashCode(e1));
        Assert.True(expressionComparer.Equals(e1, e1));
    }

    [ConditionalFact]
    public void Index_expressions_are_compared_correctly()
    {
        var expressionComparer = ExpressionEqualityComparer.Instance;

        var param = Parameter(typeof(Indexable));
        var prop = typeof(Indexable).GetProperty("Item");
        var e1 = MakeIndex(param, prop, [Constant(1)]);
        var e2 = MakeIndex(param, prop, [Constant(2)]);
        var e3 = MakeIndex(param, prop, [Constant(2)]);

        Assert.Equal(ExpressionType.Index, e1.NodeType);
        Assert.NotNull(e1.Indexer);
        Assert.Equal(expressionComparer.GetHashCode(e1), expressionComparer.GetHashCode(e1));
        Assert.True(expressionComparer.Equals(e1, e1));
        Assert.NotEqual(expressionComparer.GetHashCode(e1), expressionComparer.GetHashCode(e2));
        Assert.False(expressionComparer.Equals(e1, e2));
        Assert.Equal(expressionComparer.GetHashCode(e2), expressionComparer.GetHashCode(e3));
        Assert.True(expressionComparer.Equals(e2, e3));

        param = Parameter(typeof(int[]));
        e1 = ArrayAccess(param, Constant(1));
        e2 = ArrayAccess(param, Constant(2));
        e3 = ArrayAccess(param, Constant(2));

        Assert.Equal(ExpressionType.Index, e1.NodeType);
        Assert.Null(e1.Indexer);
        Assert.Equal(expressionComparer.GetHashCode(e1), expressionComparer.GetHashCode(e1));
        Assert.True(expressionComparer.Equals(e1, e1));
        Assert.NotEqual(expressionComparer.GetHashCode(e1), expressionComparer.GetHashCode(e2));
        Assert.False(expressionComparer.Equals(e1, e2));
        Assert.Equal(expressionComparer.GetHashCode(e2), expressionComparer.GetHashCode(e3));
        Assert.True(expressionComparer.Equals(e2, e3));
    }

    [ConditionalFact]
    public void Array_constant_expressions_are_compared_correctly()
    {
        var expressionComparer = ExpressionEqualityComparer.Instance;

        var e1 = Constant(new[] { 1, 2, 3 });
        var e2 = Constant(new[] { 1, 2, 3 });
        var e3 = Constant(new[] { 1, 2, 4 });

        Assert.True(expressionComparer.Equals(e1, e2));
        Assert.False(expressionComparer.Equals(e1, e3));

        Assert.Equal(expressionComparer.GetHashCode(e1), expressionComparer.GetHashCode(e2));
        Assert.NotEqual(expressionComparer.GetHashCode(e1), expressionComparer.GetHashCode(e3));
    }

    [ConditionalFact] // #30697
    public void Lambda_parameters_names_are_taken_into_account()
    {
        var expressionComparer = ExpressionEqualityComparer.Instance;

        Expression<Func<int, int>> lambda1 = x => 1;
        Expression<Func<int, int>> lambda2 = x => 1;
        Expression<Func<int, int>> lambda3 = y => 1;

        Assert.True(expressionComparer.Equals(lambda1, lambda2));
        Assert.False(expressionComparer.Equals(lambda1, lambda3));

        Assert.Equal(expressionComparer.GetHashCode(lambda1), expressionComparer.GetHashCode(lambda2));
        Assert.NotEqual(expressionComparer.GetHashCode(lambda1), expressionComparer.GetHashCode(lambda3));
    }

    private class Node
    {
        [UsedImplicitly]
        public List<string> Messages { set; get; }

        [UsedImplicitly]
        public List<string> Descriptions { set; get; }
    }

    private class Indexable
    {
        public int this[int index]
            => 0;
    }
}
