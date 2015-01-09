// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.ReverseEngineering;

namespace EntityFramework.ReverseEngineering.FunctionalTests
{
    public class TestDatabaseMetadataModelProvider : IDatabaseMetadataModelProvider
    {
        public IModel GenerateMetadataModel(string connectionString, string filters)
        {
            var modelBuilder = new BasicModelBuilder(new Model());

            modelBuilder.Entity<RevEngEntity1>(e =>
                {
                    e.Key(entity => entity.Id);
                    e.Property(entity => entity.Name);
                    e.Property(entity => entity.Description);
                });

            modelBuilder.Entity<RevEngEntity2>(e =>
            {
                e.Key(entity => entity.Id);
                e.Property(entity => entity.Moniker);
                e.Property(entity => entity.FulsomePraise);
            });

            return modelBuilder.Model;
        }
    }

    public class RevEngEntity1
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

        public class RevEngEntity2
    {
        public int Id { get; set; }
        public string Moniker { get; set; }
        public string FulsomePraise { get; set; }
     }
}