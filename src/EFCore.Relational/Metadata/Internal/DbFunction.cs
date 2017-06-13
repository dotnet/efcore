// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly SortedDictionary<string, DbFunctionParameter> _parameters
            = new SortedDictionary<string, DbFunctionParameter>(StringComparer.OrdinalIgnoreCase);

        private string _schema;
        private string _name;
        private Type _returnType;

        private ConfigurationSource? _schemaConfigurationSource;
        private ConfigurationSource? _nameConfigurationSource;
        private ConfigurationSource? _returnTypeConfigurationSource;

        private DbFunction([NotNull] MethodInfo dbFunctionMethodInfo,
            [NotNull] IMutableModel model,
            [NotNull] string annotationPrefix,
            [CanBeNull] string name,
            [CanBeNull] string schema,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(dbFunctionMethodInfo, nameof(dbFunctionMethodInfo));
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotationPrefix, nameof(annotationPrefix));

            if (dbFunctionMethodInfo.IsGenericMethod)
                throw new ArgumentException(CoreStrings.DbFunctionGenericMethodNotSupported(dbFunctionMethodInfo));

            if (name != null)
                _nameConfigurationSource = configurationSource;

            if (schema != null)
                _schemaConfigurationSource = configurationSource;

            _returnTypeConfigurationSource = configurationSource;

            _name = name;
            _schema = schema;
            _returnType = dbFunctionMethodInfo.ReturnType;
            MethodInfo = dbFunctionMethodInfo;

            model[BuildAnnotationName(annotationPrefix, dbFunctionMethodInfo)] = this;
        }

        public static DbFunction GetOrAddDbFunction(
            [NotNull] IMutableModel model,
            [NotNull] MethodInfo methodInfo,
            [NotNull] string annotationPrefix,
            ConfigurationSource configurationSource,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
            => FindDbFunction(model, annotationPrefix, methodInfo)
               ?? new DbFunction(methodInfo, model, annotationPrefix, name, schema, configurationSource);

        public static DbFunction FindDbFunction([NotNull] IModel model,
            [NotNull] string annotationPrefix,
            [NotNull] MethodInfo methodInfo)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotationPrefix, nameof(annotationPrefix));
            Check.NotNull(methodInfo, nameof(methodInfo));

            return model[BuildAnnotationName(annotationPrefix, methodInfo)] as DbFunction;
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

        private static string BuildAnnotationName(string annotationPrefix, MethodInfo methodInfo)
            => $@"{annotationPrefix}{methodInfo.Name}({string.Join(",", methodInfo.GetParameters().Select(p => p.ParameterType.Name))})";

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
        public virtual string Name
        {
            get => _name;

            [param: NotNull] set => SetName(value, ConfigurationSource.Explicit);
        }

        public virtual void SetName([NotNull] string name, ConfigurationSource configurationSource)
        {
            Check.NotNull(name, nameof(name));

            _name = name;
            UpdateNameConfigurationSource(configurationSource);
        }

        private void UpdateNameConfigurationSource(ConfigurationSource configurationSource)
            => _nameConfigurationSource = configurationSource.Max(_nameConfigurationSource);

        public virtual ConfigurationSource? GetNameConfigurationSource() => _nameConfigurationSource;

        /// <summary>
        ///     The return type of the mapped .Net method
        /// </summary>
        public virtual Type ReturnType
        {
            get => _returnType;

            [param: NotNull] set => SetReturnType(value, ConfigurationSource.Explicit);
        }

        public virtual void SetReturnType([NotNull] Type returnType, ConfigurationSource configurationSource)
        {
            Check.NotNull(returnType, nameof(returnType));

            _returnType = returnType;
            UpdateReturnTypeConfigurationSource(configurationSource);
        }

        private void UpdateReturnTypeConfigurationSource(ConfigurationSource configurationSource)
            => _returnTypeConfigurationSource = configurationSource.Max(_returnTypeConfigurationSource);

        public virtual ConfigurationSource? GetReturnTypeConfigurationSource() => _nameConfigurationSource;

        /// <summary>
        ///     The list of parameters which are passed to the underlying datastores function.
        /// </summary>
        public virtual IReadOnlyList<DbFunctionParameter> Parameters => _parameters.Values.ToList();

        /// <summary>
        ///     The .Net method which maps to the function in the underlying datastore
        /// </summary>
        public virtual MethodInfo MethodInfo { get; }

        /// <summary>
        ///     If set this callback is used to translate the .Net method call to a Linq Expression.
        /// </summary>
        public virtual Func<IReadOnlyCollection<Expression>, IDbFunction, SqlFunctionExpression> TranslateCallback { get; [param: CanBeNull] set; }

        public virtual DbFunctionParameter AddParameter(string name)
            => AddParameter(name, ConfigurationSource.Explicit);

        public virtual DbFunctionParameter AddParameter([NotNull] string name, ConfigurationSource configurationSource)
        {
            Check.NotNull(name, nameof(name));

            var newParam = new DbFunctionParameter(name, configurationSource);

            _parameters.Add(newParam.Name, newParam);

            return newParam;
        }

        public virtual DbFunctionParameter FindParameter([NotNull] string name, ConfigurationSource configurationSource)
        {
            Check.NotNull(name, nameof(name));

            DbFunctionParameter parameter;

            if (_parameters.TryGetValue(name, out parameter))
                parameter.UpdateConfigurationSource(configurationSource);

            return parameter;
        }

        Expression IMethodCallTranslator.Translate(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            var methodArgs = methodCallExpression.Method.GetParameters().Zip(methodCallExpression.Arguments, (p, a) => new { Parameter = p, Argument = a });

            var arguments = new ReadOnlyCollection<Expression>(
                (from dbParam in Parameters
                 join methodArg in methodArgs on dbParam.Name equals methodArg.Parameter.Name into passParams
                 from passParam in passParams
                 orderby dbParam.Index
                 select passParam.Argument).ToList());

            return TranslateCallback?.Invoke(arguments, this)
                   ?? new SqlFunctionExpression(Name, ReturnType, Schema, arguments);
        }
    }
}
