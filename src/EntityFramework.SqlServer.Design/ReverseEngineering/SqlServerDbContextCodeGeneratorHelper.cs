// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.SqlClient;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering
{
    public class SqlServerDbContextCodeGeneratorHelper : DbContextCodeGeneratorHelper
    {
        private const string _dbContextSuffix = "Context";
        private KeyConvention _keyConvention = new KeyConvention();

        public SqlServerDbContextCodeGeneratorHelper(
            [NotNull] DbContextGeneratorModel generatorModel,
            [NotNull] IRelationalMetadataExtensionProvider extensionsProvider)
            : base(generatorModel, extensionsProvider)
        {
        }

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

        public override IEnumerable<OptionsBuilderConfiguration> OnConfiguringConfigurations
            => new List<OptionsBuilderConfiguration>()
                {
                    new OptionsBuilderConfiguration(
                        "UseSqlServer("
                        + VerbatimStringLiteral(GeneratorModel.ConnectionString)
                        + ")")
                };

        public override void AddPropertyFacetsConfiguration([NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            base.AddPropertyFacetsConfiguration(propertyConfiguration);

            AddValueGeneratedNeverFacetConfiguration(propertyConfiguration);
        }

        public virtual void AddValueGeneratedNeverFacetConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            // If the EntityType has a single integer key KeyConvention assumes ValueGeneratedOnAdd().
            // If the underlying column does not have Identity set then we need to set to
            // ValueGeneratedNever() to override this behavior.
            if (_keyConvention.ValueGeneratedOnAddProperty(
                new List<Property> { (Property)propertyConfiguration.Property },
                (EntityType)propertyConfiguration.EntityConfiguration.EntityType) != null)
            {
                propertyConfiguration.AddFacetConfiguration(
                    new FacetConfiguration("ValueGeneratedNever()"));
            }
        }
    }
}
