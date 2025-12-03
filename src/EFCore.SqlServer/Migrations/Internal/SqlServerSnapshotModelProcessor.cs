// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
#pragma warning disable EF1001 // Internal EF Core API usage.
public class SqlServerSnapshotModelProcessor : SnapshotModelProcessor
#pragma warning restore EF1001 // Internal EF Core API usage.
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
#pragma warning disable EF1001 // Internal EF Core API usage.
    public SqlServerSnapshotModelProcessor(
        IOperationReporter operationReporter,
        IModelRuntimeInitializer modelRuntimeInitializer)
        : base(operationReporter, modelRuntimeInitializer)
#pragma warning restore EF1001 // Internal EF Core API usage.
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IModel? Process(IReadOnlyModel? model, bool resetVersion = false)
    {
#pragma warning disable EF1001 // Internal EF Core API usage.
        if (model is Model mutableModel
            && !mutableModel.IsReadOnly)
        {
            var version = model.GetProductVersion();
            if (version != null && IsVersion9OrOlder(version))
            {
                foreach (var entityType in model.GetEntityTypes())
                {
                    ProcessJsonContainerColumnType(entityType);
                }
            }
        }

        return base.Process(model, resetVersion);
#pragma warning restore EF1001 // Internal EF Core API usage.
    }

    private static bool IsVersion9OrOlder(string version)
        => version.StartsWith("9.", StringComparison.Ordinal)
            || version.StartsWith("8.", StringComparison.Ordinal)
            || version.StartsWith("7.", StringComparison.Ordinal)
            || version.StartsWith("6.", StringComparison.Ordinal)
            || version.StartsWith("5.", StringComparison.Ordinal)
            || version.StartsWith("4.", StringComparison.Ordinal)
            || version.StartsWith("3.", StringComparison.Ordinal)
            || version.StartsWith("2.", StringComparison.Ordinal)
            || version.StartsWith("1.", StringComparison.Ordinal);

    private static void ProcessJsonContainerColumnType(IReadOnlyEntityType entityType)
    {
        // Only process top-level owned types mapped to JSON that don't have container column type set
        if (entityType is IMutableEntityType mutableEntityType
            && entityType.IsOwned()
            && entityType.FindOwnership() is { } ownership
            && !ownership.PrincipalEntityType.IsOwned()) // top-level owned type
        {
            // Check if it has ContainerColumnName annotation (mapped to JSON) but no ContainerColumnType
            var containerColumnName = entityType.FindAnnotation(RelationalAnnotationNames.ContainerColumnName);
            if (containerColumnName?.Value is string
                && entityType.FindAnnotation(RelationalAnnotationNames.ContainerColumnType) == null)
            {
                // Set the container column type to nvarchar(max) which was the default in EF Core 9
                mutableEntityType.SetAnnotation(
                    RelationalAnnotationNames.ContainerColumnType,
                    "nvarchar(max)");
            }
        }
    }
}
