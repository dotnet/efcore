// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Serialization;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures a JSON element name for the navigation property mapped to json
///     based on the <see cref="JsonPropertyNameAttribute" /> attribute.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class RelationalNavigationJsonPropertyNameAttributeConvention :
    NavigationAttributeConventionBase<JsonPropertyNameAttribute>,
    INavigationAddedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="RelationalNavigationJsonPropertyNameAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public RelationalNavigationJsonPropertyNameAttributeConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
        : base(dependencies)
    {
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    public override void ProcessNavigationAdded(
        IConventionNavigationBuilder navigationBuilder,
        JsonPropertyNameAttribute attribute,
        IConventionContext<IConventionNavigationBuilder> context)
    {
        if (!string.IsNullOrWhiteSpace(attribute.Name))
        {
            navigationBuilder.Metadata.TargetEntityType.Builder.HasJsonPropertyName(attribute.Name, fromDataAnnotation: true);
        }
    }
}
