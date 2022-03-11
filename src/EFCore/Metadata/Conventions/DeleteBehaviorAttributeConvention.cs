// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the DeleteBehavior based on the <see cref="DeleteBehaviorAttribute" /> applied on the property.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class DeleteBehaviorAttributeConvention : PropertyAttributeConventionBase<DeleteBehaviorAttribute>, IForeignKeyAddedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="UnicodeAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public DeleteBehaviorAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies) { }

    /// <summary>
    ///     Called after a property is added to the entity type with an attribute on the associated CLR property or field.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="clrMember">The member that has the attribute.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    protected override void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        DeleteBehaviorAttribute attribute,
        MemberInfo clrMember,
        IConventionContext context)
    {
        if (!Enum.IsDefined(typeof(DeleteBehavior), attribute.Behavior))
        {
            throw new InvalidEnumArgumentException("This behavior is not defined in DeleteBehavior Enum.");
        }

        _deleteBehavior = (DeleteBehavior)attribute.Behavior;
    }

    /// <summary>
    ///     Called after a foreign key is added to the entity type.
    /// </summary>
    /// <param name="foreignKeyBuilder">The builder for the foreign key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public void ProcessForeignKeyAdded(IConventionForeignKeyBuilder foreignKeyBuilder, IConventionContext<IConventionForeignKeyBuilder> context)
    {
        // TODO: Add check does this foreign key contains DeleteBehavior attribute and only then set it
        foreignKeyBuilder.Metadata.SetDeleteBehavior(_deleteBehavior);
    }

    private DeleteBehavior _deleteBehavior;
}
