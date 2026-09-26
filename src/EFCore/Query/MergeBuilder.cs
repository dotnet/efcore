// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         Supports configuring an ExecuteMerge operation: the columns to match on and the actions to take when a source row matches
///         (<c>WhenMatched</c>) or does not match (<c>WhenNotMatched</c>) an existing target row.
///     </para>
///     <para>
///         This type does not have any behavior beyond expression-tree capture; it is used inside the LINQ query solely for the purpose
///         of building the expression tree that the query pipeline translates.
///     </para>
/// </summary>
/// <typeparam name="TTarget">The type of the target entity.</typeparam>
/// <typeparam name="TSource">The type of the source rows.</typeparam>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     and <see href="https://aka.ms/efcore-docs-how-query-works">How EF Core queries work</see> for more information and examples.
/// </remarks>
public class MergeBuilder<TTarget, TSource>
{
    private readonly List<NewExpression> _matchSelectors = [];
    private MergeUpdateSettersBuilder<TTarget, TSource>? _whenMatched;
    private MergeInsertSettersBuilder<TTarget, TSource>? _whenNotMatched;

    private static ConstructorInfo? _matchTupleConstructor;

    private static ConstructorInfo MatchTupleConstructor
        => _matchTupleConstructor ??= typeof(Tuple<Delegate, Delegate>).GetConstructor([typeof(Delegate), typeof(Delegate)])!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public MergeBuilder()
    {
    }

    /// <summary>
    ///     Specifies the columns used to match source rows against existing target rows. The selectors typically project an anonymous
    ///     type to match on multiple columns (e.g. <c>t => new { t.A, t.B }</c>). When no <see cref="Match{TKey}" /> call is made, the
    ///     target's primary key is used.
    /// </summary>
    /// <typeparam name="TKey">The type of the match key.</typeparam>
    /// <param name="targetKey">A selector for the match columns on the target row.</param>
    /// <param name="sourceKey">A selector for the corresponding match columns on the source row.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public virtual MergeBuilder<TTarget, TSource> Match<TKey>(
        Expression<Func<TTarget, TKey>> targetKey,
        Expression<Func<TSource, TKey>> sourceKey)
    {
        _matchSelectors.Add(Expression.New(MatchTupleConstructor, targetKey, sourceKey));
        return this;
    }

    /// <summary>
    ///     Specifies the properties and values to update on the target row when a source row matches an existing target row. Omitting
    ///     this call leaves matched rows unchanged.
    /// </summary>
    /// <param name="setPropertyCalls">A collection of <c>SetProperty</c> statements specifying properties to update.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public virtual MergeBuilder<TTarget, TSource> WhenMatched(
        Action<MergeUpdateSettersBuilder<TTarget, TSource>> setPropertyCalls)
    {
        _whenMatched = new MergeUpdateSettersBuilder<TTarget, TSource>();
        setPropertyCalls(_whenMatched);
        return this;
    }

    /// <summary>
    ///     Specifies the properties and values to insert when a source row does not match any existing target row. Omitting this call
    ///     inserts all mapped columns using the same-named source members.
    /// </summary>
    /// <param name="setPropertyCalls">A collection of <c>SetProperty</c> statements specifying properties to insert.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public virtual MergeBuilder<TTarget, TSource> WhenNotMatched(
        Action<MergeInsertSettersBuilder<TTarget, TSource>> setPropertyCalls)
    {
        _whenNotMatched = new MergeInsertSettersBuilder<TTarget, TSource>();
        setPropertyCalls(_whenNotMatched);
        return this;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual NewArrayExpression BuildMatchExpression()
        => Expression.NewArrayInit(typeof(ITuple), _matchSelectors);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual Expression BuildWhenMatchedExpression()
        => _whenMatched is null
            ? Expression.Constant(null, typeof(IReadOnlyList<ITuple>))
            : _whenMatched.BuildSettersExpression();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual Expression BuildWhenNotMatchedExpression()
        => _whenNotMatched is null
            ? Expression.Constant(null, typeof(IReadOnlyList<ITuple>))
            : _whenNotMatched.BuildSettersExpression();
}
