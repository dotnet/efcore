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
public class RuntimeConstantProcessor(Dictionary<object, string> constantReplacements) : ExpressionVisitor
{
    private readonly List<(string Name, RuntimeConstantExpression Expression)> _runtimeConstants = [];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression Process(Expression expression) => Visit(expression);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IReadOnlyList<(string Name, RuntimeConstantExpression Expression)> RuntimeConstants => _runtimeConstants;

    /// <inheritdoc/>
    protected override Expression VisitExtension(Expression node)
    {
        if (node is RuntimeConstantExpression runtimeConstant)
        {
            var existing = _runtimeConstants
                .Select(x => x.Expression)
                .FirstOrDefault(x => ExpressionEqualityComparer.Instance.Equals(
                    x.InitializeExpression,
                    runtimeConstant.InitializeExpression));

            if (existing is null)
            {
                var fieldName = UniquifyName(runtimeConstant.Name);

                _runtimeConstants.Add((fieldName, runtimeConstant));
                constantReplacements.Add(runtimeConstant.Value, fieldName);
            }
            else
            {
                runtimeConstant = existing;
            }

            return Expression.Constant(runtimeConstant.Value, runtimeConstant.Type);
        }

        return base.VisitExtension(node);
    }

    private string UniquifyName(string baseName)
    {
        var name = baseName;
        var i = 0;
        while (_runtimeConstants.Any(c => c.Name == name))
        {
            name = baseName + i++;
        }
        return name;
    }
}
