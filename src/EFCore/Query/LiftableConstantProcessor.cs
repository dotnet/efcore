// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

#pragma warning disable CS1591

public class LiftableConstantProcessor : ExpressionVisitor, ILiftableConstantProcessor
{
    private bool _inline;
    private MaterializerLiftableConstantContext _materializerLiftableConstantContext;

    /// <summary>
    ///     Exposes all constants that have been lifted during the last invocation of <see cref="LiftedConstants" />.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public virtual IReadOnlyList<(ParameterExpression Parameter, Expression Expression)> LiftedConstants { get; private set; }
        = Array.Empty<(ParameterExpression Parameter, Expression Expression)>();

    private record LiftedConstant(ParameterExpression Parameter, Expression Expression, ParameterExpression? ReplacingParameter = null);

    private readonly List<LiftedConstant> _liftedConstants = new();
    private readonly LiftedExpressionProcessor _liftedExpressionProcessor = new();
    private readonly LiftedConstantOptimizer _liftedConstantOptimizer = new();

    private ParameterExpression? _contextParameter;

    public LiftableConstantProcessor(ShapedQueryCompilingExpressionVisitorDependencies dependencies)
    {
        _materializerLiftableConstantContext = new(dependencies);

        _liftedConstants.Clear();
    }

    /// <summary>
    ///     Inlines all liftable constants as simple <see cref="ConstantExpression" /> nodes in the tree, containing the result of
    ///     evaluating the liftable constants' resolvers.
    /// </summary>
    /// <param name="expression">An expression containing <see cref="LiftableConstantExpression" /> nodes.</param>
    /// <returns>
    ///     An expression tree containing <see cref="ConstantExpression" /> nodes instead of <see cref="LiftableConstantExpression" /> nodes.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         Liftable constant inlining is performed in the regular, non-precompiled query pipeline flow.
    ///     </para>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    /// </remarks>
    public virtual Expression InlineConstants(Expression expression)
    {
        _liftedConstants.Clear();
        _inline = true;

        return Visit(expression);
    }

