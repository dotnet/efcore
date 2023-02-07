// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Serialization;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures JSON property name based on the applied <see cref="JsonPropertyNameAttribute" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class RelationalPropertyJsonPropertyNameAttributeConvention : PropertyAttributeConventionBase<JsonPropertyNameAttribute>
{
    /// <summary>
    ///     Creates a new instance of <see cref="RelationalPropertyJsonPropertyNameAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public RelationalPropertyJsonPropertyNameAttributeConvention(
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
    protected override void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        JsonPropertyNameAttribute attribute,
        MemberInfo clrMember,
        IConventionContext context)
    {
        if (!string.IsNullOrWhiteSpace(attribute.Name))
        {
            propertyBuilder.HasJsonPropertyName(attribute.Name, fromDataAnnotation: true);
        }
    }
}
