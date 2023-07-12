// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     <para>
///         Initializes a <see cref="IModel" /> with the runtime dependencies.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public class RelationalModelRuntimeInitializer : ModelRuntimeInitializer
{
    /// <summary>
    ///     Creates a new <see cref="ModelRuntimeInitializer" /> instance.
    /// </summary>
    /// <param name="dependencies">The dependencies to use.</param>
    /// <param name="relationalDependencies">The relational dependencies to use.</param>
    public RelationalModelRuntimeInitializer(
        ModelRuntimeInitializerDependencies dependencies,
        RelationalModelRuntimeInitializerDependencies relationalDependencies)
        : base(dependencies)
    {
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalModelRuntimeInitializerDependencies RelationalDependencies { get; }

    /// <summary>
    ///     Initializes the given model with runtime dependencies.
    /// </summary>
    /// <param name="model">The model to initialize.</param>
    /// <param name="designTime">Whether the model should contain design-time configuration.</param>
    /// <param name="prevalidation">
    ///     <see langword="true" /> indicates that only pre-validation initialization should be performed;
    ///     <see langword="false" /> indicates that only post-validation initialization should be performed.
    /// </param>
    protected override void InitializeModel(IModel model, bool designTime, bool prevalidation)
    {
        if (prevalidation)
        {
            model.SetRuntimeAnnotation(RelationalAnnotationNames.ModelDependencies, RelationalDependencies.RelationalModelDependencies);
        }
        else if (model.FindRuntimeAnnotation(RelationalAnnotationNames.RelationalModel) == null)
        {
            RelationalModel.Add(
                model,
                RelationalDependencies.RelationalAnnotationProvider,
                (IRelationalTypeMappingSource)Dependencies.ModelDependencies.TypeMappingSource,
                designTime);
        }
    }
}
