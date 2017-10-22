// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.Converters;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Represents the mapping between a .NET <see cref="uint" /> type and a database type.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class UIntTypeMapping : RelationalTypeMapping
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UIntTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="dbType"> The <see cref="DbType" /> to be used. </param>
        public UIntTypeMapping(
            [NotNull] string storeType,
            DbType? dbType = null)
            : this(storeType, null, dbType)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UIntTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="converter"> Converts types to and from the store whenever this mapping is used. </param>
        /// <param name="dbType"> The <see cref="DbType" /> to be used. </param>
        public UIntTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] ValueConverter converter,
            DbType? dbType = null)
            : base(storeType, typeof(uint), converter, dbType)
        {
        }

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        /// <returns> The newly created mapping. </returns>
        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new UIntTypeMapping(storeType, Converter, DbType);

        /// <summary>
        ///    Returns a new copy of this type mapping with the given <see cref="ValueConverter"/>
        ///    added.
        /// </summary>
        /// <param name="converter"> The converter to use. </param>
        /// <returns> A new type mapping </returns>
        public override CoreTypeMapping Clone(ValueConverter converter)
            => new UIntTypeMapping(StoreType, ComposeConverter(converter), DbType);
    }
}
