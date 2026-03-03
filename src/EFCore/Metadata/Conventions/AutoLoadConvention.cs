// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures properties based on provider-specific heuristics to determine
///     whether they should be auto-loaded.
///     Override <see cref="ShouldBeAutoLoaded" /> to customize which properties are excluded from automatic loading.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class AutoLoadConvention : IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="AutoLoadConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public AutoLoadConvention(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            ProcessType(entityType);
        }
    }

    private void ProcessType(IConventionTypeBase typeBase)
    {
        foreach (var property in typeBase.GetDeclaredProperties())
        {
            if (property.GetIsAutoLoadedConfigurationSource() == null
                && !ShouldBeAutoLoaded(property))
            {
                property.Builder.IsAutoLoaded(false, fromDataAnnotation: false);
            }
        }

        foreach (var complexProperty in typeBase.GetDeclaredComplexProperties())
        {
            // Properties on complex collections are always auto-loaded
            if (!complexProperty.IsCollection)
            {
                ProcessType(complexProperty.ComplexType);
            }
        }
    }

    /// <summary>
    ///     Returns a value indicating whether the given property should be auto-loaded.
    /// </summary>
    /// <param name="property">The property to check.</param>
    /// <returns><see langword="true" /> if the property should be auto-loaded; <see langword="false" /> otherwise.</returns>
    protected virtual bool ShouldBeAutoLoaded(IConventionProperty property)
        => true;
}