    /// <summary>
    ///     Lifts all <see cref="LiftableConstantExpression" /> nodes, embedding <see cref="ParameterExpression" /> in their place and
    ///     exposing the parameter and resolver via <see cref="LiftedConstants" />.
    /// </summary>
    /// <param name="expression">An expression containing <see cref="LiftableConstantExpression" /> nodes.</param>
    /// <param name="contextParameter">
    ///     The <see cref="ParameterExpression" /> to be embedded in the lifted constant nodes' resolvers, instead of their lambda
    ///     parameter.
    /// </param>
    /// <param name="variableNames">
    ///     A set of variables already in use, for uniquification. Any generates variables will be added to this set.
    /// </param>
    /// <returns>
    ///     An expression tree containing <see cref="ParameterExpression" /> nodes instead of <see cref="LiftableConstantExpression" /> nodes.
    /// </returns>
    public virtual Expression LiftConstants(Expression expression, ParameterExpression contextParameter, HashSet<string> variableNames)
    {
        _liftedConstants.Clear();

        _inline = false;
        _contextParameter = contextParameter;

        var expressionAfterLifting = Visit(expression);

        // All liftable constant nodes have been lifted out.
        // We'll now optimize them, looking for greatest common denominator tree fragments, in cases where e.g. two lifted constants look up
        // the same entity type.
        _liftedConstantOptimizer.Optimize(_liftedConstants);

        // Uniquify all variable names, taking into account possible remapping done in the optimization phase above
        var replacedParameters = new Dictionary<ParameterExpression, ParameterExpression>();
        // var (originalParameters, newParameters) = (new List<Expression>(), new List<Expression>());
        for (var i = 0; i < _liftedConstants.Count; i++)
        {
            var liftedConstant = _liftedConstants[i];

            if (liftedConstant.ReplacingParameter is not null)
            {
                // This lifted constant is being removed, since it's a duplicate of another with the same expression.
                // We still need to remap the parameter in the expression, but no uniquification etc.
                replacedParameters.Add(liftedConstant.Parameter,
                    replacedParameters.TryGetValue(liftedConstant.ReplacingParameter, out var replacedReplacingParameter)
                        ? replacedReplacingParameter
                        : liftedConstant.ReplacingParameter);
                _liftedConstants.RemoveAt(i--);
                continue;
            }

            var name = liftedConstant.Parameter.Name ?? "unknown";
            var baseName = name;
            for (var j = 0; variableNames.Contains(name); j++)
            {
                name = baseName + j;
            }

            variableNames.Add(name);

            if (name != liftedConstant.Parameter.Name)
            {
                var newParameter = Expression.Parameter(liftedConstant.Parameter.Type, name);
                _liftedConstants[i] = liftedConstant with { Parameter = newParameter };
                replacedParameters.Add(liftedConstant.Parameter, newParameter);
            }
        }

        // Finally, apply all remapping (optimization, uniquification) to both the expression tree and to the lifted constant variable
        // themselves.

        // var (originalParametersArray, newParametersArray) = (originalParameters.ToArray(), newParameters.ToArray());
        // var remappedExpression = ReplacingExpressionVisitor.Replace(originalParametersArray, newParametersArray, expressionAfterLifting);
        var originalParameters = new Expression[replacedParameters.Count];
        var newParameters = new Expression[replacedParameters.Count];
        var index = 0;
        foreach (var (originalParameter, newParameter) in replacedParameters)
        {
            originalParameters[index] = originalParameter;
            newParameters[index] = newParameter;
            index++;
        }
        var remappedExpression = ReplacingExpressionVisitor.Replace(originalParameters, newParameters, expressionAfterLifting);

        for (var i = 0; i < _liftedConstants.Count; i++)
        {
            var liftedConstant = _liftedConstants[i];
            var remappedLiftedConstantExpression =
                ReplacingExpressionVisitor.Replace(originalParameters, newParameters, liftedConstant.Expression);

            if (remappedLiftedConstantExpression != liftedConstant.Expression)
            {
                _liftedConstants[i] = liftedConstant with { Expression = remappedLiftedConstantExpression };
            }
        }

        LiftedConstants = _liftedConstants.Select(c => (c.Parameter, c.Expression)).ToArray();
        return remappedExpression;
    }

    protected override Expression VisitExtension(Expression node)
    {
        if (node is LiftableConstantExpression liftedConstant)
        {
            return _inline
                ? InlineConstant(liftedConstant)
                : LiftConstant(liftedConstant);
        }

        return base.VisitExtension(node);
    }

    protected virtual ConstantExpression InlineConstant(LiftableConstantExpression liftableConstant)
    {
        if (liftableConstant.ResolverExpression is Expression<Func<MaterializerLiftableConstantContext, object>>
            resolverExpression)
        {
            var resolver = resolverExpression.Compile(preferInterpretation: true);
            var value = resolver(_materializerLiftableConstantContext);
            return Expression.Constant(value, liftableConstant.Type);
        }

        throw new InvalidOperationException(
            $"Unknown resolved expression of type {liftableConstant.ResolverExpression.GetType().Name} found on liftable constant expression");
    }

    protected virtual ParameterExpression LiftConstant(LiftableConstantExpression liftableConstant)
    {
        var resolverLambda = liftableConstant.ResolverExpression;
        var parameter = resolverLambda.Parameters[0];

        // Extract the lambda body, replacing the lambda parameter with our lifted constant context parameter, and also inline any captured
        // literals
        var body = _liftedExpressionProcessor.Process(resolverLambda.Body, parameter, _contextParameter!);

        // If the lambda returns a value type, a Convert to object node gets needed that we need to unwrap
        if (body is UnaryExpression { NodeType: ExpressionType.Convert } convertNode
            && convertNode.Type == typeof(object))
        {
            body = convertNode.Operand;
        }

        // Register the lifted constant; note that the name will be uniquified later
        var variableParameter = Expression.Parameter(liftableConstant.Type, liftableConstant.VariableName);
        _liftedConstants.Add(new(variableParameter, body));

        return variableParameter;
    }

