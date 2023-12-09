// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

/// <summary>
///     <para>
///         A service on the EF internal service provider that creates the <see cref="ConventionSet" />
///         for the current database provider. This is combined with <see cref="IConventionSetPlugin" />
///         instances to produce the full convention set exposed by the <see cref="IConventionSetBuilder" />
///         service.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         Database providers should implement this service by inheriting from either
///         this class (for non-relational providers) or `RelationalConventionSetBuilder` (for relational providers).
///     </para>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
///     </para>
/// </remarks>
public class ProviderConventionSetBuilder : IProviderConventionSetBuilder
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ProviderConventionSetBuilder" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    public ProviderConventionSetBuilder(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Builds and returns the convention set for the current database provider.
    /// </summary>
    /// <returns>The convention set for the current database provider.</returns>
    public virtual ConventionSet CreateConventionSet()
    {
        var conventionSet = new ConventionSet();

        conventionSet.Add(new ModelCleanupConvention(Dependencies));

        conventionSet.Add(new NotMappedTypeAttributeConvention(Dependencies));
        conventionSet.Add(new OwnedAttributeConvention(Dependencies));
        conventionSet.Add(new ComplexTypeAttributeConvention(Dependencies));
        conventionSet.Add(new KeylessAttributeConvention(Dependencies));
        conventionSet.Add(new EntityTypeConfigurationAttributeConvention(Dependencies));
        conventionSet.Add(new NotMappedMemberAttributeConvention(Dependencies));
        conventionSet.Add(new BackingFieldAttributeConvention(Dependencies));
        conventionSet.Add(new ConcurrencyCheckAttributeConvention(Dependencies));
        conventionSet.Add(new DatabaseGeneratedAttributeConvention(Dependencies));
        conventionSet.Add(new RequiredPropertyAttributeConvention(Dependencies));
        conventionSet.Add(new MaxLengthAttributeConvention(Dependencies));
        conventionSet.Add(new StringLengthAttributeConvention(Dependencies));
        conventionSet.Add(new TimestampAttributeConvention(Dependencies));
        conventionSet.Add(new ForeignKeyAttributeConvention(Dependencies));
        conventionSet.Add(new UnicodeAttributeConvention(Dependencies));
        conventionSet.Add(new PrecisionAttributeConvention(Dependencies));
        conventionSet.Add(new InversePropertyAttributeConvention(Dependencies));
        conventionSet.Add(new DeleteBehaviorAttributeConvention(Dependencies));
        conventionSet.Add(new NavigationBackingFieldAttributeConvention(Dependencies));
        conventionSet.Add(new RequiredNavigationAttributeConvention(Dependencies));

        conventionSet.Add(new NavigationEagerLoadingConvention(Dependencies));
        conventionSet.Add(new DbSetFindingConvention(Dependencies));
        conventionSet.Add(new BaseTypeDiscoveryConvention(Dependencies));
        conventionSet.Add(new ManyToManyJoinEntityTypeConvention(Dependencies));
        conventionSet.Add(new PropertyDiscoveryConvention(Dependencies));
        conventionSet.Add(new KeyDiscoveryConvention(Dependencies));
        conventionSet.Add(new ServicePropertyDiscoveryConvention(Dependencies));
        conventionSet.Add(new RelationshipDiscoveryConvention(Dependencies));
        conventionSet.Add(new ComplexPropertyDiscoveryConvention(Dependencies));
        conventionSet.Add(new ValueGenerationConvention(Dependencies));
        conventionSet.Add(new DiscriminatorConvention(Dependencies));
        conventionSet.Add(new CascadeDeleteConvention(Dependencies));
        conventionSet.Add(new ChangeTrackingStrategyConvention(Dependencies));
        conventionSet.Add(new ConstructorBindingConvention(Dependencies));
        conventionSet.Add(new KeyAttributeConvention(Dependencies));
        conventionSet.Add(new IndexAttributeConvention(Dependencies));
        conventionSet.Add(new ForeignKeyIndexConvention(Dependencies));
        conventionSet.Add(new ForeignKeyPropertyDiscoveryConvention(Dependencies));
        conventionSet.Add(new NonNullableReferencePropertyConvention(Dependencies));
        conventionSet.Add(new NonNullableNavigationConvention(Dependencies));
        conventionSet.Add(new BackingFieldConvention(Dependencies));
        conventionSet.Add(new QueryFilterRewritingConvention(Dependencies));
        conventionSet.Add(new RuntimeModelConvention(Dependencies));
        conventionSet.Add(new ElementMappingConvention(Dependencies));
        conventionSet.Add(new ElementTypeChangedConvention(Dependencies));

        return conventionSet;
    }

    /// <summary>
    ///     Replaces an existing convention with a derived convention.
    /// </summary>
    /// <typeparam name="TConvention">The type of convention being replaced.</typeparam>
    /// <typeparam name="TImplementation">The type of the old convention.</typeparam>
    /// <param name="conventionsList">The list of existing convention instances to scan.</param>
    /// <param name="newConvention">The new convention.</param>
    protected virtual bool ReplaceConvention<TConvention, TImplementation>(
        List<TConvention> conventionsList,
        TImplementation newConvention)
        where TImplementation : TConvention
        => ConventionSet.Replace(conventionsList, newConvention);
}
