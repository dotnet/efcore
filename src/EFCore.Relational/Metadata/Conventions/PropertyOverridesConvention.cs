// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that ensures that the declaring property is current for the property overrides.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> and
///     <see href="https://aka.ms/efcore-docs-inheritance">Entity type hierarchy mapping</see> for more information and examples.
/// </remarks>
public class PropertyOverridesConvention : IPropertyAddedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="PropertyOverridesConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public PropertyOverridesConvention(
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

    /// <inheritdoc />
    public virtual void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        IConventionContext<IConventionPropertyBuilder> context)
    {
        var property = propertyBuilder.Metadata;
        if (!property.DeclaringType.HasSharedClrType)
        {
            return;
        }

        List<IConventionRelationalPropertyOverrides>? overridesToReattach = null;
        foreach (var overrides in property.GetOverrides())
        {
            if (overrides.Property == property)
            {
                continue;
            }

            overridesToReattach ??= [];

            overridesToReattach.Add(overrides);
        }

        if (overridesToReattach == null)
        {
            return;
        }

        foreach (var overrides in overridesToReattach)
        {
            var removedOverrides = property.RemoveOverrides(overrides.StoreObject);
            if (removedOverrides != null)
            {
                RelationalPropertyOverrides.Attach(property, removedOverrides);
            }
        }
    }
}
