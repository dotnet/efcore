// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

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
        protected override bool AreCompatible(IKey key, IKey duplicateKey, string tableName, string schema)
            => base.AreCompatible(key, duplicateKey, tableName, schema)
                && key.AreCompatibleForSqlServer(duplicateKey, tableName, schema, shouldThrow: false);

        /// <inheritdoc />
        protected override bool AreCompatible(IIndex index, IIndex duplicateIndex, string tableName, string schema)
            => base.AreCompatible(index, duplicateIndex, tableName, schema)
                && index.AreCompatibleForSqlServer(duplicateIndex, tableName, schema, shouldThrow: false);
    }
}
