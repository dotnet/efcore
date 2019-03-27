// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
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
            DbType? dbType = null,
            int? size = null)
            : base(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(
                        typeof(byte[])), storeType, StoreTypePostfix.None, dbType, unicode: false, size))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ByteArrayTypeMapping" /> class.
        /// </summary>
        /// <param name="parameters"> Parameter object for <see cref="RelationalTypeMapping" />. </param>
        protected ByteArrayTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="parameters"> The parameters for this mapping. </param>
        /// <returns> The newly created mapping. </returns>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new ByteArrayTypeMapping(parameters);

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
