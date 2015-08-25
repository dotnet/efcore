// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.SqlClient;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration;
using Microsoft.Data.Entity.Relational.Design.Templating;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering
{
    public class SqlServerDbContextCodeGeneratorHelper : DbContextCodeGeneratorHelper
    {
        public SqlServerDbContextCodeGeneratorHelper(
            [NotNull] DbContextGeneratorModel generatorModel, 
            [NotNull] IRelationalMetadataExtensionProvider extensionsProvider, 
            [NotNull] ModelUtilities modelUtilities)
            : base(generatorModel, extensionsProvider, modelUtilities)
        {
        }

        private const string _dbContextSuffix = "Context";

        public override string ClassName([NotNull] string connectionString)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));

            var builder = new SqlConnectionStringBuilder(connectionString);
            if (builder.InitialCatalog != null)
            {
                return CSharpUtilities.Instance.GenerateCSharpIdentifier(
                    builder.InitialCatalog + _dbContextSuffix, null);
            }

            return base.ClassName(connectionString);
        }

        public override string UseMethodName => "UseSqlServer";

        public override void AddValueGeneratedFacetConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            // If this property is the single integer primary key on the EntityType then
            // KeyConvention assumes ValueGeneratedOnAdd(). If the underlying column does
            // not have Identity set then we need to set to ValueGeneratedNever() to
            // override this behavior.
            if (propertyConfiguration.Property.SqlServer().IdentityStrategy == null
                && _keyConvention.ValueGeneratedOnAddProperty(
                    new List<Property> { (Property)propertyConfiguration.Property },
                    (EntityType)propertyConfiguration.EntityConfiguration.EntityType) != null)
            {
                propertyConfiguration.AddFacetConfiguration(
                    new FacetConfiguration("ValueGeneratedNever()"));
            }
            else
            {
                base.AddValueGeneratedFacetConfiguration(propertyConfiguration);
            }
        }
    }
}
