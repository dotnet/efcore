using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Storage;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.ExpressionTranslators
{
    internal class SqlServerHierarchyIdMethodTranslator : IMethodCallTranslator
    {
        private static readonly IDictionary<MethodInfo, string> _methodToFunctionName = new Dictionary<MethodInfo, string>
        {
            // instance methods
            { typeof(HierarchyId).GetRuntimeMethod(nameof(HierarchyId.GetAncestor), new[] { typeof(int) }), "GetAncestor" },
            { typeof(HierarchyId).GetRuntimeMethod(nameof(HierarchyId.GetDescendant), new[] { typeof(HierarchyId), typeof(HierarchyId) }), "GetDescendant" },
            { typeof(HierarchyId).GetRuntimeMethod(nameof(HierarchyId.GetLevel), Type.EmptyTypes), "GetLevel" },
            { typeof(HierarchyId).GetRuntimeMethod(nameof(HierarchyId.GetReparentedValue), new[] { typeof(HierarchyId), typeof(HierarchyId) }), "GetReparentedValue" },
            { typeof(HierarchyId).GetRuntimeMethod(nameof(HierarchyId.IsDescendantOf), new[] { typeof(HierarchyId) }), "IsDescendantOf" },
            { typeof(object).GetRuntimeMethod(nameof(HierarchyId.ToString), Type.EmptyTypes), "ToString" },

            // static methods
            { typeof(HierarchyId).GetRuntimeMethod(nameof(HierarchyId.GetRoot), Type.EmptyTypes), "hierarchyid::GetRoot" },
            { typeof(HierarchyId).GetRuntimeMethod(nameof(HierarchyId.Parse), new[] { typeof(string) }), "hierarchyid::Parse" },
        };

        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerHierarchyIdMethodTranslator(
            IRelationalTypeMappingSource typeMappingSource,
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _typeMappingSource = typeMappingSource;
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public SqlExpression Translate(
            SqlExpression instance,
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
                        simplify(arguments),
                        nullable: true,
                        instancePropagatesNullability: true,
                        argumentsPropagateNullability: arguments.Select(a => true),
                        method.ReturnType,
                        resultTypeMapping);
                }

                return _sqlExpressionFactory.Function(
                    functionName,
                    simplify(arguments),
                    nullable: true,
                    argumentsPropagateNullability: arguments.Select(a => true),
                    method.ReturnType,
                    resultTypeMapping);
            }

            return null;
        }

        private IEnumerable<SqlExpression> simplify(IEnumerable<SqlExpression> arguments)
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
}
