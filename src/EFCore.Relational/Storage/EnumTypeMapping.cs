// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Globalization;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage
{
//LAJLAJ    /// <summary>
//LAJLAJ    ///     <para>
//LAJLAJ    ///         Represents the mapping between a .NET <see cref="char" /> type and a database type.
//LAJLAJ    ///     </para>
//LAJLAJ    ///     <para>
//LAJLAJ    ///         This type is typically used by database providers (and other extensions). It is generally
//LAJLAJ    ///         not used in application code.
//LAJLAJ    ///     </para>
//LAJLAJ    /// </summary>
//LAJLAJ    public class EnumTypeMapping : RelationalTypeMapping
//LAJLAJ    {
//LAJLAJ        /// <summary>
//LAJLAJ        ///     Initializes a new instance of the <see cref="EnumTypeMapping" /> class.
//LAJLAJ        /// </summary>
//LAJLAJ        /// <param name="storeType"> The name of the database type. </param>
//LAJLAJ        public EnumTypeMapping([NotNull] string storeType)
//LAJLAJ            : this(storeType, dbType: null, unicode: false, size: null)
//LAJLAJ        {
//LAJLAJ        }
//LAJLAJ
//LAJLAJ        /// <summary>
//LAJLAJ        ///     Initializes a new instance of the <see cref="EnumTypeMapping" /> class.
//LAJLAJ        /// </summary>
//LAJLAJ        /// <param name="storeType"> The name of the database type. </param>
//LAJLAJ        /// <param name="dbType"> The <see cref="System.Data.DbType" /> to be used. </param>
//LAJLAJ        /// <param name="unicode"> A value indicating whether the type should handle Unicode data or not. </param>
//LAJLAJ        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
//LAJLAJ        /// <param name="hasNonDefaultUnicode"> A value indicating whether the Unicode setting has been manually configured to a non-default value. </param>
//LAJLAJ        /// <param name="hasNonDefaultSize"> A value indicating whether the size setting has been manually configured to a non-default value. </param>
//LAJLAJ        public EnumTypeMapping(
//LAJLAJ            [NotNull] string storeType,
//LAJLAJ            [CanBeNull] DbType? dbType,
//LAJLAJ            bool unicode,
//LAJLAJ            int? size,
//LAJLAJ            bool hasNonDefaultUnicode = false,
//LAJLAJ            bool hasNonDefaultSize = false)
//LAJLAJ            : base(storeType, dbType, unicode, size, hasNonDefaultUnicode, hasNonDefaultSize)
//LAJLAJ        {
//LAJLAJ        }
//LAJLAJ
//LAJLAJ        /// <summary>
//LAJLAJ        ///     Creates a copy of this mapping.
//LAJLAJ        /// </summary>
//LAJLAJ        /// <param name="storeType"> The name of the database type. </param>
//LAJLAJ        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
//LAJLAJ        /// <returns> The newly created mapping. </returns>
//LAJLAJ        public override RelationalTypeMapping CreateCopy([NotNull] string storeType, int? size)
//LAJLAJ            => new EnumTypeMapping(
//LAJLAJ                storeType,
//LAJLAJ                DbType,
//LAJLAJ                IsUnicode,
//LAJLAJ                size,
//LAJLAJ                HasNonDefaultUnicode,
//LAJLAJ                hasNonDefaultSize: size != Size);
//LAJLAJ
//LAJLAJ        /// <summary>
//LAJLAJ        ///     Generates the SQL representation of a literal value.
//LAJLAJ        /// </summary>
//LAJLAJ        /// <param name="value">The literal value.</param>
//LAJLAJ        /// <returns>
//LAJLAJ        ///     The generated string.
//LAJLAJ        /// </returns>
//LAJLAJ        public override string GenerateSqlLiteral([CanBeNull]object value)
//LAJLAJ        {
//LAJLAJ            return value != null
//LAJLAJ                ? string.Format(CultureInfo.InvariantCulture, "{0:d}", value)
//LAJLAJ                : base.GenerateSqlLiteral(value);
//LAJLAJ        }
//LAJLAJ    }
}
