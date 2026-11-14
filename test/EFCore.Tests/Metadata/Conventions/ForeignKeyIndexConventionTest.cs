// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable ClassWithVirtualMembersNeverInherited.Local
// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class ForeignKeyIndexConventionTest
{
    [Fact]
    public void Does_not_override_foreign_key_index_uniqueness_when_referenced_key_changes()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        var principalTypeBuilder = modelBuilder.Entity<PrincipalEntity>();
        var dependentTypeBuilder = modelBuilder.Entity<DependentEntity>();

        principalTypeBuilder.HasKey(t => t.InitialPrincipalId);
        dependentTypeBuilder.HasOne(t => t.Principal).WithOne().HasForeignKey<DependentEntity>(t => t.PrincipalId);
        dependentTypeBuilder.HasIndex(t => t.PrincipalId).IsUnique(false);
        principalTypeBuilder.HasKey(t => t.ChangedPrincipalId);

        var entityType = modelBuilder.Model.FindEntityType(typeof(DependentEntity))!;
        var property = entityType.FindProperty(nameof(DependentEntity.PrincipalId))!;
        var index = entityType.FindIndex(property)!;

        Assert.False(index.IsUnique);
    }

    [Fact]
    public void Partial_index_does_not_cover_foreign_key_index()
    {
        var modelBuilder = CreateModelBuilderWithPartialAwareConvention();
        modelBuilder.Entity<DependentEntity>().HasOne(t => t.Principal).WithMany().HasForeignKey(t => t.PrincipalId);
        var index = modelBuilder.Entity<DependentEntity>().HasIndex(t => new { t.PrincipalId, t.DependentId }).Metadata;

        var entityType = modelBuilder.Model.FindEntityType(typeof(DependentEntity))!;
        Assert.Equal([index], entityType.GetIndexes());

        ((IMutableIndex)index).SetAnnotation(PartialAnnotation, true);

        Assert.Equal(2, entityType.GetIndexes().Count());
        Assert.NotNull(entityType.FindIndex(entityType.FindProperty(nameof(DependentEntity.PrincipalId))!));

        ((IMutableIndex)index).RemoveAnnotation(PartialAnnotation);

        Assert.Equal([index], entityType.GetIndexes());
    }

    [Fact]
    public void Partial_index_declared_before_foreign_key_does_not_prevent_foreign_key_index()
    {
        var modelBuilder = CreateModelBuilderWithPartialAwareConvention();
        var index = modelBuilder.Entity<DependentEntity>().HasIndex(t => new { t.PrincipalId, t.DependentId }).Metadata;
        ((IMutableIndex)index).SetAnnotation(PartialAnnotation, true);

        modelBuilder.Entity<DependentEntity>().HasOne(t => t.Principal).WithMany().HasForeignKey(t => t.PrincipalId);

        var entityType = modelBuilder.Model.FindEntityType(typeof(DependentEntity))!;
        Assert.Equal(2, entityType.GetIndexes().Count());
    }

    private const string PartialAnnotation = "Test:Partial";

    private static ModelBuilder CreateModelBuilderWithPartialAwareConvention()
    {
        var services = InMemoryTestHelpers.Instance.CreateContextServices();
        var dependencies = services.GetRequiredService<ProviderConventionSetBuilderDependencies>();

        // The in-memory provider doesn't use this convention, so it is added here.
        return InMemoryTestHelpers.Instance.CreateConventionBuilder(
            services, c => c.ConventionSet.Add(new PartialAwareConvention(dependencies)));
    }

    private class PartialAwareConvention(ProviderConventionSetBuilderDependencies dependencies)
        : ForeignKeyIndexConvention(dependencies), IIndexAnnotationChangedConvention
    {
        protected override bool IsPartial(IConventionIndex index)
            => index.FindAnnotation(PartialAnnotation) != null;

        public void ProcessIndexAnnotationChanged(
            IConventionIndexBuilder indexBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation,
            IConventionContext<IConventionAnnotation> context)
        {
            if (name == PartialAnnotation)
            {
                ProcessIndexCoverageChanged(indexBuilder.Metadata);
            }
        }
    }

    private class PrincipalEntity
    {
        public int InitialPrincipalId { get; set; }

        public int ChangedPrincipalId { get; set; }
    }

    private class DependentEntity
    {
        public int PrincipalId { get; set; }

        public int DependentId { get; set; }

        public virtual PrincipalEntity Principal { get; set; } = null!;
    }
}
