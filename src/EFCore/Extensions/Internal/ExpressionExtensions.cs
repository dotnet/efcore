// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class ExpressionExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static Expression MakeHasSentinel(
        this Expression currentValueExpression,
        IReadOnlyPropertyBase? propertyBase)
    {
        var sentinel = propertyBase?.Sentinel;

        var isReferenceType = !currentValueExpression.Type.IsValueType;
        var isNullableValueType = currentValueExpression.Type.IsGenericType
            && currentValueExpression.Type.GetGenericTypeDefinition() == typeof(Nullable<>);

        if (sentinel == null)
        {
            return isReferenceType
                ? Expression.ReferenceEqual(
                    currentValueExpression,
                    Expression.Constant(null, currentValueExpression.Type))
                : isNullableValueType
                    ? Expression.Not(
                        Expression.MakeMemberAccess(
                            currentValueExpression,
                            currentValueExpression.Type.GetProperty("HasValue")!))
                    : Expression.Constant(false);
        }

        var comparer = (propertyBase as IProperty)?.GetValueComparer()
            ?? ValueComparer.CreateDefault(
                propertyBase?.ClrType ?? currentValueExpression.Type, favorStructuralComparisons: false);

        var equalsExpression = comparer.ExtractEqualsBody(
            comparer.Type != currentValueExpression.Type
                ? Expression.Convert(currentValueExpression, comparer.Type)
                : currentValueExpression,
            Expression.Constant(sentinel, comparer.Type));

        if (isReferenceType || isNullableValueType)
        {
            return Expression.AndAlso(
                isReferenceType
                    ? Expression.Not(
                        Expression.ReferenceEqual(
                            currentValueExpression,
                            Expression.Constant(null, currentValueExpression.Type)))
                    : Expression.MakeMemberAccess(
                        currentValueExpression,
                        currentValueExpression.Type.GetProperty("HasValue")!),
                equalsExpression);
        }

        return equalsExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IReadOnlyList<IReadOnlyList<MemberInfo>>? MatchMemberAccessChainList(
        this LambdaExpression lambdaExpression)
    {
        Check.DebugAssert(lambdaExpression.Body != null, "lambdaExpression.Body is null");
        Check.DebugAssert(
            lambdaExpression.Parameters.Count == 1,
            "lambdaExpression.Parameters.Count is " + lambdaExpression.Parameters.Count + ". Should be 1.");

        var parameter = lambdaExpression.Parameters[0];
        var body = RemoveConvert(lambdaExpression.Body);
        var paths = body is NewExpression newExpression
            ? newExpression.Arguments
            : (IReadOnlyList<Expression>)[lambdaExpression.Body];

        var chains = new List<IReadOnlyList<MemberInfo>>(paths.Count);
        foreach (var path in paths)
        {
            var parsed = MatchComplexMemberAccess(path, parameter);
            if (parsed is null)
            {
                return null;
            }

            chains.Add(parsed.Value.Members);
        }

        return chains;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IReadOnlyList<IReadOnlyList<MemberInfo>> GetMemberAccessChainList(
        this LambdaExpression expression)
        => expression.MatchMemberAccessChainList()
            ?? throw new ArgumentException(
                CoreStrings.InvalidMembersExpression(expression));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IReadOnlyList<TMemberInfo>? MatchMemberAccessList<TMemberInfo>(
        this LambdaExpression lambdaExpression,
        Func<Expression, Expression, TMemberInfo?> memberMatcher)
        where TMemberInfo : MemberInfo
    {
        Check.DebugAssert(lambdaExpression.Body != null, "lambdaExpression.Body is null");
        Check.DebugAssert(
            lambdaExpression.Parameters.Count == 1,
            "lambdaExpression.Parameters.Count is " + lambdaExpression.Parameters.Count + ". Should be 1.");

        var parameterExpression = lambdaExpression.Parameters[0];

        if (RemoveConvert(lambdaExpression.Body) is NewExpression newExpression)
        {
            var memberInfos
                = (List<TMemberInfo>)newExpression
                    .Arguments
                    .Select(a => memberMatcher(a, parameterExpression))
                    .Where(p => p != null)
                    .ToList()!;

            return memberInfos.Count != newExpression.Arguments.Count ? null : memberInfos;
        }

        var memberPath = memberMatcher(lambdaExpression.Body, parameterExpression);

        return memberPath != null ? new[] { memberPath } : null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static TMemberInfo? MatchSimpleMemberAccess<TMemberInfo>(
        this Expression parameterExpression,
        Expression memberAccessExpression)
        where TMemberInfo : MemberInfo
    {
        var memberInfos = MatchMemberAccess<TMemberInfo>(parameterExpression, memberAccessExpression);

        return memberInfos?.Count == 1 ? memberInfos[0] : null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IReadOnlyList<MemberInfo>? MatchMemberAccessChain(
        this LambdaExpression lambdaExpression)
    {
        Check.DebugAssert(
            lambdaExpression.Parameters.Count == 1,
            $"Parameters.Count is {lambdaExpression.Parameters.Count}");

        var parsed = MatchComplexMemberAccess(lambdaExpression.Body, lambdaExpression.Parameters[0]);
        return parsed?.Members;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IReadOnlyList<MemberInfo> GetMemberAccessChain(
        this LambdaExpression expression,
        string parameterName)
            => expression.MatchMemberAccessChain()
                ?? throw new ArgumentException(
                    CoreStrings.InvalidMemberAccessChainExpression(expression),
                    parameterName);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Parses a lambda whose body may traverse complex collections via
    ///         <see cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})" />
    ///         or constant indexers.
    ///     </para>
    ///     <para>
    ///         Returns one entry per indexed member (an anonymous-type <see cref="NewExpression" /> body produces
    ///         multiple entries; any other body produces one entry). <c>Members</c> contains the resolved member
    ///         chain (skipping over <c>Select</c> / indexer boundaries). <c>IsCollection</c> runs parallel to
    ///         <c>Members</c> (length equal to <c>Members.Count</c>): <see langword="true" /> at a given position
    ///         means the corresponding member was reached as a complex-collection traversal (a <c>Select</c>
    ///         projection or a constant indexer). <c>CollectionIndices</c> has one entry per traversed
    ///         complex-collection segment — ordered to match the <see langword="true" /> entries in
    ///         <c>IsCollection</c>; <see langword="null" /> means "all elements" (a <c>Select</c> projection)
    ///         and a non-<see langword="null" /> <see cref="int" /> means the fixed element index.
    ///     </para>
    ///     <para>
    ///         The top-level <c>IsCollection</c> is <see langword="null" /> only when every parsed chain is
    ///         a single non-collection member; the per-chain inner <c>CollectionIndices</c> is
    ///         <see langword="null" /> when that chain traverses no complex collection, and the top-level
    ///         <c>CollectionIndices</c> is <see langword="null" /> when no chain traverses any complex collection.
    ///     </para>
    ///     <para>
    ///         Throws <see cref="ArgumentException" /> if any leaf cannot be parsed as a recognised member-access chain.
    ///     </para>
    /// </remarks>
    public static (IReadOnlyList<IReadOnlyList<MemberInfo>> Members,
        IReadOnlyList<IReadOnlyList<bool>>? IsCollection,
        IReadOnlyList<IReadOnlyList<int?>?>? CollectionIndices)
        MatchComplexMemberAccessList(this LambdaExpression lambdaExpression, string parameterName)
    {
        Check.DebugAssert(lambdaExpression.Body != null, "lambdaExpression.Body is null");
        Check.DebugAssert(
            lambdaExpression.Parameters.Count == 1,
            "lambdaExpression.Parameters.Count is " + lambdaExpression.Parameters.Count + ". Should be 1.");

        var parameter = lambdaExpression.Parameters[0];
        var body = RemoveConvert(lambdaExpression.Body);

        var paths = body is NewExpression newExpression ? newExpression.Arguments : (IReadOnlyList<Expression>)[lambdaExpression.Body];
        var members = new List<IReadOnlyList<MemberInfo>>(paths.Count);
        var isCollection = new List<IReadOnlyList<bool>>(paths.Count);
        var indices = new List<IReadOnlyList<int?>?>(paths.Count);
        var anyIndices = false;
        var anyComplexChain = false;

        foreach (var path in paths)
        {
            var parsed = MatchComplexMemberAccess(path, parameter) ?? throw new ArgumentException(
                    CoreStrings.InvalidMemberAccessChainExpression(lambdaExpression), parameterName);

            members.Add(parsed.Members);
            isCollection.Add(parsed.IsCollection);
            indices.Add(parsed.CollectionIndices);
            if (InternalTypeBaseBuilder.ContainsMultipleOrTrue(parsed.IsCollection))
            {
                anyComplexChain = true;
            }

            if (parsed.CollectionIndices is not null)
            {
                anyIndices = true;
            }
        }

        return (members, anyComplexChain ? isCollection : null, anyIndices ? indices : null);
    }

    private static (IReadOnlyList<MemberInfo> Members, IReadOnlyList<bool> IsCollection, IReadOnlyList<int?>? CollectionIndices)?
        MatchComplexMemberAccess(
            Expression expression,
            ParameterExpression parameter)
    {
        var members = new List<MemberInfo>();
        var indices = new List<int?>();
        var collectionPositions = new HashSet<int>();
        if (!VisitMemberAccess(expression, parameter, members, indices, collectionPositions))
        {
            return null;
        }

        // Build a per-member is-collection list (length = members.Count). A position is marked true when
        // the corresponding member was reached through a complex-collection traversal (Select or indexer).
        bool[] isCollection;
        if (members.Count == 0)
        {
            isCollection = [];
        }
        else
        {
            isCollection = new bool[members.Count];
            foreach (var pos in collectionPositions)
            {
                isCollection[pos] = true;
            }
        }

        return (members, isCollection, indices.Count == 0 ? null : indices);
    }

    private static bool VisitMemberAccess(
        Expression expression,
        ParameterExpression parameter,
        List<MemberInfo> members,
        List<int?> indices,
        HashSet<int> collectionPositions)
    {
        // Members and indices are populated in order from the outermost of the chain (closest to the parameter)
        // to the innermost (the leaf). This method appends to them; recursive calls process the part of the
        // chain that is closer to the parameter and then we add the post-boundary members/index on top.
        var current = RemoveTypeAs(RemoveConvert(expression));

        // Collect a tail run of MemberExpressions (post-boundary).
        var tailMembers = new List<MemberInfo>();
        while (current is MemberExpression me)
        {
            tailMembers.Add(me.Member);
            current = RemoveTypeAs(RemoveConvert(me.Expression));
        }

        tailMembers.Reverse();

        // Reached the parameter directly: no boundary, just a member chain.
        if (current == parameter)
        {
            members.AddRange(tailMembers);
            return true;
        }

        // Enumerable.Select(source, lambda) — the inner lambda's body becomes the tail.
        if (current is MethodCallExpression call)
        {
            if (call.Method.IsStatic
                && (call.Method.DeclaringType == typeof(Enumerable) || call.Method.DeclaringType == typeof(Queryable))
                && call.Method.Name == nameof(Enumerable.Select)
                && call.Arguments.Count == 2
                && tailMembers.Count == 0)
            {
                var selectorOperand = call.Arguments[1];
                if (selectorOperand is UnaryExpression { NodeType: ExpressionType.Quote } quoted)
                {
                    selectorOperand = quoted.Operand;
                }

                if (selectorOperand is LambdaExpression innerLambda
                    && innerLambda.Parameters.Count == 1
                    && VisitMemberAccess(call.Arguments[0], parameter, members, indices, collectionPositions))
                {
                    indices.Add(null);
                    collectionPositions.Add(members.Count - 1);
                    return VisitMemberAccess(innerLambda.Body, innerLambda.Parameters[0], members, indices, collectionPositions);
                }

                return false;
            }

            // List<T>.get_Item / IList<T> indexer with constant int.
            if (!call.Method.IsStatic
                && call.Method.Name == "get_Item"
                && call.Arguments.Count == 1
                && call.Object is not null
                && TryGetConstantIntIndex(call.Arguments[0], out var indexerValue)
                && VisitMemberAccess(call.Object, parameter, members, indices, collectionPositions))
            {
                indices.Add(indexerValue);
                collectionPositions.Add(members.Count - 1);
                members.AddRange(tailMembers);
                return true;
            }

            return false;
        }

        // T[] indexer.
        if (current is BinaryExpression { NodeType: ExpressionType.ArrayIndex } arrayIndex
            && TryGetConstantIntIndex(arrayIndex.Right, out var arrayIndexValue)
            && VisitMemberAccess(arrayIndex.Left, parameter, members, indices, collectionPositions))
        {
            indices.Add(arrayIndexValue);
            collectionPositions.Add(members.Count - 1);
            members.AddRange(tailMembers);
            return true;
        }

        return false;
    }

    private static bool TryGetConstantIntIndex(Expression expression, out int value)
    {
        if (RemoveConvert(expression) is ConstantExpression { Value: int i } && i >= 0)
        {
            value = i;
            return true;
        }

        value = 0;
        return false;
    }

    private static List<TMemberInfo>? MatchMemberAccess<TMemberInfo>(
        this Expression parameterExpression,
        Expression memberAccessExpression)
        where TMemberInfo : MemberInfo
    {
        var memberInfos = new List<TMemberInfo>();

        var unwrappedExpression = RemoveTypeAs(RemoveConvert(memberAccessExpression));
        do
        {
            var memberExpression = unwrappedExpression as MemberExpression;
            if (memberExpression?.Member is not TMemberInfo memberInfo)
            {
                return null;
            }

            memberInfos.Insert(0, memberInfo);

            unwrappedExpression = RemoveTypeAs(RemoveConvert(memberExpression.Expression));
        }
        while (unwrappedExpression != parameterExpression);

        return memberInfos;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static Expression? RemoveTypeAs(this Expression? expression)
    {
        while (expression?.NodeType == ExpressionType.TypeAs)
        {
            expression = ((UnaryExpression)RemoveConvert(expression)).Operand;
        }

        return expression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsLogicalOperation(this Expression expression)
        => expression.NodeType is ExpressionType.AndAlso or ExpressionType.OrElse;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static LambdaExpression? GetLambdaOrNull(this Expression expression)
        => expression is LambdaExpression lambda
            ? lambda
            : expression is UnaryExpression unary && expression.NodeType == ExpressionType.Quote
                ? (LambdaExpression)unary.Operand
                : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsLogicalNot(this UnaryExpression sqlUnaryExpression)
        => sqlUnaryExpression.NodeType == ExpressionType.Not
            && (sqlUnaryExpression.Type == typeof(bool)
                || sqlUnaryExpression.Type == typeof(bool?));

    [return: NotNullIfNotNull(nameof(expression))]
    private static Expression? RemoveConvert(Expression? expression)
        => expression is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unaryExpression
            ? RemoveConvert(unaryExpression.Operand)
            : expression;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static Expression BuildPredicate(
        IReadOnlyList<IReadOnlyProperty> keyProperties,
        ValueBuffer keyValues,
        ParameterExpression entityParameter)
    {
        var keyValuesConstant = Expression.Constant(keyValues);

        var predicate = GenerateEqualExpression(entityParameter, keyValuesConstant, keyProperties[0], 0);

        for (var i = 1; i < keyProperties.Count; i++)
        {
            predicate = Expression.AndAlso(predicate, GenerateEqualExpression(entityParameter, keyValuesConstant, keyProperties[i], i));
        }

        return predicate;

        static Expression GenerateEqualExpression(
            Expression entityParameterExpression,
            Expression keyValuesConstantExpression,
            IReadOnlyProperty property,
            int i)
            => property.ClrType.IsValueType
                && property.ClrType.UnwrapNullableType() is var nonNullableType
                && !(nonNullableType == typeof(bool) || nonNullableType.IsNumeric() || nonNullableType.IsEnum)
                    ? Infrastructure.ExpressionExtensions.CreateEqualsExpression(
                        Expression.Call(
                            EF.MakePropertyMethod(typeof(object)),
                            entityParameterExpression,
                            Expression.Constant(property.Name, typeof(string))),
                        Expression.MakeIndex(
                            keyValuesConstantExpression,
                            ValueBuffer.Indexer,
                            [Expression.Constant(i)]))
                    : Expression.Equal(
                        Expression.Call(
                            EF.MakePropertyMethod(property.ClrType),
                            entityParameterExpression,
                            Expression.Constant(property.Name, typeof(string))),
                        Expression.Convert(
                            Expression.MakeIndex(
                                keyValuesConstantExpression,
                                ValueBuffer.Indexer,
                                [Expression.Constant(i)]),
                            property.ClrType));
    }
}
