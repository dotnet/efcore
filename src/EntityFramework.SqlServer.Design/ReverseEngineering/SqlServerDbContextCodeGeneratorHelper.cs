// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering
{
    public class SqlServerDbContextCodeGeneratorHelper : DbContextCodeGeneratorHelper
    {
        private const string _dbContextSuffix = "Context";

        public SqlServerDbContextCodeGeneratorHelper(
            [NotNull]DbContextGeneratorModel generatorModel)
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

        public override string ClassName([NotNull]string connectionString)
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

        public override void AddNavigationFacetsConfiguration(
            [NotNull]NavigationConfiguration navigationConfiguration)
        {
            Check.NotNull(navigationConfiguration, nameof(navigationConfiguration));

            foreach (var foreignKey in navigationConfiguration.EntityType.GetForeignKeys())
            {
                var dependentEndNavigationPropertyName =
                    (string)foreignKey[SqlServerMetadataModelProvider.AnnotationNameDependentEndNavPropName];
                var principalEndNavigationPropertyName =
                    (string)foreignKey[SqlServerMetadataModelProvider.AnnotationNamePrincipalEndNavPropName];

                var sb = new StringBuilder();
                sb.Append("Reference");
                sb.Append("(d => d.");
                sb.Append(dependentEndNavigationPropertyName);
                sb.Append(")");

                if (foreignKey.IsUnique)
                {
                    sb.Append(".InverseReference(");
                }
                else
                {
                    sb.Append(".InverseCollection(");
                }
                if (!string.IsNullOrEmpty(principalEndNavigationPropertyName))
                {
                    sb.Append("p => p.");
                    sb.Append(principalEndNavigationPropertyName);
                }
                sb.Append(")");

                sb.Append(".ForeignKey");
                if (foreignKey.IsUnique)
                {
                    // If the relationship is 1:1 need to define to which end
                    // the ForeignKey properties belong.
                    sb.Append("<");
                    sb.Append(navigationConfiguration.EntityType.Name);
                    sb.Append(">");
                }

                sb.Append("(d => ");
                sb.Append(GeneratorModel.Generator.ModelUtilities
                            .GenerateLambdaToKey(foreignKey.Properties, "d"));
                sb.Append(")");

                navigationConfiguration.AddFacetConfiguration(
                    new FacetConfiguration(sb.ToString()));
            }
        }

        public override void AddPropertyFacetsConfiguration([NotNull]PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            base.AddPropertyFacetsConfiguration(propertyConfiguration);
            AddUseIdentityFacetConfiguration(propertyConfiguration);
        }

        public virtual void AddUseIdentityFacetConfiguration(
            [NotNull]PropertyConfiguration propertyConfiguration)
        {
            Check.NotNull(propertyConfiguration, nameof(propertyConfiguration));

            if (propertyConfiguration.Property.SqlServer().ValueGenerationStrategy.HasValue
                && SqlServerValueGenerationStrategy.Identity
                   == propertyConfiguration.Property.SqlServer().ValueGenerationStrategy.Value)
            {
                propertyConfiguration.AddFacetConfiguration(
                    new FacetConfiguration(
                        "ForSqlServer()",
                        "UseIdentity()"));
            }
        }
    }
}