// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     This is an experimental API used by the Entity Framework Core feature and it is not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
[Experimental(EFDiagnostics.PrecompiledQueryExperimental)]
public class RuntimeConstantProcessor : ExpressionVisitor
{
    private IEnumerable<RuntimeConstantExpression> _preexistingRuntimeConstants = null!;
    private readonly List<RuntimeConstantExpression> _foundRuntimeConstants = [];
    private readonly Dictionary<Expression, RuntimeConstantExpression> _runtimeConstantsByExpression =
        new(ExpressionEqualityComparer.Instance);
    private readonly Dictionary<object, RuntimeConstantExpression> _runtimeConstantsByValue = [];
    private bool _runtimeConstantLookupsInitialized;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression Process(Expression expression, IEnumerable<RuntimeConstantExpression>? preexistingRuntimeConstants)
    {
        _preexistingRuntimeConstants = preexistingRuntimeConstants ?? [];
        _foundRuntimeConstants.Clear();
        _runtimeConstantsByExpression.Clear();
        _runtimeConstantsByValue.Clear();
        _runtimeConstantLookupsInitialized = false;

        var result = Visit(expression);

        return result;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<RuntimeConstantExpression> RuntimeConstants => _foundRuntimeConstants;

    /// <inheritdoc/>
    protected override Expression VisitExtension(Expression node)
    {
        if (node is RuntimeConstantExpression runtimeConstant)
        {
            if (runtimeConstant.Value is null)
            {
                return Expression.Constant(null, runtimeConstant.Type);
            }

            if (!_runtimeConstantLookupsInitialized)
            {
                foreach (var preexistingRuntimeConstant in _preexistingRuntimeConstants)
                {
                    AddRuntimeConstantLookup(preexistingRuntimeConstant);
                }

                _runtimeConstantLookupsInitialized = true;
            }

            if (_runtimeConstantsByValue.TryGetValue(runtimeConstant.Value, out var existing)
             || _runtimeConstantsByExpression.TryGetValue(runtimeConstant.InitializeExpression, out existing))
            {
                runtimeConstant = existing;
            }
            else
            {
                AddRuntimeConstantLookup(runtimeConstant);
                _foundRuntimeConstants.Add(runtimeConstant);
            }

            return Expression.Constant(runtimeConstant.Value, runtimeConstant.Type);
        }

        return base.VisitExtension(node);
    }

    private void AddRuntimeConstantLookup(RuntimeConstantExpression runtimeConstant)
    {
        _runtimeConstantsByExpression.TryAdd(runtimeConstant.InitializeExpression, runtimeConstant);

        if (runtimeConstant.Value is not null)
        {
            _runtimeConstantsByValue.TryAdd(runtimeConstant.Value, runtimeConstant);
        }
    }
}
