// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Initializes a <see cref="IModel" /> with the runtime dependencies.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class RelationalModelRuntimeInitializer : ModelRuntimeInitializer
    {
        /// <summary>
        ///     Creates a new <see cref="ModelRuntimeInitializer" /> instance.
        /// </summary>
        /// <param name="dependencies"> The dependencies to use. </param>
        /// <param name="relationalDependencies"> The relational dependencies to use. </param>
        public RelationalModelRuntimeInitializer(
            ModelRuntimeInitializerDependencies dependencies,
            RelationalModelRuntimeInitializerDependencies relationalDependencies)
            : base(dependencies)
        {
            Check.NotNull(relationalDependencies, nameof(relationalDependencies));

            RelationalDependencies = relationalDependencies;
        }

        /// <summary>
        ///     The relational dependencies.
        /// </summary>
        protected virtual RelationalModelRuntimeInitializerDependencies RelationalDependencies { get; }

        /// <summary>
        ///     Initializes the given model with runtime dependencies.
        /// </summary>
        /// <param name="model"> The model to initialize. </param>
        /// <param name="preValidation">
        ///     <see langword="true"/> indicates that only pre-validation initialization should be performed;
        ///     <see langword="false"/> indicates that only post-validation initialization should be performed.
        /// </param>
        protected override void InitializeModel(IModel model, bool preValidation)
        {
            if (preValidation)
            {
                model.AddRuntimeAnnotation(RelationalAnnotationNames.ModelDependencies, RelationalDependencies.RelationalModelDependencies);
            }
            else
            {
                RelationalModel.Add(model, RelationalDependencies.RelationalAnnotationProvider);
            }
        }
    }
}