    private class LiftedConstantOptimizer : ExpressionVisitor
    {
        private List<LiftedConstant> _liftedConstants = null!;

        private record ExpressionInfo(ExpressionStatus Status, ParameterExpression? Parameter = null, string? PreferredName = null);
        private readonly Dictionary<Expression, ExpressionInfo> _indexedExpressions = new(ExpressionEqualityComparer.Instance);
        private LiftedConstant _currentLiftedConstant = null!;
        private bool _firstPass;
        private int _index;

        public void Optimize(List<LiftedConstant> liftedConstants)
        {
            _liftedConstants = liftedConstants;
            _indexedExpressions.Clear();

            _firstPass = true;

            // Phase 1: recursively seek out tree fragments which appear more than once across the lifted constants. These will be extracted
            // out to separate variables.
            foreach (var liftedConstant in liftedConstants)
            {
                _currentLiftedConstant = liftedConstant;
                Visit(liftedConstant.Expression);
            }

            // Filter out fragments which don't appear at least once
            foreach (var (expression, expressionInfo) in _indexedExpressions)
            {
                if (expressionInfo.Status == ExpressionStatus.SeenOnce)
                {
                    _indexedExpressions.Remove(expression);
                    continue;
                }

                Check.DebugAssert(expressionInfo.Status == ExpressionStatus.SeenMultipleTimes,
                    "expressionInfo.Status == ExpressionStatus.SeenMultipleTimes");
            }

            // Second pass: extract common denominator tree fragments to separate variables
            _firstPass = false;
            for (_index = 0; _index < liftedConstants.Count; _index++)
            {
                _currentLiftedConstant = _liftedConstants[_index];
                if (_indexedExpressions.TryGetValue(_currentLiftedConstant.Expression, out var expressionInfo)
                    && expressionInfo.Status == ExpressionStatus.Extracted)
                {
                    // This entire lifted constant has already been extracted before, so we no longer need it as a separate variable.
                    _liftedConstants[_index] = _currentLiftedConstant with { ReplacingParameter = expressionInfo.Parameter };

                    continue;
                }

                var optimizedExpression = Visit(_currentLiftedConstant.Expression);
                if (optimizedExpression != _currentLiftedConstant.Expression)
                {
                    _liftedConstants[_index] = _currentLiftedConstant with { Expression = optimizedExpression };
                }
            }
        }

        [return: NotNullIfNotNull(nameof(node))]
        public override Expression? Visit(Expression? node)
        {
            if (node is null)
            {
                return null;
            }

            if (node is ParameterExpression or ConstantExpression || node.Type.IsAssignableTo(typeof(LambdaExpression)))
            {
                return node;
            }

            if (_firstPass)
            {
                var preferredName = ReferenceEquals(node, _currentLiftedConstant.Expression)
                    ? _currentLiftedConstant.Parameter.Name
                    : null;

                if (!_indexedExpressions.TryGetValue(node, out var expressionInfo))
                {
                    // Unseen expression, add it to the dictionary with a null value, to indicate it's only a candidate at this point.
                    _indexedExpressions[node] = new(ExpressionStatus.SeenOnce, PreferredName: preferredName);
                    return base.Visit(node);
                }

                // We've already seen this expression.
                if (expressionInfo.Status == ExpressionStatus.SeenOnce
                    || expressionInfo.PreferredName is null && preferredName is not null)
                {
                    // This is the 2nd time we're seeing the expression - mark it as a common denominator
                    _indexedExpressions[node] = _indexedExpressions[node] with
                    {
                        Status = ExpressionStatus.SeenMultipleTimes,
                        PreferredName = preferredName
                    };
                }

                // We've already seen and indexed this expression, no need to do it again
                return node;
            }
            else
            {
                // 2nd pass
                if (_indexedExpressions.TryGetValue(node, out var expressionInfo) && expressionInfo.Status != ExpressionStatus.SeenOnce)
                {
                    // This fragment is common across multiple lifted constants.
                    if (expressionInfo.Status == ExpressionStatus.SeenMultipleTimes)
                    {
                        // This fragment hasn't yet been extracted out to its own variable in the 2nd pass.

                        // If this happens to be a top-level node in the lifted constant, no need to extract an additional variable - just
                        // use that as the "extracted" parameter further down.
                        if (ReferenceEquals(node, _currentLiftedConstant.Expression))
                        {
                            _indexedExpressions[node] = new(ExpressionStatus.Extracted, _currentLiftedConstant.Parameter);
                            return base.Visit(node);
                        }

                        // Otherwise, we need to extract a new variable, integrating it just before this one.
                        var parameter = Expression.Parameter(node.Type, node switch
                        {
                            _ when expressionInfo.PreferredName is not null => expressionInfo.PreferredName,
                            MemberExpression me => char.ToLowerInvariant(me.Member.Name[0]) + me.Member.Name[1..],
                            MethodCallExpression mce => char.ToLowerInvariant(mce.Method.Name[0]) + mce.Method.Name[1..],
                            _ => "unknown"
                        });

                        var visitedNode = base.Visit(node);
                        _liftedConstants.Insert(_index++, new(parameter, visitedNode));

                        // Mark this node as having been extracted, to prevent it from getting extracted again
                        expressionInfo = _indexedExpressions[node] = new(ExpressionStatus.Extracted, parameter);
                    }

                    Check.DebugAssert(expressionInfo.Parameter is not null, "expressionInfo.Parameter is not null");

                    return expressionInfo.Parameter;
                }

                // This specific fragment only appears once across the lifted constants; keep going down.
                return base.Visit(node);
            }
        }

