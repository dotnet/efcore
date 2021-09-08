// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics;
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
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information.
    /// </remarks>
    public class ModelRuntimeInitializer : IModelRuntimeInitializer
    {
        private static readonly object _syncObject = new();

        /// <summary>
        ///     Creates a new <see cref="ModelRuntimeInitializer" /> instance.
        /// </summary>
        /// <param name="dependencies"> The dependencies to use. </param>
        public ModelRuntimeInitializer(ModelRuntimeInitializerDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies for this service.
        /// </summary>
        protected virtual ModelRuntimeInitializerDependencies Dependencies { get; }

        /// <summary>
        ///     Validates and initializes the given model with runtime dependencies.
        /// </summary>
        /// <param name="model"> The model to initialize. </param>
        /// <param name="designTime"> Whether the model should contain design-time configuration.</param>
        /// <param name="validationLogger"> The validation logger. </param>
        /// <returns> The initialized model. </returns>
        public virtual IModel Initialize(
            IModel model,
            bool designTime = true,
            IDiagnosticsLogger<DbLoggerCategory.Model.Validation>? validationLogger = null)
        {
            if (model is Model mutableModel
                && !mutableModel.IsReadOnly)
            {
                lock (_syncObject)
                {
                    if (!mutableModel.IsReadOnly)
                    {
                        model = mutableModel.FinalizeModel();
                    }
                }
            }

            if (model.ModelDependencies == null)
            {
                // Make sure InitializeModel really only gets called once, since it may not be thread safe or idempotent.
                lock (_syncObject)
                {
                    if (model.ModelDependencies == null)
                    {
                        model.ModelDependencies = Dependencies.ModelDependencies;

                        InitializeModel(model, designTime, prevalidation: true);

                        if (validationLogger != null
                            && model is IConventionModel)
                        {
                            Dependencies.ModelValidator.Validate(model, validationLogger);
                        }

                        InitializeModel(model, designTime, prevalidation: false);
                    }
                }
            }

            if (designTime)
            {
                return model;
            }

            model = model.GetOrAddRuntimeAnnotationValue(
                CoreAnnotationNames.ReadOnlyModel,
                static model =>
                {
                    if (model is Model mutableModel)
                    {
                        // This assumes OnModelFinalized is thread-safe
                        model = mutableModel.OnModelFinalized();
                    }

                    return model!;
                },
                model);

            return model;
        }

        /// <summary>
        ///     Initializes the given model with runtime dependencies.
        /// </summary>
        /// <param name="model"> The model to initialize. </param>
        /// <param name="designTime"> Whether the model should contain design-time configuration. </param>
        /// <param name="prevalidation">
        ///     <see langword="true"/> indicates that only pre-validation initialization should be performed;
        ///     <see langword="false"/> indicates that only post-validation initialization should be performed.
        /// </param>
        protected virtual void InitializeModel(IModel model, bool designTime, bool prevalidation)
        {
        }
    }
}
