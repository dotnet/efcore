// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a relational database function in a model.
    /// </summary>
    public class SlimDbFunction : AnnotatableBase, IRuntimeDbFunction
    {
        private readonly List<SlimDbFunctionParameter> _parameters = new();
        private readonly MethodInfo? _methodInfo;
        private readonly Type _returnType;
        private readonly bool _isScalar;
        private readonly bool _isAggregate;
        private readonly bool _isNullable;
        private readonly bool _isBuiltIn;
        private readonly string _storeName;
        private readonly string? _schema;
        private readonly string? _storeType;
        private readonly Func<IReadOnlyList<SqlExpression>, SqlExpression>? _translation;
        private RelationalTypeMapping? _typeMapping;
        private IStoreFunction? _storeFunction;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public SlimDbFunction(
            string modelName,
            SlimModel model,
            MethodInfo? methodInfo,
            Type returnType,
            bool scalar,
            bool aggregate,
            bool nullable,
            bool builtIn,
            string storeName,
            string? schema,
            string? storeType,
            RelationalTypeMapping? typeMapping = null,
            Func<IReadOnlyList<SqlExpression>, SqlExpression>? translation = null)
        {
            ModelName = modelName;
            Model = model;
            _returnType = returnType;
            _methodInfo = methodInfo;
            _isScalar = scalar;
            _isAggregate = aggregate;
            _isNullable = nullable;
            _isBuiltIn = builtIn;
            _storeName = storeName;
            _schema = schema;
            _storeType = storeType;
            _typeMapping = typeMapping;
            _translation = translation;
        }

        /// <summary>
        ///     Gets the model in which this function is defined.
        /// </summary>
        public virtual SlimModel Model { get; }

        /// <summary>
        ///     Gets the name of the function in the model.
        /// </summary>
        public virtual string ModelName { get; }

        /// <summary>
        ///     Adds a parameter to the function.
        /// </summary>
        /// <param name="name"> The parameter name. </param>
        /// <param name="clrType"> The parameter type. </param>
        /// <param name="propagatesNullability"> A value which indicates whether the parameter propagates nullability. </param>
        /// <param name="storeType"> The store type of this parameter. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> for this parameter. </param>
        /// <returns> The new parameter. </returns>
        public virtual SlimDbFunctionParameter AddParameter(
            string name,
            Type clrType,
            bool propagatesNullability,
            string storeType,
            RelationalTypeMapping? typeMapping = null)
        {
            var slimFunctionParameter = new SlimDbFunctionParameter(this,
                name,
                clrType,
                propagatesNullability,
                storeType,
                typeMapping);

            _parameters.Add(slimFunctionParameter);
            return slimFunctionParameter;
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        public override string ToString()
            => ((IDbFunction)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public virtual DebugView DebugView
            => new(
                () => ((IDbFunction)this).ToDebugString(MetadataDebugStringOptions.ShortDefault),
                () => ((IDbFunction)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

        /// <inheritdoc />
        IReadOnlyModel IReadOnlyDbFunction.Model
        {
            [DebuggerStepThrough]
            get => Model;
        }

        /// <inheritdoc />
        IModel IDbFunction.Model
        {
            [DebuggerStepThrough]
            get => Model;
        }

        /// <inheritdoc />
        IReadOnlyList<IReadOnlyDbFunctionParameter> IReadOnlyDbFunction.Parameters
        {
            [DebuggerStepThrough]
            get => _parameters;
        }

        /// <inheritdoc />
        IReadOnlyList<IDbFunctionParameter> IDbFunction.Parameters
        {
            [DebuggerStepThrough]
            get => _parameters;
        }

        /// <inheritdoc />
        MethodInfo? IReadOnlyDbFunction.MethodInfo
        {
            [DebuggerStepThrough]
            get => _methodInfo;
        }

        /// <inheritdoc />
        Type IReadOnlyDbFunction.ReturnType
        {
            [DebuggerStepThrough]
            get => _returnType;
        }

        /// <inheritdoc />
        bool IReadOnlyDbFunction.IsScalar
        {
            [DebuggerStepThrough]
            get => _isScalar;
        }

        /// <inheritdoc />
        bool IReadOnlyDbFunction.IsAggregate
        {
            [DebuggerStepThrough]
            get => _isAggregate;
        }

        /// <inheritdoc />
        bool IReadOnlyDbFunction.IsBuiltIn
        {
            [DebuggerStepThrough]
            get => _isBuiltIn;
        }

        /// <inheritdoc />
        bool IReadOnlyDbFunction.IsNullable
        {
            [DebuggerStepThrough]
            get => _isNullable;
        }

        /// <inheritdoc />
        IStoreFunction IDbFunction.StoreFunction
        {
            [DebuggerStepThrough]
            get => _storeFunction!;
        }

        IStoreFunction IRuntimeDbFunction.StoreFunction
        {
            get => _storeFunction!;
            set => _storeFunction = value;
        }

        /// <inheritdoc />
        string IReadOnlyDbFunction.Name
        {
            [DebuggerStepThrough]
            get => _storeName;
        }

        /// <inheritdoc />
        string? IReadOnlyDbFunction.Schema
        {
            [DebuggerStepThrough]
            get => _schema;
        }

        /// <inheritdoc />
        string? IReadOnlyDbFunction.StoreType
        {
            [DebuggerStepThrough]
            get => _storeType;
        }

        /// <inheritdoc />
        Func<IReadOnlyList<SqlExpression>, SqlExpression>? IReadOnlyDbFunction.Translation
        {
            [DebuggerStepThrough]
            get => _translation;
        }

        /// <inheritdoc />
        RelationalTypeMapping? IReadOnlyDbFunction.TypeMapping
        {
            [DebuggerStepThrough]
            get => _isScalar
                    ? NonCapturingLazyInitializer.EnsureInitialized(ref _typeMapping, this, static dbFunction =>
                        {
                            var relationalTypeMappingSource =
                                (IRelationalTypeMappingSource)((IModel)dbFunction.Model).GetModelDependencies().TypeMappingSource;
                            return !string.IsNullOrEmpty(dbFunction._storeType)
                                        ? relationalTypeMappingSource.FindMapping(dbFunction._storeType)!
                                        : relationalTypeMappingSource.FindMapping(dbFunction._returnType)!;
                        })
                    : _typeMapping;
        }
    }
}
