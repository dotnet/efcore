// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerHierarchyIdMethodTranslator : IMethodCallTranslator
{
    private static readonly IDictionary<MethodInfo, string> _methodToFunctionName = new Dictionary<MethodInfo, string>
    {
        // instance methods
        { typeof(HierarchyId).GetRuntimeMethod(nameof(HierarchyId.GetAncestor), new[] { typeof(int) })!, "GetAncestor" },
        {
            typeof(HierarchyId).GetRuntimeMethod(nameof(HierarchyId.GetDescendant), new[] { typeof(HierarchyId), typeof(HierarchyId) })!,
            "GetDescendant"
        },
        { typeof(HierarchyId).GetRuntimeMethod(nameof(HierarchyId.GetLevel), Type.EmptyTypes)!, "GetLevel" },
        {
            typeof(HierarchyId).GetRuntimeMethod(
                nameof(HierarchyId.GetReparentedValue), new[] { typeof(HierarchyId), typeof(HierarchyId) })!,
            "GetReparentedValue"
        },
        { typeof(HierarchyId).GetRuntimeMethod(nameof(HierarchyId.IsDescendantOf), new[] { typeof(HierarchyId) })!, "IsDescendantOf" },
        { typeof(object).GetRuntimeMethod(nameof(HierarchyId.ToString), Type.EmptyTypes)!, "ToString" },

        // static methods
        { typeof(HierarchyId).GetRuntimeMethod(nameof(HierarchyId.GetRoot), Type.EmptyTypes)!, "hierarchyid::GetRoot" },
        { typeof(HierarchyId).GetRuntimeMethod(nameof(HierarchyId.Parse), new[] { typeof(string) })!, "hierarchyid::Parse" },
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
    public SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        // instance is null for static methods like Parse
        const string storeType = SqlServerHierarchyIdTypeMappingSourcePlugin.SqlServerTypeName;
        var callingType = instance?.Type ?? method.DeclaringType;
        if (typeof(HierarchyId).IsAssignableFrom(callingType)
            && _methodToFunctionName.TryGetValue(method, out var functionName))
        {
            var typeMappedArguments = new List<SqlExpression>();
            foreach (var argument in arguments)
            {
                var argumentTypeMapping = typeof(HierarchyId).IsAssignableFrom(argument.Type)
                    ? _typeMappingSource.FindMapping(argument.Type, storeType)
                    : _typeMappingSource.FindMapping(argument.Type);
                var mappedArgument = _sqlExpressionFactory.ApplyTypeMapping(argument, argumentTypeMapping);
                typeMappedArguments.Add(mappedArgument);
            }

            var resultTypeMapping = typeof(HierarchyId).IsAssignableFrom(method.ReturnType)
                ? _typeMappingSource.FindMapping(method.ReturnType, storeType)
                : _typeMappingSource.FindMapping(method.ReturnType);

            if (instance != null)
            {
                var instanceMapping = _typeMappingSource.FindMapping(instance.Type, storeType);
                instance = _sqlExpressionFactory.ApplyTypeMapping(instance, instanceMapping);

                return _sqlExpressionFactory.Function(
                    instance,
                    functionName,
                    Simplify(arguments),
                    nullable: true,
                    instancePropagatesNullability: true,
                    argumentsPropagateNullability: arguments.Select(a => true),
                    method.ReturnType,
                    resultTypeMapping);
            }

            return _sqlExpressionFactory.Function(
                functionName,
                Simplify(arguments),
                nullable: true,
                argumentsPropagateNullability: arguments.Select(a => true),
                method.ReturnType,
                resultTypeMapping);
        }

        return null;
    }

    private IEnumerable<SqlExpression> Simplify(IEnumerable<SqlExpression> arguments)
    {
        foreach (var argument in arguments)
        {
            if (argument is SqlConstantExpression constant
                && constant.Value is HierarchyId hierarchyId)
            {
                yield return _sqlExpressionFactory.Fragment($"'{hierarchyId}'");
            }
            else
            {
                yield return argument;
            }
        }
    }
}
