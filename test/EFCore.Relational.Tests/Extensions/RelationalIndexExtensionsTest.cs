// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Index = Microsoft.EntityFrameworkCore.Metadata.Internal.Index;

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

public class RelationalIndexExtensionsTest
{
    [ConditionalFact]
    public void IndexAttribute_database_name_can_be_overriden_using_fluent_api()
    {
        var modelBuilder = CreateConventionModelBuilder();
        var entityBuilder = modelBuilder.Entity<EntityWithIndexes>();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var index in entityType.GetDeclaredIndexes())
            {
                index.SetDatabaseName("My" + index.Name);
            }
        }

        modelBuilder.Model.FinalizeModel();

        var index0 = (Index)entityBuilder.Metadata.GetIndexes().First();
        Assert.Equal(ConfigurationSource.DataAnnotation, index0.GetConfigurationSource());
        Assert.Equal("IndexOnAAndB", index0.Name);
        Assert.Equal("MyIndexOnAAndB", index0.GetDatabaseName());
        Assert.Equal(ConfigurationSource.Explicit, index0.GetDatabaseNameConfigurationSource());
        Assert.True(index0.IsUnique);
        Assert.Equal(ConfigurationSource.DataAnnotation, index0.GetIsUniqueConfigurationSource());
        Assert.Collection(
            index0.Properties,
            prop0 => Assert.Equal("A", prop0.Name),
            prop1 => Assert.Equal("B", prop1.Name));

        var index1 = (Index)entityBuilder.Metadata.GetIndexes().Skip(1).First();
        Assert.Equal(ConfigurationSource.DataAnnotation, index1.GetConfigurationSource());
        Assert.Equal("IndexOnBAndC", index1.Name);
        Assert.Equal("MyIndexOnBAndC", index1.GetDatabaseName());
        Assert.Equal(ConfigurationSource.Explicit, index1.GetDatabaseNameConfigurationSource());
        Assert.False(index1.IsUnique);
        Assert.Null(index1.GetIsUniqueConfigurationSource());
        Assert.Collection(
            index1.Properties,
            prop0 => Assert.Equal("B", prop0.Name),
            prop1 => Assert.Equal("C", prop1.Name));
    }

    protected virtual ModelBuilder CreateConventionModelBuilder()
        => FakeRelationalTestHelpers.Instance.CreateConventionBuilder();

    [Index(nameof(A), nameof(B), Name = "IndexOnAAndB", IsUnique = true)]
    [Index(nameof(B), nameof(C), Name = "IndexOnBAndC")]
    private class EntityWithIndexes
    {
        public int Id { get; set; }
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
    }
}
