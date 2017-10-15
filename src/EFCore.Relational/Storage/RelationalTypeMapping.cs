// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
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
    public abstract class RelationalTypeMapping : CoreTypeMapping
    {
        /// <summary>
        ///     Gets the mapping to be used when the only piece of information is that there is a null value.
        /// </summary>
        public static readonly RelationalTypeMapping NullMapping = new NullTypeMapping("NULL");

        private class NullTypeMapping : RelationalTypeMapping
        {
            public NullTypeMapping(string storeType)
                : base(storeType, typeof(object))
            {
            }

            public override RelationalTypeMapping Clone(string storeType, int? size)
                => new NullTypeMapping(storeType);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="clrType"> The .NET type. </param>
        /// <param name="dbType"> The <see cref="System.Data.DbType" /> to be used. </param>
        /// <param name="unicode"> A value indicating whether the type should handle Unicode data or not. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        protected RelationalTypeMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType,
            DbType? dbType = null,
            bool unicode = false,
            int? size = null)
            : this(storeType, clrType, null, dbType, unicode, size)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="clrType"> The .NET type. </param>
        /// <param name="converter"> Converts types to and from the store whenever this mapping is used. </param>
        /// <param name="dbType"> The <see cref="System.Data.DbType" /> to be used. </param>
        /// <param name="unicode"> A value indicating whether the type should handle Unicode data or not. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        protected RelationalTypeMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType,
            [CanBeNull] ValueConverter converter,
            DbType? dbType = null,
            bool unicode = false,
            int? size = null)
            : base(clrType, converter)
        {
            Check.NotEmpty(storeType, nameof(storeType));

            StoreType = storeType;
            DbType = dbType;
            IsUnicode = unicode;
            Size = size;
        }

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        /// <returns> The newly created mapping. </returns>
        public abstract RelationalTypeMapping Clone([NotNull] string storeType, int? size);

        /// <summary>
        ///     Gets the name of the database type.
        /// </summary>
        public virtual string StoreType { get; }

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
        ///     Gets the string format to be used to generate SQL literals of this type.
        /// </summary>
        protected virtual string SqlLiteralFormatString { get; } = "{0}";

        /// <summary>
        ///     Creates a <see cref="DbParameter" /> with the appropriate type information configured.
        /// </summary>
        /// <param name="command"> The command the parameter should be created on. </param>
        /// <param name="name"> The name of the parameter. </param>
        /// <param name="value"> The value to be assigned to the parameter. </param>
        /// <param name="nullable"> A value indicating whether the parameter should be a nullable type. </param>
        /// <returns> The newly created parameter. </returns>
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

            if (value != null
                && value.GetType().IsEnum)
            {
                value = Convert.ChangeType(value, value.GetType().GetEnumUnderlyingType());
            }

            parameter.Value = (Converter != null
                                  ? Converter.ConvertToStore(value)
                                  : value)
                              ?? DBNull.Value;

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

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public virtual string GenerateSqlLiteral([CanBeNull] object value)
            => value == null
                ? "NULL"
                : GenerateNonNullSqlLiteral(value);

        /// <summary>
        ///     Generates the SQL representation of a non-null literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        protected virtual string GenerateNonNullSqlLiteral([NotNull] object value)
            => string.Format(CultureInfo.InvariantCulture, SqlLiteralFormatString, Check.NotNull(value, nameof(value)));
    }
}
