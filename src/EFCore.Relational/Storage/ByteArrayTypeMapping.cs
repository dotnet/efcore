// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Represents the mapping between a .NET <see cref="byte" /> array type and a database type.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class ByteArrayTypeMapping : RelationalTypeMapping
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ByteArrayTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="dbType"> The <see cref="DbType" /> to be used. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        public ByteArrayTypeMapping(
            [NotNull] string storeType,
            DbType? dbType,
            int? size)
            : this(storeType, null, null, dbType, size)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ByteArrayTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="dbType"> The <see cref="DbType" /> to be used. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        /// <param name="fixedLength"> A value indicating whether the type is constrained to fixed-length data. </param>
        public ByteArrayTypeMapping(
            [NotNull] string storeType,
            DbType? dbType = null,
            int? size = null,
            bool fixedLength = false)
            : this(storeType, null, null, dbType, size, fixedLength)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ByteArrayTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="converter"> Converts values to and from the store whenever this mapping is used. </param>
        /// <param name="comparer"> Supports custom value snapshotting and comparisons. </param>
        /// <param name="dbType"> The <see cref="DbType" /> to be used. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        /// <param name="fixedLength"> A value indicating whether the type is constrained to fixed-length data. </param>
        public ByteArrayTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] ValueConverter converter,
            [CanBeNull] ValueComparer comparer,
            DbType? dbType = null,
            int? size = null,
            bool fixedLength = false)
            : base(storeType, typeof(byte[]), converter, comparer, dbType, size: size, fixedLength: fixedLength)
        {
        }

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        /// <returns> The newly created mapping. </returns>
        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new ByteArrayTypeMapping(storeType, Converter, Comparer, DbType, size, IsFixedLength);

        /// <summary>
        ///    Returns a new copy of this type mapping with the given <see cref="ValueConverter"/>
        ///    added.
        /// </summary>
        /// <param name="converter"> The converter to use. </param>
        /// <returns> A new type mapping </returns>
        public override CoreTypeMapping Clone(ValueConverter converter)
            => new ByteArrayTypeMapping(StoreType, ComposeConverter(converter), Comparer, DbType, Size, IsFixedLength);

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        protected override string GenerateNonNullSqlLiteral(object value)
        {
            Check.NotNull(value, nameof(value));

            var stringBuilder = new StringBuilder();
            stringBuilder.Append("X'");

            foreach (var @byte in (byte[])value)
            {
                stringBuilder.Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
            }

            stringBuilder.Append("'");
            return stringBuilder.ToString();
        }
    }
}
