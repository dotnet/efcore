// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering
{
    public class SqlServerDbContextCodeGeneratorHelper : DbContextCodeGeneratorHelper
    {
        private const string _dbContextSuffix = "Context";

        public SqlServerDbContextCodeGeneratorHelper(
            [NotNull] DbContextGeneratorModel generatorModel)
            : base(generatorModel)
        {
        }

        public override IEnumerable<IEntityType> OrderedEntityTypes()
        {
            // do not configure EntityTypes for which we had an error when generating
            return GeneratorModel.MetadataModel.EntityTypes.OrderBy(e => e.Name)
                .Where(e => ((EntityType)e).FindAnnotation(
                    SqlServerMetadataModelProvider.AnnotationNameEntityTypeError) == null);
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

        public override void AddNavigationsConfiguration(
            [NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            foreach (var foreignKey in entityConfiguration.EntityType.GetForeignKeys())
            {
                var dependentEndNavigationPropertyName =
                    (string)foreignKey[SqlServerMetadataModelProvider.AnnotationNameDependentEndNavPropName];
                var principalEndNavigationPropertyName =
                    (string)foreignKey[SqlServerMetadataModelProvider.AnnotationNamePrincipalEndNavPropName];

                entityConfiguration.NavigationConfigurations.Add(
                    new NavigationConfiguration(entityConfiguration, foreignKey,
                        dependentEndNavigationPropertyName, principalEndNavigationPropertyName));
            }
        }

        public override void AddPropertyFacetsConfiguration([NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            base.AddPropertyFacetsConfiguration(propertyConfiguration);
            AddUseIdentityFacetConfiguration(propertyConfiguration);
        }

        public virtual void AddUseIdentityFacetConfiguration(
            [NotNull] PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (propertyConfiguration.Property.SqlServer().IdentityStrategy.HasValue
                && SqlServerIdentityStrategy.IdentityColumn
                == propertyConfiguration.Property.SqlServer().IdentityStrategy.Value)
            {
                propertyConfiguration.AddFacetConfiguration(
                    new FacetConfiguration(
                        "ForSqlServer",
                        "UseIdentity()"));
            }
        }
    }
}
