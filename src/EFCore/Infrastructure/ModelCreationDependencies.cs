// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
/// <remarks>
///     The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///     <see cref="DbContext" /> instance will use its own instance of this service.
///     The implementation may depend on other services registered with any lifetime.
///     The implementation does not need to be thread-safe.
/// </remarks>
public sealed record ModelCreationDependencies
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public ModelCreationDependencies(
        IModelSource modelSource,
        IConventionSetBuilder conventionSetBuilder,
        ModelDependencies modelDependencies,
        IModelRuntimeInitializer modelRuntimeInitializer,
        IDiagnosticsLogger<DbLoggerCategory.Model.Validation> validationLogger)
    {
        ModelSource = modelSource;
        ConventionSetBuilder = conventionSetBuilder;
        ModelDependencies = modelDependencies;
        ModelRuntimeInitializer = modelRuntimeInitializer;
        ValidationLogger = validationLogger;
    }

    /// <summary>
    ///     The model source.
    /// </summary>
    public IModelSource ModelSource { get; init; }

    /// <summary>
    ///     The convention set to use when creating the model.
    /// </summary>
    public IConventionSetBuilder ConventionSetBuilder { get; init; }

    /// <summary>
    ///     The dependencies object for the model.
    /// </summary>
    public ModelDependencies ModelDependencies { get; init; }

    /// <summary>
    ///     The model runtime initializer that will be used after the model building is finished.
    /// </summary>
    public IModelRuntimeInitializer ModelRuntimeInitializer { get; init; }

    /// <summary>
    ///     The validation logger.
    /// </summary>
    public IDiagnosticsLogger<DbLoggerCategory.Model.Validation> ValidationLogger { get; init; }
}
