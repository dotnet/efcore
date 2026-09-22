// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A relational-specific convention inheriting from <see cref="KeyDiscoveryConvention"/>.
/// </summary>
/// <remarks>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
///     </para>
/// </remarks>
public class RelationalKeyDiscoveryConvention : KeyDiscoveryConvention, IEntityTypeAnnotationChangedConvention
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public const string SynthesizedOrdinalPropertyName = "__synthesizedOrdinal";

    /// <summary>
    ///     Creates a new instance of <see cref="RelationalKeyDiscoveryConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public RelationalKeyDiscoveryConvention(
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
    protected override List<IConventionProperty>? DiscoverKeyProperties(IConventionEntityType entityType)
    {
        var ownership = entityType.FindOwnership();
        if (ownership?.DeclaringEntityType != entityType)
        {
            ownership = null;
        }

        // Don't discover key properties for owned collection types mapped to JSON so that we can persist properties
        // called `Id` without attempting to persist key values.
        if (ownership?.IsUnique == false
            && entityType.GetContainerColumnName() is not null)
        {
            return [];
        }

        return base.DiscoverKeyProperties(entityType);
    }

    /// <inheritdoc />
    protected override void ProcessKeyProperties(
        IList<IConventionProperty> keyProperties,
        IConventionEntityType entityType)
    {
        var isMappedToJson = entityType.GetContainerColumnName() is not null;
        var synthesizedProperty = keyProperties.FirstOrDefault(p => p.Name == SynthesizedOrdinalPropertyName);
        var ownershipForeignKey = entityType.FindOwnership();
        if (ownershipForeignKey?.IsUnique == false
            && isMappedToJson)
        {
            // This is an owned collection, so it has a composite key consisting of FK properties pointing to the owner PK,
            // any additional key properties defined by the application, and then the synthesized property.
            // Add these in the correct order--this is somewhat inefficient, but we are limited because we have to manipulate the
            // existing collection.
            var existingKeyProperties = keyProperties.ToList();
            keyProperties.Clear();

            // Add the FK properties to form the first part of the composite key.
            foreach (var conventionProperty in ownershipForeignKey.Properties)
            {
                keyProperties.Add(conventionProperty);
            }

            // Generate the synthesized key property if it doesn't exist.
            if (synthesizedProperty == null)
            {
                var builder = entityType.Builder.CreateUniqueProperty(typeof(int), SynthesizedOrdinalPropertyName, required: true);
                builder = builder?.ValueGenerated(ValueGenerated.OnAdd) ?? builder;
                synthesizedProperty = builder!.Metadata;
            }

            // Add non-duplicate, non-ownership, non-synthesized properties.
            foreach (var keyProperty in existingKeyProperties)
            {
                if (keyProperty != synthesizedProperty
                    && !keyProperties.Contains(keyProperty))
                {
                    keyProperties.Add(keyProperty);
                }
            }

            // Finally, the synthesized property always goes at the end.
            keyProperties.Add(synthesizedProperty);
        }
        else
        {
            // Not an owned collection or not mapped to JSON.
            if (synthesizedProperty is not null)
            {
                // This was an owned collection, but now is not, so remove the synthesized property.
                keyProperties.Remove(synthesizedProperty);
            }

            base.ProcessKeyProperties(keyProperties, entityType);
        }
    }

    /// <inheritdoc />
    public override void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        IConventionContext<IConventionPropertyBuilder> context)
    {
        if (propertyBuilder.Metadata.Name != SynthesizedOrdinalPropertyName)
        {
            base.ProcessPropertyAdded(propertyBuilder, context);
        }
    }

    /// <inheritdoc />
    public virtual void ProcessEntityTypeAnnotationChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        if (name == RelationalAnnotationNames.ContainerColumnName)
        {
            Configure(this, entityTypeBuilder);
        }

        static void Configure(RelationalKeyDiscoveryConvention me, IConventionEntityTypeBuilder builder)
        {
            me.TryConfigurePrimaryKey(builder);

            foreach (var ownershipFk in builder.Metadata.GetReferencingForeignKeys())
            {
                if (ownershipFk.IsOwnership)
                {
                    Configure(me, ownershipFk.DeclaringEntityType.Builder);
                }
            }
        }
    }
}
