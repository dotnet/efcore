﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that ensures that <see cref="IReadOnlyDbFunction.Schema" /> is populated for database functions which
    ///     have <see cref="IReadOnlyDbFunction.IsBuiltIn" /> flag set to <see langword="false" />.
    /// </summary>
    public class SqlServerDbFunctionConvention : IModelFinalizingConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="SqlServerDbFunctionConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public SqlServerDbFunctionConvention(
            ProviderConventionSetBuilderDependencies dependencies,
            RelationalConventionSetBuilderDependencies relationalDependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <inheritdoc />
        public virtual void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var dbFunction in modelBuilder.Metadata.GetDbFunctions())
            {
                if (!dbFunction.IsBuiltIn
                    && string.IsNullOrEmpty(dbFunction.Schema))
                {
                    dbFunction.SetSchema("dbo");
                }
            }
        }
    }
}
