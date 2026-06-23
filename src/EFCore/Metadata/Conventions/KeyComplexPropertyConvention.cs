// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that marks every complex property in the path to a property used by a key as
///     required, since the value of such a property must be available in order to compute the key value.
/// </summary>
/// <remarks>
///     <para>
///         When the last referencing key is removed, the implicit non-nullable configuration applied
///         here is reverted as long as no other key keeps the chain in use and no explicit configuration
///         was applied by the user.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
///     </para>
/// </remarks>
/// <remarks>
///     Creates a new instance of <see cref="KeyComplexPropertyConvention" />.
/// </remarks>
/// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
public class KeyComplexPropertyConvention(ProviderConventionSetBuilderDependencies dependencies) :
    IKeyAddedConvention,
    IKeyRemovedConvention
{
    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; } = dependencies;

    /// <inheritdoc />
    public virtual void ProcessKeyAdded(
        IConventionKeyBuilder keyBuilder,
        IConventionContext<IConventionKeyBuilder> context)
        => MarkChainAsRequired(keyBuilder.Metadata.Properties);

    /// <inheritdoc />
    public virtual void ProcessKeyRemoved(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionKey key,
        IConventionContext<IConventionKey> context)
        => TryRevertChain(key.Properties);

    private static void MarkChainAsRequired(IReadOnlyList<IConventionProperty> properties)
    {
        foreach (var property in properties)
        {
            if (property.DeclaringType is not IConventionComplexType)
            {
                continue;
            }

            // Walk the complex-property chain from the entity down to the property's declaring complex type
            // and configure each link as required by convention.
            // TODO: Use layering #15898 - the implicit "required because referenced by a key" state
            // should be tracked separately so it can be cleanly reverted without round-tripping through
            // the convention configuration source.
            foreach (var complexProperty in EnumerateChain(property))
            {
                complexProperty.Builder.IsRequired(true);
            }
        }
    }

    private static void TryRevertChain(IReadOnlyList<IConventionProperty> properties)
    {
        foreach (var property in properties)
        {
            if (property.DeclaringType is not IConventionComplexType)
            {
                continue;
            }

            foreach (var complexProperty in EnumerateChain(property))
            {
                if (complexProperty.GetIsNullableConfigurationSource() != ConfigurationSource.Convention)
                {
                    continue;
                }

                if (IsStillReferenced(complexProperty))
                {
                    continue;
                }

                // TODO: Use layering #15898 - we currently can't distinguish a Convention-set IsRequired
                // that was applied because of a key from one applied for any other reason, so we
                // only revert when no key in the entity references any property declared on the
                // complex type (or any of its nested complex types).
                complexProperty.Builder.IsRequired(null);
            }
        }
    }

    private static IEnumerable<IConventionComplexProperty> EnumerateChain(IConventionPropertyBase property)
    {
        var typeBase = property.DeclaringType;
        while (typeBase is IConventionComplexType complexType)
        {
            yield return (IConventionComplexProperty)complexType.ComplexProperty;
            typeBase = complexType.ComplexProperty.DeclaringType;
        }
    }

    private static bool IsStillReferenced(IConventionComplexProperty complexProperty)
    {
        var entityType = complexProperty.DeclaringType.ContainingEntityType;
        var coveredTypes = new HashSet<IReadOnlyTypeBase>();
        CollectCoveredTypes(complexProperty.ComplexType, coveredTypes);

        foreach (var key in entityType.GetKeys())
        {
            if (key.Properties.Any(p => coveredTypes.Contains(p.DeclaringType)))
            {
                return true;
            }
        }

        return false;
    }

    private static void CollectCoveredTypes(IReadOnlyComplexType complexType, HashSet<IReadOnlyTypeBase> coveredTypes)
    {
        coveredTypes.Add(complexType);
        foreach (var nested in complexType.GetComplexProperties())
        {
            CollectCoveredTypes(nested.ComplexType, coveredTypes);
        }
    }
}
