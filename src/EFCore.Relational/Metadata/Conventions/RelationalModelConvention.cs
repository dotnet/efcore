// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that precomputes a relational model.
    /// </summary>
    [Obsolete("Use IModelRuntimeInitializer.Initialize instead.")]
    public class RelationalModelConvention : IModelFinalizedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RelationalModelConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public RelationalModelConvention(
            ProviderConventionSetBuilderDependencies dependencies,
            RelationalConventionSetBuilderDependencies relationalDependencies)
        {
            RelationalDependencies = relationalDependencies;
        }

        /// <summary>
        ///     The service dependencies for <see cref="RelationalConventionSetBuilder" />
        /// </summary>
        protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

        /// <inheritdoc />
        public virtual IModel ProcessModelFinalized(IModel model)
            => RelationalModel.Add(model, RelationalDependencies.RelationalAnnotationProvider, designTime: true);
    }
}
