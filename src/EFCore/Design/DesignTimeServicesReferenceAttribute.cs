// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design;

/// <summary>
///     <para>
///         Identifies where to find additional design time services.
///     </para>
///     <para>
///         This attribute is typically used by design-time extensions. It is generally not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class DesignTimeServicesReferenceAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DesignTimeServicesReferenceAttribute" /> class.
    /// </summary>
    /// <param name="typeName">
    ///     The assembly-qualified name of the type that can be used to add additional design time services to a <see cref="ServiceCollection" />.
    ///     This type should implement <see cref="IDesignTimeServices" />.
    /// </param>
    public DesignTimeServicesReferenceAttribute(string typeName)
        : this(typeName, forProvider: null)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DesignTimeServicesReferenceAttribute" /> class.
    /// </summary>
    /// <param name="typeName">
    ///     The assembly-qualified name of the type that can be used to add additional design time services to a <see cref="ServiceCollection" />.
    ///     This type should implement <see cref="IDesignTimeServices" />.
    /// </param>
    /// <param name="forProvider">
    ///     The name of the provider for which these services should be added. If null, the services will be added
    ///     for all providers.
    /// </param>
    public DesignTimeServicesReferenceAttribute(string typeName, string? forProvider)
    {
        Check.NotEmpty(typeName, nameof(typeName));

        TypeName = typeName;
        ForProvider = forProvider;
    }

    /// <summary>
    ///     Gets the assembly-qualified name of the type that can be used to add additional design time services to a
    ///     <see cref="ServiceCollection" />.
    ///     This type should implement <see cref="IDesignTimeServices" />.
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    ///     Gets the name of the provider for which these services should be added. If null, the services will be
    ///     added for all providers.
    /// </summary>
    public string? ForProvider { get; }
}