        private enum ExpressionStatus
        {
            SeenOnce,
            SeenMultipleTimes,
            Extracted
        }
    }

    private class LiftedExpressionProcessor : ExpressionVisitor
    {
        private ParameterExpression _originalParameter = null!;
        private ParameterExpression _replacingParameter = null!;

        public Expression Process(Expression expression, ParameterExpression originalParameter, ParameterExpression replacingParameter)
        {
            _originalParameter = originalParameter;
            _replacingParameter = replacingParameter;

            return Visit(expression);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            // The expression to be lifted may contain a captured variable; for limited literal scenarios, inline that variable into the
            // expression so we can render it out to C#.

            // TODO: For the general case, this needs to be a full blown "evaluatable" identifier (like ParameterExtractingEV), which can
            // identify any fragments of the tree which don't depend on the lambda parameter, and evaluate them.
            // But for now we're doing a reduced version.

            var visited = base.VisitMember(node);

            if (visited is MemberExpression
                {
                    Expression: ConstantExpression { Value: { } constant },
                    Member: var member
                })
            {
                return member switch
                {
                    FieldInfo fi => Expression.Constant(fi.GetValue(constant), node.Type),
                    PropertyInfo pi => Expression.Constant(pi.GetValue(constant), node.Type),
                    _ => visited
                };
            }

            return visited;
        }

        protected override Expression VisitParameter(ParameterExpression node)
            => ReferenceEquals(node, _originalParameter)
                ? _replacingParameter
                : base.VisitParameter(node);
    }

#if DEBUG
    protected override Expression VisitConstant(ConstantExpression node)
    {
        return IsLiteral(node.Value)
            ? node
            : throw new InvalidOperationException(
                $"Materializer expression contains a non-literal constant of type '{node.Value!.GetType().Name}'. " +
                $"Use a {nameof(LiftableConstantExpression)} to reference any non-literal constants.");

        static bool IsLiteral(object? value)
        {
            return value switch
            {
                int or long or uint or ulong or short or sbyte or ushort or byte or double or float or decimal
                    => true,

                string or bool or Type or Enum or null => true,

                ITuple tuple
                    when tuple.GetType() is { IsGenericType: true } tupleType
                         && tupleType.Name.StartsWith("ValueTuple`", StringComparison.Ordinal)
                         && tupleType.Namespace == "System"
                    => IsTupleLiteral(tuple),

                _ => false
            };

            bool IsTupleLiteral(ITuple tuple)
            {
                for (var i = 0; i < tuple.Length; i++)
                {
                    if (!IsLiteral(tuple[i]))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
#endif
}
