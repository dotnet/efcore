// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Represents the mapping between a .NET <see cref="DateTime" /> type and a database type.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class DateTimeTypeMapping : RelationalTypeMapping
    {
        private const string DateTimeFormatConst = @"{0:yyyy-MM-dd HH\:mm\:ss.fffffff}";

        /// <summary>
        ///     Initializes a new instance of the <see cref="DateTimeTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="dbType"> The <see cref="DbType" /> to be used. </param>
        public DateTimeTypeMapping(
            string storeType,
            DbType? dbType = null)
            : base(storeType, typeof(DateTime), dbType)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DateTimeTypeMapping" /> class.
        /// </summary>
        /// <param name="parameters"> Parameter object for <see cref="RelationalTypeMapping" />. </param>
        protected DateTimeTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="parameters"> The parameters for this mapping. </param>
        /// <returns> The newly created mapping. </returns>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new DateTimeTypeMapping(parameters);

        /// <summary>
        ///     Gets the string format to be used to generate SQL literals of this type.
        /// </summary>
        protected override string SqlLiteralFormatString
            => "TIMESTAMP '" + DateTimeFormatConst + "'";
    }
}
