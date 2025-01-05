// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Specifies the configuration type for the entity type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class EntityTypeConfigurationAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EntityTypeConfigurationAttribute" /> class.
    /// </summary>
    /// <param name="entityConfigurationType">The IEntityTypeConfiguration&lt;&gt; type to use.</param>
    public EntityTypeConfigurationAttribute(Type entityConfigurationType)
    {
        Check.NotNull(entityConfigurationType, nameof(entityConfigurationType));

        EntityTypeConfigurationType = entityConfigurationType;
    }

    /// <summary>
    ///     Type of the entity type configuration.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.Interfaces)]
    public Type EntityTypeConfigurationType { get; }
}
