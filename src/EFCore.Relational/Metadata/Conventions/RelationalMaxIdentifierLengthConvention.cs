// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the maximum object identifier length supported by the database.
    /// </summary>
    public class RelationalMaxIdentifierLengthConvention : IModelInitializedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RelationalMaxIdentifierLengthConvention" />.
        /// </summary>
        /// <param name="maxIdentifierLength"> The maximum object identifier length supported by the database. </param>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public RelationalMaxIdentifierLengthConvention(
            int maxIdentifierLength,
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
        {
            MaxIdentifierLength = maxIdentifierLength;
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     The maximum object identifier length supported by the database.
        /// </summary>
        public virtual int MaxIdentifierLength { get; }

        /// <summary>
        ///     Called after a model is initialized.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessModelInitialized(
            IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            modelBuilder.Metadata.Builder.HasMaxIdentifierLength(MaxIdentifierLength);
        }
    }
}
