// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

public class QueryableMethodNormalizingExpressionVisitorTest
{
    private sealed class KeyedCollection<TKey, TItem>(List<TItem> items) : ICollection<TItem>
    {
        public int Count => items.Count;
        public bool IsReadOnly => true;
        public void Add(TItem item) => throw new NotSupportedException();
        public void Clear() => throw new NotSupportedException();
        public bool Contains(TItem item) => items.Contains(item);
        public void CopyTo(TItem[] array, int arrayIndex) => items.CopyTo(array, arrayIndex);
        public bool Remove(TItem item) => throw new NotSupportedException();
        public IEnumerator<TItem> GetEnumerator() => items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class SpanSet<TSpan, T>(List<TSpan> items) : ICollection<TSpan>
    {
        public int Count => items.Count;
        public bool IsReadOnly => true;
        public void Add(TSpan item) => throw new NotSupportedException();
        public void Clear() => throw new NotSupportedException();
        public bool Contains(TSpan item) => items.Contains(item);
        public bool Contains(T value) => true;
        public void CopyTo(TSpan[] array, int arrayIndex) => items.CopyTo(array, arrayIndex);
        public bool Remove(TSpan item) => throw new NotSupportedException();
        public IEnumerator<TSpan> GetEnumerator() => items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class ReadOnlyKeyedCollection<TKey, TItem>(List<TItem> items) : IReadOnlyCollection<TItem>
    {
        public int Count => items.Count;
        public bool Contains(TItem item) => items.Contains(item);
        public IEnumerator<TItem> GetEnumerator() => items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class ReadOnlySpanSet<TSpan, T>(List<TSpan> items) : IReadOnlyCollection<TSpan>
    {
        public int Count => items.Count;
        public bool Contains(TSpan item) => items.Contains(item);
        public bool Contains(T value) => true;
        public IEnumerator<TSpan> GetEnumerator() => items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [Fact]
    public void Collection_Contains_normalized_for_custom_collection_with_multiple_generic_type_arguments()
    {
        var visitor = new QueryableMethodNormalizingExpressionVisitor(null!, isEfConstantSupported: false);
        var collection = new KeyedCollection<string, int>([1, 2]);
        var method = typeof(KeyedCollection<string, int>).GetMethod(nameof(KeyedCollection<string, int>.Contains), [typeof(int)])!;

        var expression = Expression.Call(Expression.Constant(collection), method, Expression.Constant(1));
        var result = visitor.Normalize(expression);

        var methodCall = Assert.IsAssignableFrom<MethodCallExpression>(result);
        Assert.Equal(QueryableMethods.Contains.MakeGenericMethod(typeof(int)), methodCall.Method);
    }

    [Fact]
    public void Collection_Contains_not_normalized_for_overload_not_matching_element_type()
    {
        var visitor = new QueryableMethodNormalizingExpressionVisitor(null!, isEfConstantSupported: false);
        var collection = new SpanSet<string, int>(["a", "b"]);
        var method = typeof(SpanSet<string, int>).GetMethod(nameof(SpanSet<string, int>.Contains), [typeof(int)])!;

        var expression = Expression.Call(Expression.Constant(collection), method, Expression.Constant(1));
        var result = visitor.Normalize(expression);

        var methodCall = Assert.IsAssignableFrom<MethodCallExpression>(result);
        Assert.Equal(method, methodCall.Method);
    }

    [Fact]
    public void Collection_Contains_normalized_for_overload_matching_element_type()
    {
        var visitor = new QueryableMethodNormalizingExpressionVisitor(null!, isEfConstantSupported: false);
        var collection = new SpanSet<string, int>(["a", "b"]);
        var method = typeof(SpanSet<string, int>).GetMethod(nameof(SpanSet<string, int>.Contains), [typeof(string)])!;

        var expression = Expression.Call(Expression.Constant(collection), method, Expression.Constant("a"));
        var result = visitor.Normalize(expression);

        var methodCall = Assert.IsAssignableFrom<MethodCallExpression>(result);
        Assert.Equal(QueryableMethods.Contains.MakeGenericMethod(typeof(string)), methodCall.Method);
    }

    [Fact]
    public void ReadOnlyCollection_Contains_normalized_for_custom_collection_with_multiple_generic_type_arguments()
    {
        var visitor = new QueryableMethodNormalizingExpressionVisitor(null!, isEfConstantSupported: false);
        var collection = new ReadOnlyKeyedCollection<string, int>([1, 2]);
        var method = typeof(ReadOnlyKeyedCollection<string, int>).GetMethod(nameof(ReadOnlyKeyedCollection<string, int>.Contains), [typeof(int)])!;

        var expression = Expression.Call(Expression.Constant(collection), method, Expression.Constant(1));
        var result = visitor.Normalize(expression);

        var methodCall = Assert.IsAssignableFrom<MethodCallExpression>(result);
        Assert.Equal(QueryableMethods.Contains.MakeGenericMethod(typeof(int)), methodCall.Method);
    }

    [Fact]
    public void ReadOnlyCollection_Contains_not_normalized_for_overload_not_matching_element_type()
    {
        var visitor = new QueryableMethodNormalizingExpressionVisitor(null!, isEfConstantSupported: false);
        var collection = new ReadOnlySpanSet<string, int>(["a", "b"]);
        var method = typeof(ReadOnlySpanSet<string, int>).GetMethod(nameof(ReadOnlySpanSet<string, int>.Contains), [typeof(int)])!;

        var expression = Expression.Call(Expression.Constant(collection), method, Expression.Constant(1));
        var result = visitor.Normalize(expression);

        var methodCall = Assert.IsAssignableFrom<MethodCallExpression>(result);
        Assert.Equal(method, methodCall.Method);
    }
}
