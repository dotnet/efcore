// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
///     <para>
///         Identifies where to find the design time services for a given database provider. This attribute should
///         be present in the primary assembly of the database provider.
///     </para>
///     <para>
///         This attribute is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class DesignTimeProviderServicesAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DesignTimeProviderServicesAttribute" /> class.
    /// </summary>
    /// <param name="typeName">
    ///     The name of the type that can be used to add the database providers design time services to a <see cref="ServiceCollection" />.
    ///     This type should implement <see cref="IDesignTimeServices" />.
    /// </param>
    public DesignTimeProviderServicesAttribute(string typeName)
    {
        Check.NotEmpty(typeName, nameof(typeName));

        TypeName = typeName;
    }

    /// <summary>
    ///     Gets the name of the type that can be used to add the database providers design time services to a <see cref="ServiceCollection" />.
    ///     This type should implement <see cref="IDesignTimeServices" />.
    /// </summary>
    public string TypeName { get; }
}
