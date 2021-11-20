// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Represents the mapping between a .NET <see cref="byte" /> type and a database type.
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
    public class ByteTypeMapping : RelationalTypeMapping
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ByteTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType">The name of the database type.</param>
        /// <param name="dbType">The <see cref="DbType" /> to be used.</param>
        public ByteTypeMapping(
            string storeType,
            DbType? dbType = System.Data.DbType.Byte)
            : base(storeType, typeof(byte), dbType)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ByteTypeMapping" /> class.
        /// </summary>
        /// <param name="parameters">Parameter object for <see cref="RelationalTypeMapping" />.</param>
        protected ByteTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="parameters">The parameters for this mapping.</param>
        /// <returns>The newly created mapping.</returns>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new ByteTypeMapping(parameters);
    }
}
