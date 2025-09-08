// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    // TODO: Use as base class for all type mappings.
    /// <summary>
    /// The base class for mapping XG-specific types. It configures parameters with the
    /// <see cref="XGDbType"/> provider-specific type enum.
    /// </summary>
    public abstract class XGTypeMapping : RelationalTypeMapping
    {
        /// <summary>
        /// The database type used by XG.
        /// </summary>
        public virtual XGDbType XGDbType { get; }

        // ReSharper disable once PublicConstructorInAbstractClass
        public XGTypeMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType,
            XGDbType xgDbType,
            DbType? dbType = null,
            bool unicode = false,
            int? size = null,
            ValueConverter valueConverter = null,
            ValueComparer valueComparer = null,
            JsonValueReaderWriter jsonValueReaderWriter = null)
            : base(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(
                        clrType,
                        valueConverter,
                        valueComparer,
                        jsonValueReaderWriter: jsonValueReaderWriter),
                    storeType,
                    StoreTypePostfix.None,
                    dbType,
                    unicode,
                    size))
            => XGDbType = xgDbType;

        /// <summary>
        /// Constructs an instance of the <see cref="XGTypeMapping"/> class.
        /// </summary>
        /// <param name="parameters">The parameters for this mapping.</param>
        /// <param name="xgDbType">The database type of the range subtype.</param>
        protected XGTypeMapping(RelationalTypeMappingParameters parameters, XGDbType xgDbType)
            : base(parameters)
            => XGDbType = xgDbType;

        protected override void ConfigureParameter(DbParameter parameter)
        {
            if (!(parameter is XGParameters xgParameters))
            {
                throw new ArgumentException($"XG-specific type mapping {GetType()} being used with non-XG parameter type {parameter.GetType().Name}");
            }

            base.ConfigureParameter(parameter);

            xgParameters.m_DbType = XGDbType;
        }
    }
}
