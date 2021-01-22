// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

#nullable enable

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that manipulates names of database objects for entity types that share a table to avoid clashes.
    /// </summary>
    public class SqlServerSharedTableConvention : SharedTableConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="SqlServerSharedTableConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public SqlServerSharedTableConvention(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        /// <inheritdoc />
        protected override bool AreCompatible(IReadOnlyKey key, IReadOnlyKey duplicateKey, in StoreObjectIdentifier storeObject)
            => base.AreCompatible(key, duplicateKey, storeObject)
                && key.AreCompatibleForSqlServer(duplicateKey, storeObject, shouldThrow: false);

        /// <inheritdoc />
        protected override bool AreCompatible(IReadOnlyIndex index, IReadOnlyIndex duplicateIndex, in StoreObjectIdentifier storeObject)
            => base.AreCompatible(index, duplicateIndex, storeObject)
                && index.AreCompatibleForSqlServer(duplicateIndex, storeObject, shouldThrow: false);
    }
}
