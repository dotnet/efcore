// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering
{
    public class SqlServerDbContextCodeGeneratorContext : DbContextCodeGenerator
    {
        public SqlServerDbContextCodeGeneratorContext(
            ReverseEngineeringGenerator generator,
            IModel model, string namespaceName,
            string className, string connectionString)
            : base(generator, model, namespaceName, className, connectionString)
        {
        }

        public override IEnumerable<IEntityType> OrderedEntityTypes()
        {
            // do not configure EntityTypes for which we had an error when generating
            return Model.EntityTypes.OrderBy(e => e.Name)
                .Where(e => ((EntityType)e).TryGetAnnotation(SqlServerMetadataModelProvider.AnnotationNameEntityTypeError) == null);
        }

        public override void GenerateNavigationsConfiguration(IndentedStringBuilder sb, IEntityType entityType)
        {
            foreach (var foreignKey in entityType.ForeignKeys.Cast<ForeignKey>())
            {
                var dependentEndNavigationPropertyName = foreignKey
                    .GetAnnotation(SqlServerMetadataModelProvider.AnnotationNameDependentEndNavPropName);
                var principalEndNavigationPropertyName = foreignKey
                    .TryGetAnnotation(SqlServerMetadataModelProvider.AnnotationNamePrincipalEndNavPropName);
                sb.AppendLine();
                sb.Append("entity.");
                sb.Append("HasOne");
                sb.Append("<");
                sb.Append(foreignKey.ReferencedEntityType.Name);
                sb.Append(">( d => d.");
                sb.Append(dependentEndNavigationPropertyName.Value);
                sb.Append(" )");

                if (principalEndNavigationPropertyName != null)
                {
                    if (((IForeignKey)foreignKey).IsUnique)
                    {
                        sb.Append(".WithOne( ");
                    }
                    else
                    {
                        sb.Append(".WithMany( ");
                    }
                    sb.Append("p => p.");
                    sb.Append(principalEndNavigationPropertyName.Value);
                    sb.Append(" )");
                }

                sb.Append(";");
            }
        }

        public override void GenerateEntityFacetsConfiguration(IndentedStringBuilder sb, IEntityType entityType)
        {
            var nonForSqlServerEntityFacetsConfiguration = GenerateNonForSqlServerEntityFacetsConfiguration(entityType);
            var forSqlServerEntityFacetsConfiguration = GenerateForSqlServerEntityFacetsConfiguration(entityType);

            if (nonForSqlServerEntityFacetsConfiguration.Count > 0
                || forSqlServerEntityFacetsConfiguration.Count > 0)
            {
                foreach (var facetConfig in nonForSqlServerEntityFacetsConfiguration)
                {
                    sb.AppendLine();
                    sb.Append(facetConfig);
                }

                if (forSqlServerEntityFacetsConfiguration.Count > 0)
                {
                    sb.AppendLine();
                    sb.Append("entity.ForSqlServer()");
                    using (sb.Indent())
                    {
                        foreach (var facetConfig in forSqlServerEntityFacetsConfiguration)
                        {
                            sb.AppendLine();
                            sb.Append(facetConfig);
                        }
                    }
                }
                sb.Append(";");
            }
        }

        public virtual List<string> GenerateNonForSqlServerEntityFacetsConfiguration(IEntityType entityType)
        {
            return new List<string>();
        }

        public virtual List<string> GenerateForSqlServerEntityFacetsConfiguration(IEntityType entityType)
        {
            var facetsConfig = new List<string>();
            var tableNameFacetConfig = GenerateTableNameFacetConfiguration(entityType);
            if (tableNameFacetConfig != null)
            {
                facetsConfig.Add(tableNameFacetConfig);
            }

            return facetsConfig;
        }

        public virtual string GenerateTableNameFacetConfiguration(IEntityType entityType)
        {
            if ("dbo" != entityType.SqlServer().Schema)
            {
                return string.Format(CultureInfo.InvariantCulture, ".Table({0}, {1})", 
                    CSharpUtilities.Instance.DelimitString(entityType.SqlServer().Table),
                    CSharpUtilities.Instance.DelimitString(entityType.SqlServer().Schema));
            }

            if (entityType.SqlServer().Table != null
                && entityType.SqlServer().Table != entityType.Name)
            {
                return string.Format(CultureInfo.InvariantCulture, ".Table({0})",
                    CSharpUtilities.Instance.DelimitString(entityType.SqlServer().Table));
            }

            return null;
        }

        public override void GeneratePropertyFacetsConfiguration(IndentedStringBuilder sb, IProperty property)
        {
            var nonForSqlServerPropertyFacetsConfiguration = GenerateNonForSqlServerPropertyFacetsConfiguration(property);
            var forSqlServerPropertyFacetsConfiguration = GenerateForSqlServerPropertyFacetsConfiguration(property);

            if (nonForSqlServerPropertyFacetsConfiguration.Count > 0
                || forSqlServerPropertyFacetsConfiguration.Count > 0)
            {
                sb.AppendLine();
                sb.Append("entity.Property( e => e.");
                sb.Append(property.Name);
                sb.Append(" )");
                sb.IncrementIndent();
                foreach(var facetConfig in nonForSqlServerPropertyFacetsConfiguration)
                {
                    sb.AppendLine();
                    sb.Append(facetConfig);
                }

                if (forSqlServerPropertyFacetsConfiguration.Count > 0)
                {
                    sb.AppendLine();
                    sb.Append(".ForSqlServer()");
                    sb.IncrementIndent();
                    foreach (var facetConfig in forSqlServerPropertyFacetsConfiguration)
                    {
                        sb.AppendLine();
                        sb.Append(facetConfig);
                    }
                    sb.DecrementIndent();
                }
                sb.Append(";");
                sb.DecrementIndent();
            }
        }

        public virtual List<string> GenerateNonForSqlServerPropertyFacetsConfiguration(IProperty property)
        {
            var facetsConfig = new List<string>();
            var maxLengthFacetConfig = GenerateMaxLengthFacetConfiguration(property);
            if (maxLengthFacetConfig != null)
            {
                facetsConfig.Add(maxLengthFacetConfig);
            }

            var storeComputedFacetConfig = GenerateStoreComputedFacetConfiguration(property);
            if (storeComputedFacetConfig != null)
            {
                facetsConfig.Add(storeComputedFacetConfig);
            }

            return facetsConfig;
        }

        public virtual List<string> GenerateForSqlServerPropertyFacetsConfiguration(IProperty property)
        {
            var facetsConfig = new List<string>();
            var columnNameFacetConfig = GenerateColumnNameFacetConfiguration(property);
            if (columnNameFacetConfig != null)
            {
                facetsConfig.Add(columnNameFacetConfig);
            }

            var columnTypeFacetConfig = GenerateColumnTypeFacetConfiguration(property);
            if (columnTypeFacetConfig != null)
            {
                facetsConfig.Add(columnTypeFacetConfig);
            }

            var useIdentityFacetConfig = GenerateUseIdentityFacetConfiguration(property);
            if (useIdentityFacetConfig != null)
            {
                facetsConfig.Add(useIdentityFacetConfig);
            }

            return facetsConfig;
        }

        public virtual string GenerateMaxLengthFacetConfiguration(IProperty property)
        {
            if (((Property)property).MaxLength.HasValue)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    ".MaxLength({0})",
                    CSharpUtilities.Instance.GenerateLiteral(
                        ((Property)property).MaxLength.Value));
            }

            return null;
        }

        public virtual string GenerateStoreComputedFacetConfiguration(IProperty property)
        {
            if (((Property)property).IsStoreComputed.HasValue)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    ".StoreComputed({0})",
                    CSharpUtilities.Instance.GenerateLiteral(
                        ((Property)property).IsStoreComputed.Value));
            }

            return null;
        }

        public virtual string GenerateColumnNameFacetConfiguration(IProperty property)
        {
            if (property.SqlServer().Column != null && property.SqlServer().Column != property.Name)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    ".Column({0})",
                    CSharpUtilities.Instance.DelimitString(property.SqlServer().Column));
            }

            return null;
        }

        public virtual string GenerateColumnTypeFacetConfiguration(IProperty property)
        {
            // output columnType if decimal to define precision and scale
            if (property.SqlServer().ColumnType != null
                && property.SqlServer().ColumnType.StartsWith("decimal"))
            {
                return string.Format(CultureInfo.InvariantCulture,
                    ".ColumnType({0})",
                    CSharpUtilities.Instance.DelimitString(property.SqlServer().ColumnType));
            }

            return null;
        }

        public virtual string GenerateUseIdentityFacetConfiguration(IProperty property)
        {
            // output columnType if decimal to define precision and scale
            if (property.SqlServer().ValueGenerationStrategy.HasValue
                && SqlServerValueGenerationStrategy.Identity == property.SqlServer().ValueGenerationStrategy)
            {
                return ".UseIdentity()";
            }

            return null;
        }
    }
}