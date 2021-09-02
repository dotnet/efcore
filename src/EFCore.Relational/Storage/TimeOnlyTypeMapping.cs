// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Represents the mapping between a .NET <see cref="TimeOnly" /> type and a database type.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class TimeOnlyTypeMapping : RelationalTypeMapping
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeOnlyTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="dbType"> The <see cref="DbType" /> to be used. </param>
        public TimeOnlyTypeMapping(
            string storeType,
            DbType? dbType = System.Data.DbType.Time)
            : base(storeType, typeof(TimeOnly), dbType)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeOnlyTypeMapping" /> class.
        /// </summary>
        /// <param name="parameters"> Parameter object for <see cref="RelationalTypeMapping" />. </param>
        protected TimeOnlyTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="parameters"> The parameters for this mapping. </param>
        /// <returns> The newly created mapping. </returns>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new TimeOnlyTypeMapping(parameters);

        /// <inheritdoc />
        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var timeOnly = (TimeOnly)value;

            return timeOnly.Ticks % TimeSpan.TicksPerSecond == 0
                ? FormattableString.Invariant($@"TIME '{value:HH\:mm\:ss}'")
                : FormattableString.Invariant($@"TIME '{value:HH\:mm\:ss\.FFFFFFF}'");
        }
    }
}
