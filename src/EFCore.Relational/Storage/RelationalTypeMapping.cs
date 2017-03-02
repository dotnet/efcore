// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Represents the mapping between a .NET type and a database type.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class RelationalTypeMapping
    {
        /// <summary>
        ///     Gets the mapping to be used when the only piece of information is that there is a null value.
        /// </summary>
        public static readonly RelationalTypeMapping NullMapping = new RelationalTypeMapping("NULL");

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="clrType"> The .NET type. </param>
        public RelationalTypeMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType)
            : this(storeType, clrType, dbType: null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="clrType"> The .NET type. </param>
        /// <param name="dbType"> The <see cref="System.Data.DbType" /> to be used. </param>
        public RelationalTypeMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType,
            [CanBeNull] DbType? dbType)
            : this(storeType, clrType, dbType, unicode: false, size: null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="clrType"> The .NET type. </param>
        /// <param name="dbType"> The <see cref="System.Data.DbType" /> to be used. </param>
        /// <param name="unicode"> A value indicating whether the type should handle Unicode data or not. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        /// <param name="hasNonDefaultUnicode"> A value indicating whether the Unicode setting has been manually configured to a non-default value. </param>
        /// <param name="hasNonDefaultSize"> A value indicating whether the size setting has been manually configured to a non-default value. </param>
        public RelationalTypeMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType,
            [CanBeNull] DbType? dbType,
            bool unicode,
            int? size,
            bool hasNonDefaultUnicode = false,
            bool hasNonDefaultSize = false)
            : this(storeType)
        {
            Check.NotNull(clrType, nameof(clrType));

            ClrType = clrType;
            DbType = dbType;
            IsUnicode = unicode;
            Size = size;
            HasNonDefaultUnicode = hasNonDefaultUnicode;
            HasNonDefaultSize = hasNonDefaultSize;
        }

        private RelationalTypeMapping([NotNull] string storeType)
        {
            Check.NotEmpty(storeType, nameof(storeType));

            StoreType = storeType;
        }

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        /// <returns> The newly created mapping. </returns>
        public virtual RelationalTypeMapping CreateCopy([NotNull] string storeType, int? size)
            => new RelationalTypeMapping(
                storeType,
                ClrType,
                DbType,
                IsUnicode,
                size,
                HasNonDefaultUnicode,
                hasNonDefaultSize: size != Size);

        /// <summary>
        ///     Gets the name of the database type.
        /// </summary>
        public virtual string StoreType { get; }

        /// <summary>
        ///     Gets the .NET type.
        /// </summary>
        public virtual Type ClrType { get; }

        /// <summary>
        ///     Gets the <see cref="System.Data.DbType" /> to be used.
        /// </summary>
        public virtual DbType? DbType { get; }

        /// <summary>
        ///     Gets a value indicating whether the type should handle Unicode data or not.
        /// </summary>
        public virtual bool IsUnicode { get; }

        /// <summary>
        ///     Gets the size of data the property is configured to store, or null if no size is configured.
        /// </summary>
        public virtual int? Size { get; }

        /// <summary>
        ///     Gets a value indicating whether the Unicode setting has been manually configured to a non-default value.
        /// </summary>
        public virtual bool HasNonDefaultUnicode { get; }

        /// <summary>
        ///     Gets a value indicating whether the size setting has been manually configured to a non-default value.
        /// </summary>
        public virtual bool HasNonDefaultSize { get; }

        /// <summary>
        ///     Creates a <see cref="DbParameter" /> with the appropriate type information configured.
        /// </summary>
        /// <param name="command"> The command the parameter should be created on. </param>
        /// <param name="name"> The name of the parameter. </param>
        /// <param name="value"> The value to be assigned to the parameter. </param>
        /// <param name="nullable"> A value indicating whether the parameter should be a nullable type. </param>
        /// <returns></returns>
        public virtual DbParameter CreateParameter(
            [NotNull] DbCommand command,
            [NotNull] string name,
            [CanBeNull] object value,
            bool? nullable = null)
        {
            Check.NotNull(command, nameof(command));

            var parameter = command.CreateParameter();
            parameter.Direction = ParameterDirection.Input;
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;

            if (nullable.HasValue)
            {
                parameter.IsNullable = nullable.Value;
            }

            if (DbType.HasValue)
            {
                parameter.DbType = DbType.Value;
            }

            if (Size.HasValue)
            {
                parameter.Size = Size.Value;
            }

            ConfigureParameter(parameter);

            return parameter;
        }

        /// <summary>
        ///     Configures type information of a <see cref="DbParameter" />.
        /// </summary>
        /// <param name="parameter"> The parameter to be configured. </param>
        protected virtual void ConfigureParameter([NotNull] DbParameter parameter)
        {
        }
    }
}
