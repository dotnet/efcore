// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerHierarchyIdMethodTranslator : IMethodCallTranslator
{
    private static readonly Dictionary<MethodInfo, string> _methodToFunctionName = new()
    {
        // instance methods
        { typeof(HierarchyId).GetRuntimeMethod(nameof(HierarchyId.GetAncestor), [typeof(int)])!, "GetAncestor" },
        { typeof(HierarchyId).GetRuntimeMethod(nameof(HierarchyId.GetDescendant), [typeof(HierarchyId)])!, "GetDescendant" },
        {
            typeof(HierarchyId).GetRuntimeMethod(nameof(HierarchyId.GetDescendant), [typeof(HierarchyId), typeof(HierarchyId)])!,
            "GetDescendant"
        },
        { typeof(HierarchyId).GetRuntimeMethod(nameof(HierarchyId.GetLevel), Type.EmptyTypes)!, "GetLevel" },
        {
            typeof(HierarchyId).GetRuntimeMethod(
                nameof(HierarchyId.GetReparentedValue), [typeof(HierarchyId), typeof(HierarchyId)])!,
            "GetReparentedValue"
        },
        { typeof(HierarchyId).GetRuntimeMethod(nameof(HierarchyId.IsDescendantOf), [typeof(HierarchyId)])!, "IsDescendantOf" },
        { typeof(object).GetRuntimeMethod(nameof(ToString), Type.EmptyTypes)!, "ToString" },

        // static methods
        { typeof(HierarchyId).GetRuntimeMethod(nameof(HierarchyId.GetRoot), Type.EmptyTypes)!, "hierarchyid::GetRoot" },
        { typeof(HierarchyId).GetRuntimeMethod(nameof(HierarchyId.Parse), [typeof(string)])!, "hierarchyid::Parse" },
    };

    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerHierarchyIdMethodTranslator(
        IRelationalTypeMappingSource typeMappingSource,
        ISqlExpressionFactory sqlExpressionFactory)
    {
        _typeMappingSource = typeMappingSource;
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if ((instance?.Type ?? method.DeclaringType) == typeof(HierarchyId)
            && _methodToFunctionName.TryGetValue(method, out var functionName))
        {
            var candidates = arguments.Where(a => a.Type == typeof(HierarchyId));
            if (instance is not null)
            {
                candidates = candidates.Prepend(instance);
            }

            var typeMapping = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions.InferTypeMapping(candidates.ToArray())
                ?? _typeMappingSource.FindMapping(typeof(HierarchyId))!;

            var newArguments = new List<SqlExpression>();
            for (var i = 0; i < arguments.Count; i++)
            {
                var argument = arguments[i];

                if (argument.Type == typeof(HierarchyId))
                {
                    if (argument is SqlConstantExpression { Value: HierarchyId hierarchyId })
                    {
                        argument = _sqlExpressionFactory.Fragment($"'{hierarchyId}'");
                    }

                    argument = _sqlExpressionFactory.ApplyTypeMapping(argument, typeMapping);
                }

                newArguments.Add(argument);
            }

            if (functionName == "GetDescendant"
                && newArguments.Count == 1)
            {
                newArguments.Add(_sqlExpressionFactory.Constant(null, typeof(HierarchyId)));
            }

            if (instance is not null)
            {
                if (instance.Type == typeof(HierarchyId))
                {
                    instance = _sqlExpressionFactory.ApplyTypeMapping(instance, typeMapping);
                }

                return _sqlExpressionFactory.Function(
                    instance,
                    functionName,
                    newArguments,
                    nullable: true,
                    instancePropagatesNullability: true,
                    argumentsPropagateNullability: newArguments.Select(a => true),
                    method.ReturnType,
                    method.ReturnType == typeof(HierarchyId) ? typeMapping : null);
            }

            return _sqlExpressionFactory.Function(
                functionName,
                newArguments,
                nullable: true,
                argumentsPropagateNullability: newArguments.Select(a => true),
                method.ReturnType,
                method.ReturnType == typeof(HierarchyId) ? typeMapping : null);
        }

        return null;
    }
}
