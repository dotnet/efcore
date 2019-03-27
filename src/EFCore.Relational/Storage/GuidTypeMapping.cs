// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Represents the mapping between a .NET <see cref="Guid" /> type and a database type.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class GuidTypeMapping : RelationalTypeMapping
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GuidTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="dbType"> The <see cref="DbType" /> to be used. </param>
        public GuidTypeMapping(
            [NotNull] string storeType,
            DbType? dbType = null)
            : base(storeType, typeof(Guid), dbType)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GuidTypeMapping" /> class.
        /// </summary>
        /// <param name="parameters"> Parameter object for <see cref="RelationalTypeMapping" />. </param>
        protected GuidTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="parameters"> The parameters for this mapping. </param>
        /// <returns> The newly created mapping. </returns>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new GuidTypeMapping(parameters);

        /// <summary>
        ///     Gets the string format to be used to generate SQL literals of this type.
        /// </summary>
        protected override string SqlLiteralFormatString => "'{0}'";
    }
}
