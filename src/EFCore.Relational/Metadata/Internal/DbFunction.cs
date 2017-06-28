// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     Represents a db function in an <see cref="IModel" />.
    /// </summary>
    public class DbFunction : IMutableDbFunction, IMethodCallTranslator
    {
        public static DbFunction GetOrAddDbFunction(
            [NotNull] IMutableModel model,
            [NotNull] MethodInfo methodInfo,
            [NotNull] string annotationPrefix)
            => FindDbFunction(model, annotationPrefix, methodInfo)
               ?? new DbFunction(methodInfo, model, annotationPrefix);

        public static DbFunction FindDbFunction(
            [NotNull] IModel model,
            [NotNull] string annotationPrefix,
            [NotNull] MethodInfo methodInfo)
        {
            Check.NotNull(model, nameof(model));
            Check.NotEmpty(annotationPrefix, nameof(annotationPrefix));
            Check.NotNull(methodInfo, nameof(methodInfo));

            return model[BuildAnnotationName(annotationPrefix, methodInfo)] as DbFunction;
        }

        private string _schema;
        private string _functionName;

        private ConfigurationSource? _schemaConfigurationSource;
        private ConfigurationSource? _nameConfigurationSource;

        private DbFunction(
            [NotNull] MethodInfo methodInfo,
            [NotNull] IMutableModel model,
            [NotNull] string annotationPrefix)
        {
            Check.NotNull(methodInfo, nameof(methodInfo));
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotationPrefix, nameof(annotationPrefix));

            if (methodInfo.IsGenericMethod)
            {
                throw new ArgumentException(RelationalStrings.DbFunctionGenericMethodNotSupported(methodInfo.DisplayName()));
            }

            if (!methodInfo.IsStatic)
            {
                throw new ArgumentException(RelationalStrings.DbFunctionMethodMustBeStatic(methodInfo.DisplayName()));
            }

            if (methodInfo.ReturnType == null
                || methodInfo.ReturnType == typeof(void))
            {
                throw new ArgumentException(
                    RelationalStrings.DbFunctionInvalidReturnType(methodInfo.DisplayName(), methodInfo.ReturnType.ShortDisplayName()));
            }

            MethodInfo = methodInfo;

            model[BuildAnnotationName(annotationPrefix, methodInfo)] = this;
        }

        public static IEnumerable<IDbFunction> GetDbFunctions([NotNull] IModel model, [NotNull] string annotationPrefix)
        {
            Check.NotNull(model, nameof(model));
            Check.NotEmpty(annotationPrefix, nameof(annotationPrefix));

            return model.GetAnnotations()
                .Where(a => a.Name.StartsWith(annotationPrefix, StringComparison.Ordinal))
                .Select(a => a.Value)
                .Cast<IDbFunction>();
        }

        private static string BuildAnnotationName(string annotationPrefix, MethodBase methodBase)
            => $@"{annotationPrefix}{methodBase.Name}({string.Join(",", methodBase.GetParameters().Select(p => p.ParameterType.Name))})";

        /// <summary>
        ///     The schema where the function lives in the underlying datastore.
        /// </summary>
        public virtual string Schema
        {
            get => _schema;
            [param: CanBeNull] set => SetSchema(value, ConfigurationSource.Explicit);
        }

        public virtual void SetSchema([CanBeNull] string schema, ConfigurationSource configurationSource)
        {
            _schema = schema;
            
            UpdateSchemaConfigurationSource(configurationSource);
        }

        private void UpdateSchemaConfigurationSource(ConfigurationSource configurationSource)
            => _schemaConfigurationSource = configurationSource.Max(_schemaConfigurationSource);

        public virtual ConfigurationSource? GetSchemaConfigurationSource() => _schemaConfigurationSource;

        /// <summary>
        ///     The name of the function in the underlying datastore.
        /// </summary>
        public virtual string FunctionName
        {
            get => _functionName;
            [param: NotNull] set => SetFunctionName(value, ConfigurationSource.Explicit);
        }

        public virtual void SetFunctionName([NotNull] string functionName, ConfigurationSource configurationSource)
        {
            Check.NotNull(functionName, nameof(functionName));

            _functionName = functionName;
            
            UpdateNameConfigurationSource(configurationSource);
        }

        private void UpdateNameConfigurationSource(ConfigurationSource configurationSource)
            => _nameConfigurationSource = configurationSource.Max(_nameConfigurationSource);

        public virtual ConfigurationSource? GetNameConfigurationSource() => _nameConfigurationSource;

        /// <summary>
        ///     The method which maps to the function in the underlying datastore
        /// </summary>
        public virtual MethodInfo MethodInfo { get; }

        /// <summary>
        ///     A method for converting a method call into sql.
        /// </summary>
        public virtual Func<IReadOnlyCollection<Expression>, Expression> Translation { get; [param: CanBeNull] set; }

        Expression IMethodCallTranslator.Translate(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            return Translation?.Invoke(methodCallExpression.Arguments)
                   ?? new SqlFunctionExpression(FunctionName, MethodInfo.ReturnType, Schema, methodCallExpression.Arguments);
        }
    }
}
