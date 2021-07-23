// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that manipulates names of database objects for entity types that share a table to avoid clashes.
    /// </summary>
    public class SqliteSharedTableConvention : SharedTableConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="SqliteSharedTableConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public SqliteSharedTableConvention(
            ProviderConventionSetBuilderDependencies dependencies,
            RelationalConventionSetBuilderDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        /// <inheritdoc />
        protected override bool CheckConstraintsUniqueAcrossTables
            => false;
    }
}
