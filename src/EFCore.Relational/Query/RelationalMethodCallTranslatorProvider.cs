// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         Provides translations for LINQ <see cref="MethodCallExpression" /> expressions by dispatching to multiple specialized
    ///         method call translators.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class RelationalMethodCallTranslatorProvider : IMethodCallTranslatorProvider
    {
        private readonly List<IMethodCallTranslator> _plugins = new();
        private readonly List<IMethodCallTranslator> _translators = new();
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     Creates a new instance of the <see cref="RelationalMethodCallTranslatorProvider" /> class.
        /// </summary>
        /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
        public RelationalMethodCallTranslatorProvider(RelationalMethodCallTranslatorProviderDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;

            _plugins.AddRange(dependencies.Plugins.SelectMany(p => p.Translators));

            var sqlExpressionFactory = dependencies.SqlExpressionFactory;

            _translators.AddRange(
                new IMethodCallTranslator[]
                {
                    new EqualsTranslator(sqlExpressionFactory),
                    new StringMethodTranslator(sqlExpressionFactory),
                    new CollateTranslator(),
                    new ContainsTranslator(sqlExpressionFactory),
                    new LikeTranslator(sqlExpressionFactory),
                    new EnumHasFlagTranslator(sqlExpressionFactory),
                    new GetValueOrDefaultTranslator(sqlExpressionFactory),
                    new ComparisonTranslator(sqlExpressionFactory),
                    new ByteArraySequenceEqualTranslator(sqlExpressionFactory),
                    new RandomTranslator(sqlExpressionFactory)
                });
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <summary>
        ///     Dependencies for this service.
        /// </summary>
        protected virtual RelationalMethodCallTranslatorProviderDependencies Dependencies { get; }

        /// <inheritdoc />
        public virtual SqlExpression? Translate(
            IModel model,
            SqlExpression? instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(logger, nameof(logger));

            var dbFunction = model.FindDbFunction(method);
            if (dbFunction != null)
            {
                if (dbFunction.Translation != null)
                {
                    return dbFunction.Translation.Invoke(
                        arguments.Select(e => _sqlExpressionFactory.ApplyDefaultTypeMapping(e)).ToList());
                }

                var argumentsPropagateNullability = dbFunction.Parameters.Select(p => p.PropagatesNullability);

                return dbFunction.IsBuiltIn
                    ? _sqlExpressionFactory.Function(
                        dbFunction.Name,
                        arguments,
                        dbFunction.IsNullable,
                        argumentsPropagateNullability,
                        method.ReturnType.UnwrapNullableType())
                    : _sqlExpressionFactory.Function(
                        dbFunction.Schema,
                        dbFunction.Name,
                        arguments,
                        dbFunction.IsNullable,
                        argumentsPropagateNullability,
                        method.ReturnType.UnwrapNullableType());
            }

            return _plugins.Concat(_translators)
                .Select(t => t.Translate(instance, method, arguments, logger))
                .FirstOrDefault(t => t != null);
        }

        /// <summary>
        ///     Adds additional translators which will take priority over existing registered translators.
        /// </summary>
        /// <param name="translators">Translators to add.</param>
        protected virtual void AddTranslators(IEnumerable<IMethodCallTranslator> translators)
        {
            Check.NotNull(translators, nameof(translators));

            _translators.InsertRange(0, translators);
        }
    }
}
