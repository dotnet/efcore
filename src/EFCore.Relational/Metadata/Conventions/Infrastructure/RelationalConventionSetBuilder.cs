// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

/// <summary>
///     <para>
///         A service on the EF internal service provider that creates the <see cref="ConventionSet" />
///         for the current relational database provider. This is combined with <see cref="IConventionSetPlugin" />
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
///         this class (for relational providers) or <see cref="ProviderConventionSetBuilder" /> (for non-relational providers).
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
public abstract class RelationalConventionSetBuilder : ProviderConventionSetBuilder
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationalConventionSetBuilder" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this service.</param>
    protected RelationalConventionSetBuilder(
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

    /// <summary>
    ///     Builds and returns the convention set for the current database provider.
    /// </summary>
    /// <returns>The convention set for the current database provider.</returns>
    public override ConventionSet CreateConventionSet()
    {
        var conventionSet = base.CreateConventionSet();

        conventionSet.Add(new RelationalColumnAttributeConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new RelationalColumnCommentAttributeConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new RelationalTableAttributeConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new RelationalTableCommentAttributeConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new RelationalDbFunctionAttributeConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new RelationalPropertyJsonPropertyNameAttributeConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new RelationalNavigationJsonPropertyNameAttributeConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new TableSharingConcurrencyTokenConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new TableNameFromDbSetConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new PropertyOverridesConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new CheckConstraintConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new StoredProcedureConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new TableValuedDbFunctionConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new StoreGenerationConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new EntitySplittingConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new DiscriminatorLengthConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new EntityTypeHierarchyMappingConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new SequenceUniquificationConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new SharedTableConvention(Dependencies, RelationalDependencies));
        conventionSet.Add(new RelationalMapToJsonConvention(Dependencies, RelationalDependencies));

        conventionSet.Replace<ValueGenerationConvention>(
            new RelationalValueGenerationConvention(Dependencies, RelationalDependencies));
        conventionSet.Replace<QueryFilterRewritingConvention>(
            new RelationalQueryFilterRewritingConvention(Dependencies, RelationalDependencies));
        conventionSet.Replace<RuntimeModelConvention>(new RelationalRuntimeModelConvention(Dependencies, RelationalDependencies));

        return conventionSet;
    }
}
