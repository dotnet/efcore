// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;

// ReSharper disable AssignNullToNotNullAttribute

namespace Microsoft.EntityFrameworkCore.Query;

public class ExpressionEqualityComparerTest
{
    [ConditionalFact]
    public void Member_init_expressions_are_compared_correctly()
    {
        var expressionComparer = ExpressionEqualityComparer.Instance;

        var addMethod = typeof(List<string>).GetTypeInfo().GetDeclaredMethod("Add");

        var bindingMessages = Expression.ListBind(
            typeof(Node).GetProperty("Messages"),
            Expression.ElementInit(addMethod, Expression.Constant("Constant1"))
        );

        var bindingDescriptions = Expression.ListBind(
            typeof(Node).GetProperty("Descriptions"),
            Expression.ElementInit(addMethod, Expression.Constant("Constant2"))
        );

        Expression e1 = Expression.MemberInit(
            Expression.New(typeof(Node)),
            new List<MemberBinding> { bindingMessages }
        );

        Expression e2 = Expression.MemberInit(
            Expression.New(typeof(Node)),
            new List<MemberBinding> { bindingMessages, bindingDescriptions }
        );

        Assert.NotEqual(expressionComparer.GetHashCode(e1), expressionComparer.GetHashCode(e2));
        Assert.False(expressionComparer.Equals(e1, e2));
        Assert.Equal(expressionComparer.GetHashCode(e1), expressionComparer.GetHashCode(e1));
        Assert.True(expressionComparer.Equals(e1, e1));
    }

    [ConditionalFact]
    public void Default_expressions_are_compared_correctly()
    {
        var expressionComparer = ExpressionEqualityComparer.Instance;

        Expression e1 = Expression.Default(typeof(int));
        Expression e2 = Expression.Default(typeof(int));
        Expression e3 = Expression.Default(typeof(string));

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

        var param = Expression.Parameter(typeof(Indexable));
        var prop = typeof(Indexable).GetProperty("Item");
        var e1 = Expression.MakeIndex(param, prop, new Expression[] { Expression.Constant(1) });
        var e2 = Expression.MakeIndex(param, prop, new Expression[] { Expression.Constant(2) });
        var e3 = Expression.MakeIndex(param, prop, new Expression[] { Expression.Constant(2) });

        Assert.Equal(ExpressionType.Index, e1.NodeType);
        Assert.NotNull(e1.Indexer);
        Assert.Equal(expressionComparer.GetHashCode(e1), expressionComparer.GetHashCode(e1));
        Assert.True(expressionComparer.Equals(e1, e1));
        Assert.NotEqual(expressionComparer.GetHashCode(e1), expressionComparer.GetHashCode(e2));
        Assert.False(expressionComparer.Equals(e1, e2));
        Assert.Equal(expressionComparer.GetHashCode(e2), expressionComparer.GetHashCode(e3));
        Assert.True(expressionComparer.Equals(e2, e3));

        param = Expression.Parameter(typeof(int[]));
        e1 = Expression.ArrayAccess(param, Expression.Constant(1));
        e2 = Expression.ArrayAccess(param, Expression.Constant(2));
        e3 = Expression.ArrayAccess(param, Expression.Constant(2));

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

        var e1 = Expression.Constant(new[] { 1, 2, 3 });
        var e2 = Expression.Constant(new[] { 1, 2, 3 });
        var e3 = Expression.Constant(new[] { 1, 2, 4 });

        Assert.True(expressionComparer.Equals(e1, e2));
        Assert.False(expressionComparer.Equals(e1, e3));

        Assert.Equal(expressionComparer.GetHashCode(e1), expressionComparer.GetHashCode(e2));
        Assert.NotEqual(expressionComparer.GetHashCode(e1), expressionComparer.GetHashCode(e3));
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
