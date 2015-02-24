// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.FunctionalTests.ReverseEngineering
{
    public class TestDatabaseMetadataModelProvider : IDatabaseMetadataModelProvider
    {
        public IModel Model { get; set; }

        public IModel GenerateMetadataModel(string connectionString)
        {
            Model = CreateTestModel();
            return Model;
        }

        public DbContextCodeGenerator GetContextModelCodeGenerator(
            ReverseEngineeringGenerator generator, DbContextGeneratorModel dbContextGeneratorModel)
        {
            return new TestDbContextCodeGenerator(
                generator,
                dbContextGeneratorModel.MetadataModel,
                dbContextGeneratorModel.Namespace,
                dbContextGeneratorModel.ClassName,
                dbContextGeneratorModel.ConnectionString);
        }

        public EntityTypeCodeGenerator GetEntityTypeModelCodeGenerator(
            ReverseEngineeringGenerator generator, EntityTypeGeneratorModel entityTypeGeneratorModel)
        {
            return new TestEntityTypeCodeGenerator(
                generator,
                entityTypeGeneratorModel.EntityType,
                entityTypeGeneratorModel.Namespace);
        }

        public IModel CreateTestModel()
        {
            var modelBuilder = new ModelBuilder();
            modelBuilder.Entity<EntityA>(entity => entity.Key(e => e.Id));
            modelBuilder.Entity<EntityB>(entity => entity.Key(e => e.Id));

            return modelBuilder.Model;
        }
    }

    public class TestDbContextCodeGenerator : DbContextCodeGenerator
    {
        public TestDbContextCodeGenerator(
            ReverseEngineeringGenerator generator,
            IModel model, string namespaceName,
            string className, string connectionString)
            : base(generator, model, namespaceName, className, connectionString)
        {
        }

        public override void GenerateNavigationsConfiguration(IEntityType entityType, IndentedStringBuilder sb)
        {
        }

        public override void GenerateProviderSpecificPropertyFacetsConfiguration(IProperty property, string entityVariableName, IndentedStringBuilder sb)
        {
        }
    }

    public class TestEntityTypeCodeGenerator : EntityTypeCodeGenerator
    {
        public TestEntityTypeCodeGenerator(
            ReverseEngineeringGenerator generator,
            IEntityType entityType,
            string namespaceName)
            : base(generator, entityType, namespaceName)
        {
        }

        public override void GenerateEntityNavigations(IndentedStringBuilder sb)
        {
        }

        public override void GenerateEntityProperty(IProperty property, IndentedStringBuilder sb)
        {
        }
    }

    public class EntityA
    {
        public int Id { get; set; }
        public string A1 { get; set; }
    }

    public class EntityB
    {
        public int Id { get; set; }
        public string B1 { get; set; }
    }
}