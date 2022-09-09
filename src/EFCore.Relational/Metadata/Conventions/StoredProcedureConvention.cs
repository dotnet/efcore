// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that ensures that the entity type is current for the stored procedures.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> and
///     <see href="https://aka.ms/efcore-docs-inheritance">Entity type hierarchy mapping</see> for more information and examples.
/// </remarks>
public class StoredProcedureConvention : IEntityTypeAddedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="StoredProcedureConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public StoredProcedureConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
    {
        Dependencies = dependencies;
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    /// <summary>
    ///     Called after an entity type is added to the model.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context)
    {
        var entityType = entityTypeBuilder.Metadata;
        if (!entityType.HasSharedClrType)
        {
            return;
        }

        var sproc = (StoredProcedure?)entityType.GetDeleteStoredProcedure();
        if (sproc != null
            && sproc.EntityType != entityType)
        {
            sproc.EntityType = (IMutableEntityType)entityType;
        }

        sproc = (StoredProcedure?)entityType.GetInsertStoredProcedure();
        if (sproc != null
            && sproc.EntityType != entityType)
        {
            sproc.EntityType = (IMutableEntityType)entityType;
        }

        sproc = (StoredProcedure?)entityType.GetUpdateStoredProcedure();
        if (sproc != null
            && sproc.EntityType != entityType)
        {
            sproc.EntityType = (IMutableEntityType)entityType;
        }
    }
}
