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
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    // Issue#11266 This type is being used by provider code. Do not break.
    public class DbFunction : IMutableDbFunction, IMethodCallTranslator
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static DbFunction GetOrAddDbFunction(
            [NotNull] IMutableModel model,
            [NotNull] MethodInfo methodInfo,
            [NotNull] string annotationPrefix)
            => FindDbFunction(model, annotationPrefix, methodInfo)
               ?? new DbFunction(methodInfo, model, annotationPrefix);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        private readonly IMutableModel _model;
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

            if (!methodInfo.IsStatic
                && !typeof(DbContext).IsAssignableFrom(methodInfo.DeclaringType))
            {
                throw new ArgumentException(
                    RelationalStrings.DbFunctionInvalidInstanceType(methodInfo.DisplayName(), methodInfo.DeclaringType.ShortDisplayName()));
            }

            if (methodInfo.ReturnType == null
                || methodInfo.ReturnType == typeof(void))
            {
                throw new ArgumentException(
                    RelationalStrings.DbFunctionInvalidReturnType(methodInfo.DisplayName(), methodInfo.ReturnType.ShortDisplayName()));
            }

            MethodInfo = methodInfo;

            _model = model;
            _model[BuildAnnotationName(annotationPrefix, methodInfo)] = this;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
            => $"{annotationPrefix}{methodBase.DeclaringType.ShortDisplayName()}{methodBase.Name}({string.Join(",", methodBase.GetParameters().Select(p => p.ParameterType.Name))})";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string DefaultSchema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Schema
        {
            get => _schema ?? _model.Relational().DefaultSchema ?? DefaultSchema;
            set => SetSchema(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetSchema([CanBeNull] string schema, ConfigurationSource configurationSource)
        {
            _schema = schema;

            UpdateSchemaConfigurationSource(configurationSource);
        }

        private void UpdateSchemaConfigurationSource(ConfigurationSource configurationSource)
            => _schemaConfigurationSource = configurationSource.Max(_schemaConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetSchemaConfigurationSource() => _schemaConfigurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string FunctionName
        {
            get => _functionName;
            set => SetFunctionName(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetFunctionName([NotNull] string functionName, ConfigurationSource configurationSource)
        {
            Check.NotNull(functionName, nameof(functionName));

            _functionName = functionName;

            UpdateNameConfigurationSource(configurationSource);
        }

        private void UpdateNameConfigurationSource(ConfigurationSource configurationSource)
            => _nameConfigurationSource = configurationSource.Max(_nameConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetNameConfigurationSource() => _nameConfigurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo MethodInfo { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<IReadOnlyCollection<Expression>, Expression> Translation { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        Expression IMethodCallTranslator.Translate(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            return Translation?.Invoke(methodCallExpression.Arguments)
                   ?? new SqlFunctionExpression(FunctionName, MethodInfo.ReturnType, Schema, methodCallExpression.Arguments);
        }
    }
}
