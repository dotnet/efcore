// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a function parameter.
    /// </summary>
    public class RuntimeDbFunctionParameter : AnnotatableBase, IRuntimeDbFunctionParameter
    {
        private readonly string _name;
        private readonly Type _clrType;
        private readonly bool _propagatesNullability;
        private readonly string _storeType;
        private IStoreFunctionParameter? _storeFunctionParameter;
        private RelationalTypeMapping? _typeMapping;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public RuntimeDbFunctionParameter(
            RuntimeDbFunction function,
            string name,
            Type clrType,
            bool propagatesNullability,
            string storeType,
            RelationalTypeMapping? typeMapping)
        {
            _name = name;
            Function = function;
            _clrType = clrType;
            _propagatesNullability = propagatesNullability;
            _storeType = storeType;
            _typeMapping = typeMapping;
        }

        /// <summary>
        ///     Gets the name of the function in the database.
        /// </summary>
        public virtual string Name
        {
            [DebuggerStepThrough]
            get => _name;
        }

        /// <summary>
        ///     Gets the function to which this parameter belongs.
        /// </summary>
        public virtual RuntimeDbFunction Function { get; }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        public override string ToString()
            => ((IDbFunctionParameter)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public virtual DebugView DebugView
            => new(
                () => ((IDbFunctionParameter)this).ToDebugString(MetadataDebugStringOptions.ShortDefault),
                () => ((IDbFunctionParameter)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

        /// <inheritdoc />
        IReadOnlyDbFunction IReadOnlyDbFunctionParameter.Function
        {
            [DebuggerStepThrough]
            get => Function;
        }

        /// <inheritdoc />
        IDbFunction IDbFunctionParameter.Function
        {
            [DebuggerStepThrough]
            get => Function;
        }

        /// <inheritdoc />
        IStoreFunctionParameter IDbFunctionParameter.StoreFunctionParameter
        {
            [DebuggerStepThrough]
            get => _storeFunctionParameter!;
        }

        IStoreFunctionParameter IRuntimeDbFunctionParameter.StoreFunctionParameter
        {
            get => _storeFunctionParameter!;
            set => _storeFunctionParameter = value;
        }

        /// <inheritdoc />
        Type IReadOnlyDbFunctionParameter.ClrType
        {
            [DebuggerStepThrough]
            get => _clrType;
        }

        /// <inheritdoc />
        string? IReadOnlyDbFunctionParameter.StoreType
        {
            [DebuggerStepThrough]
            get => _storeType;
        }

        /// <inheritdoc />
        string IDbFunctionParameter.StoreType
        {
            [DebuggerStepThrough]
            get => _storeType;
        }

        /// <inheritdoc />
        bool IReadOnlyDbFunctionParameter.PropagatesNullability
        {
            [DebuggerStepThrough]
            get => _propagatesNullability;
        }

        /// <inheritdoc />
        RelationalTypeMapping? IReadOnlyDbFunctionParameter.TypeMapping
            => NonCapturingLazyInitializer.EnsureInitialized(ref _typeMapping, this, static parameter =>
                {
                    var relationalTypeMappingSource =
                        (IRelationalTypeMappingSource)((IModel)parameter.Function.Model).GetModelDependencies().TypeMappingSource;
                    return relationalTypeMappingSource.FindMapping(parameter._storeType)!;
                });
    }
}
