// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         An expression visitor that replaces one expression with another in given expression tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     and <see href="https://aka.ms/efcore-docs-how-query-works">How EF Core queries work</see> for more information and examples.
/// </remarks>
public class ReplacingExpressionVisitor : ExpressionVisitor
{
    private readonly IReadOnlyList<Expression> _originals;
    private readonly IReadOnlyList<Expression> _replacements;

    /// <summary>
    ///     Replaces one expression with another in given expression tree.
    /// </summary>
    /// <param name="original">The expression to replace.</param>
    /// <param name="replacement">The expression to be used as replacement.</param>
    /// <param name="tree">The expression tree in which replacement is going to be performed.</param>
    /// <returns>An expression tree with replacements made.</returns>
    public static Expression Replace(Expression original, Expression replacement, Expression tree)
        => new ReplacingExpressionVisitor([original], [replacement]).Visit(tree);

    /// <summary>
    ///     Replaces one expression with another in given expression tree.
    /// </summary>
    /// <param name="originals">A list of original expressions to replace.</param>
    /// <param name="replacements">A list of expressions to be used as replacements.</param>
    /// <param name="tree">The expression tree in which replacement is going to be performed.</param>
    /// <returns>An expression tree with replacements made.</returns>
    public static Expression Replace(IReadOnlyList<Expression> originals, IReadOnlyList<Expression> replacements, Expression tree)
        => new ReplacingExpressionVisitor(originals, replacements).Visit(tree);

    /// <summary>
    ///     Creates a new instance of the <see cref="ReplacingExpressionVisitor" /> class.
    /// </summary>
    /// <param name="originals">A list of original expressions to replace.</param>
    /// <param name="replacements">A list of expressions to be used as replacements.</param>
    public ReplacingExpressionVisitor(IReadOnlyList<Expression> originals, IReadOnlyList<Expression> replacements)
    {
        _originals = originals;
        _replacements = replacements;
    }

    /// <inheritdoc />
    [return: NotNullIfNotNull(nameof(expression))]
    public override Expression? Visit(Expression? expression)
    {
        if (expression is null or ShapedQueryExpression or StructuralTypeShaperExpression or GroupByShaperExpression
            or LiftableConstantExpression)
        {
            return expression;
        }

        // We use two arrays rather than a dictionary because hash calculation here can be prohibitively expensive
        // for deep trees. Locality of reference makes arrays better for the small number of replacements anyway.
        for (var i = 0; i < _originals.Count; i++)
        {
            if (ReferenceEquals(expression, _originals[i]))
            {
                return _replacements[i];
            }
        }

        return base.Visit(expression);
    }

    /// <inheritdoc />
    protected override Expression VisitMember(MemberExpression memberExpression)
    {
        var innerExpression = Visit(memberExpression.Expression);

        if (innerExpression is GroupByShaperExpression groupByShaperExpression
            && memberExpression.Member.Name == nameof(IGrouping<,>.Key))
        {
            return groupByShaperExpression.KeySelector;
        }

        if (innerExpression is NewExpression newExpression)
        {
            var index = newExpression.Members?.IndexOf(memberExpression.Member)
                ?? GetConstructorParameterIndex(newExpression, memberExpression.Member.Name);
            if (index >= 0)
            {
                return newExpression.Arguments[index.Value];
            }
        }

        var mayBeMemberInitExpression = innerExpression.UnwrapTypeConversion(out _);
        if (mayBeMemberInitExpression is MemberInitExpression memberInitExpression
            && memberInitExpression.Bindings.SingleOrDefault(mb => mb.Member.IsSameAs(memberExpression.Member)) is MemberAssignment
                memberAssignment)
        {
            return memberAssignment.Expression;
        }

        return memberExpression.Update(innerExpression);
    }

