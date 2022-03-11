// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the DeleteBehavior based on the <see cref="DeleteBehaviorAttribute" /> applied on the property.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class DeleteBehaviorAttributeConvention : IForeignKeyAddedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="UnicodeAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public DeleteBehaviorAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }
    
    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Called after a foreign key is added to the entity type.
    /// </summary>
    /// <param name="foreignKeyBuilder">The builder for the foreign key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public void ProcessForeignKeyAdded(IConventionForeignKeyBuilder foreignKeyBuilder, IConventionContext<IConventionForeignKeyBuilder> context)
    {
        var foreignKey = foreignKeyBuilder.Metadata;
        var properties = foreignKey.Properties;
        foreach (var property in properties)
        {
            var attribute = property?.PropertyInfo?.GetCustomAttribute<DeleteBehaviorAttribute>();
            if (attribute != null)
            {
                var deleteBehavior = (DeleteBehavior)attribute.Behavior;
                foreignKey.SetDeleteBehavior(deleteBehavior);
            }
        }
    }
}
