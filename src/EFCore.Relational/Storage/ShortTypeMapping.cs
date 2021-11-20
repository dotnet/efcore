// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Represents the mapping between a .NET <see cref="short" /> type and a database type.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    public class ShortTypeMapping : RelationalTypeMapping
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ShortTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType">The name of the database type.</param>
        /// <param name="dbType">The <see cref="DbType" /> to be used.</param>
        public ShortTypeMapping(
            string storeType,
            DbType? dbType = System.Data.DbType.Int16)
            : base(storeType, typeof(short), dbType)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ShortTypeMapping" /> class.
        /// </summary>
        /// <param name="parameters">Parameter object for <see cref="RelationalTypeMapping" />.</param>
        protected ShortTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="parameters">The parameters for this mapping.</param>
        /// <returns>The newly created mapping.</returns>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new ShortTypeMapping(parameters);
    }
}