    // The compiler only populates NewExpression.Members for anonymous types; it is always null for every other type
    // (records, record structs, and ordinary classes/structs with a primary or explicit constructor), even when a
    // constructor parameter and a read-only property share the same name (the common "positional" pattern). Without
    // some fallback, a later member access on such a projected object (e.g. a join key selector referencing a member
    // of a constructor-bound whole-object projection) can't be folded back to its underlying constructor argument,
    // causing translation of the containing query to fail entirely.
    //
    // This fallback is intentionally restricted to System.ValueTuple<...> and System.Tuple<...>: these are the only
    // non-anonymous types where the constructor-parameter-to-member correspondence is a guaranteed, closed contract
    // rather than a mere convention -- their constructors and Item1..Item7/Rest members (fields on ValueTuple,
    // properties on Tuple) are fixed, documented BCL behavior that no user code can intercept or override (unlike an
    // ordinary class/record/struct, where a same-named property can legally be redeclared with a transforming
    // initializer, e.g. "public string Name { get; } = name.Trim();", which is indistinguishable from a safe
    // passthrough via reflection alone -- see the discussion that led to narrowing this from a general
    // by-convention name match to this closed set). For every other type,
    // this returns null and the member access is left unfolded, exactly as before this fallback existed: the
    // containing query still fails to translate (loudly), rather than risking a silently-wrong fold.
    private static int? GetConstructorParameterIndex(NewExpression newExpression, string memberName)
    {
        // NewExpression.Constructor is null for a value type's parameterless "new T()" (e.g. "() => new
        // ValueTuple<int, int>()" compiles to exactly this), which -- like the anonymous-type case this fallback
        // exists for -- also has null Members. IsValueTupleOrTuple would still be true here, so this null check
        // must come first: there is no constructor to fold against, and Arguments is empty anyway.
        if (newExpression.Constructor is null || !IsValueTupleOrTuple(newExpression.Type))
        {
            return null;
        }

        // ValueTuple/Tuple constructor parameters are always named "item1".."item7"/"rest", matching the
        // corresponding "Item1".."Item7"/"Rest" member names up to case -- a fixed BCL contract, not a heuristic.
        // Names are guaranteed unique here, so unlike a general by-convention match, no ambiguity is possible.
        var parameters = newExpression.Constructor.GetParameters();
        for (var i = 0; i < parameters.Length; i++)
        {
            if (string.Equals(parameters[i].Name, memberName, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return null;
    }

    private static readonly HashSet<Type> ValueTupleAndTupleOpenGenericTypes =
    [
        typeof(ValueTuple<>), typeof(ValueTuple<,>), typeof(ValueTuple<,,>), typeof(ValueTuple<,,,>),
        typeof(ValueTuple<,,,,>), typeof(ValueTuple<,,,,,>), typeof(ValueTuple<,,,,,,>), typeof(ValueTuple<,,,,,,,>),
        typeof(Tuple<>), typeof(Tuple<,>), typeof(Tuple<,,>), typeof(Tuple<,,,>),
        typeof(Tuple<,,,,>), typeof(Tuple<,,,,,>), typeof(Tuple<,,,,,,>), typeof(Tuple<,,,,,,,>)
    ];

    private static bool IsValueTupleOrTuple(Type type)
        => type.IsGenericType && ValueTupleAndTupleOpenGenericTypes.Contains(type.GetGenericTypeDefinition());

    /// <inheritdoc />
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        if (methodCallExpression.TryGetEFPropertyArguments(out var entityExpression, out var propertyName))
        {
            var newEntityExpression = Visit(entityExpression);

            // EF.Property<T>'s instance parameter is object, so a value-type entity expression (e.g. a ValueTuple)
            // arrives here boxed via a Convert node; unwrap it once up front for both checks below, same as the
            // single unwrap VisitMember does. UnwrapTypeConversion also strips any redundant Convert/TypeAs on a
            // reference-type expression, so this can newly expose a New/MemberInit that a boxing-only unwrap would
            // have missed -- the effect isn't limited to value types.
            var unwrappedEntityExpression = newEntityExpression.UnwrapTypeConversion(out _);

            if (unwrappedEntityExpression is NewExpression newExpression)
            {
                var index = newExpression.Members?.Select(m => m.Name).IndexOf(propertyName)
                    ?? GetConstructorParameterIndex(newExpression, propertyName);
                if (index >= 0)
                {
                    // Discarding the outer Convert-to-object is safe: EF.Property<T>'s T is inferred from the call
                    // site to match the property's/argument's own type, never the boxed instance's type.
                    return newExpression.Arguments[index.Value];
                }
            }

            if (unwrappedEntityExpression is MemberInitExpression memberInitExpression
                && memberInitExpression.Bindings.SingleOrDefault(mb => mb.Member.Name == propertyName) is MemberAssignment memberAssignment)
            {
                return memberAssignment.Expression;
            }

            return methodCallExpression.Update(null, [newEntityExpression, methodCallExpression.Arguments[1]]);
        }

        return base.VisitMethodCall(methodCallExpression);
    }
}
