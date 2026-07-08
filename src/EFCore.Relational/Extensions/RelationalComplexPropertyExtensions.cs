// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Complex property extension methods for relational database metadata.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public static class RelationalComplexPropertyExtensions
{
    /// <summary>
    ///     Gets the value of JSON property name used for the given complex property of an entity mapped to a JSON column.
    /// </summary>
    /// <remarks>
    ///     Unless configured explicitly, complex property name is used.
    /// </remarks>
    /// <param name="complexProperty">The complex property.</param>
    /// <returns>
    ///     The value for the JSON property used to store the value of this complex property.
    ///     <see langword="null" /> is returned for complex properties of entities that are not mapped to a JSON column.
    /// </returns>
    public static string? GetJsonPropertyName(this IReadOnlyComplexProperty complexProperty)
        => complexProperty.ComplexType.GetJsonPropertyName();

    /// <summary>
    ///     Sets the value of JSON property name used for the given complex property of an entity mapped to a JSON column.
    /// </summary>
    /// <param name="complexProperty">The complex property.</param>
    /// <param name="name">The name to be used.</param>
    public static void SetJsonPropertyName(this IMutableComplexProperty complexProperty, string? name)
        => complexProperty.ComplexType.SetJsonPropertyName(name);

    /// <summary>
    ///     Sets the value of JSON property name used for the given complex property of an entity mapped to a JSON column.
    /// </summary>
    /// <param name="complexProperty">The complex property.</param>
    /// <param name="name">The name to be used.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetJsonPropertyName(
        this IConventionComplexProperty complexProperty,
        string? name,
        bool fromDataAnnotation = false)
        => complexProperty.ComplexType.SetJsonPropertyName(name, fromDataAnnotation);

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for the JSON property name for a given complex property.
    /// </summary>
    /// <param name="complexProperty">The complex property.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the JSON property name for a given complex property.</returns>
    public static ConfigurationSource? GetJsonPropertyNameConfigurationSource(this IConventionComplexProperty complexProperty)
        => complexProperty.ComplexType.GetJsonPropertyNameConfigurationSource();

    /// <summary>
    ///     <para>
    ///         Returns the table-like store objects to which this complex property is mapped if it's mapped to a JSON column.
    ///     </para>
    ///     <para>
    ///         This method is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    /// <param name="property">The complex property.</param>
    /// <param name="storeObjectType">The type of the store object.</param>
    /// <returns>The table-like store objects to which this property is mapped.</returns>
    public static IEnumerable<StoreObjectIdentifier> GetMappedStoreObjects(
        this IReadOnlyComplexProperty property,
        StoreObjectType storeObjectType)
    {
        var declaringType = property.DeclaringType;
        var declaringStoreObject = StoreObjectIdentifier.Create(declaringType, storeObjectType);

        // TODO: Support different JSON column names for different store objects. Issue #28584
        if (declaringStoreObject != null
            && property.ComplexType.GetContainerColumnName() != null)
        {
            yield return declaringStoreObject.Value;
        }

        if (storeObjectType is StoreObjectType.Function or StoreObjectType.SqlQuery)
        {
            yield break;
        }

        // TODO: Support entity splitting with JSON columns. Issue #36172
        // foreach (var fragment in declaringType.GetMappingFragments(storeObjectType))
        // {
        //     if (property.ComplexType.GetContainerColumnName(fragment.StoreObject) != null)
        //     {
        //         yield return fragment.StoreObject;
        //     }
        // }

        if (declaringType.GetMappingStrategy() == RelationalAnnotationNames.TphMappingStrategy)
        {
            yield break;
        }

        if (declaringType is IReadOnlyEntityType entityType)
        {
            foreach (var derivedType in entityType.GetDerivedTypes())
            {
                // TODO: Support different JSON column names in derived types. Issue #38214
                var derivedStoreObject = StoreObjectIdentifier.Create(derivedType, storeObjectType);
                if (derivedStoreObject != null
                    && property.ComplexType.GetContainerColumnName() != null)
                {
                    yield return derivedStoreObject.Value;
                }
            }
        }
    }
}
