// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.Templating.Compilation;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class NullDatabaseMetadataModelProvider : IDatabaseMetadataModelProvider
    {
        public string DbContextTemplate
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string EntityTypeTemplate
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void AddReferencesForTemplates([NotNull] MetadataReferencesProvider metadataReferencesProvider)
        {
            throw new NotImplementedException();
        }

        public DbContextCodeGeneratorHelper DbContextCodeGeneratorHelper([NotNull] DbContextGeneratorModel model)
        {
            throw new NotImplementedException();
        }

        public EntityTypeCodeGeneratorHelper EntityTypeCodeGeneratorHelper([NotNull] EntityTypeGeneratorModel model)
        {
            throw new NotImplementedException();
        }

        public IModel GenerateMetadataModel([NotNull] string connectionString)
        {
            throw new NotImplementedException();
        }
    }
}
