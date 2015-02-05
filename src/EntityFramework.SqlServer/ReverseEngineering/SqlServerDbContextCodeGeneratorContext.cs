// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.ReverseEngineering
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

        public override void GenerateForeignKeysConfiguration(IndentedStringBuilder sb, IEntityType entityType)
        {
            //foreach (var otherEntityType in entityType.Model.EntityTypes.Where(et => et != entityType))
            //{
            //    foreach(var foreignKey in otherEntityType.ForeignKeys.Where(fk => fk.ReferencedEntityType == entityType))
            //    {
            //        // found a foreign key which references me
            //        sb.AppendLine();
            //        sb.Append("entity.HasMany<");
            //        sb.Append(otherEntityType.Name);
            //        sb.Append(">( e => e.");
            //        sb.Append(otherEntityType.Name);
            //        sb.Append(" );");
            //    }
            //}

            foreach (var foreignKey in entityType.ForeignKeys.Cast<ForeignKey>())
            {
                var navigationPropertyName = foreignKey
                    .GetAnnotation(SqlServerMetadataModelProvider.AnnotationNameDependentEndNavPropName).Value;
                sb.AppendLine();
                sb.Append("entity.ManyToOne<");
                sb.Append(foreignKey.ReferencedEntityType.Name);
                sb.Append(">( e => e.");
                sb.Append(navigationPropertyName);
                sb.Append(" );");

            }

            ////// construct dictionary mapping foreignKeyConstraintId to the list of Properties which constitute that foreign key
            ////var allForeignKeyConstraints = new Dictionary<string, List<Property>>(); // maps foreignKeyConstraintId to Properties 
            ////foreach (var prop in entityType.Properties.Cast<Property>())
            ////{
            ////    var foreignKeyConstraintsAnnotation = prop.TryGetAnnotation(SqlServerMetadataModelProvider.AnnotationNameForeignKeyConstraints);
            ////    if (foreignKeyConstraintsAnnotation != null)
            ////    {
            ////        var foreignKeyConstraintIds = SqlServerMetadataModelProvider.SplitString(
            ////            SqlServerMetadataModelProvider.AnnotationFormatForeignKeyConstraintSeparator.ToCharArray()
            ////            , foreignKeyConstraintsAnnotation.Value);
            ////        foreach (var fkcId in foreignKeyConstraintIds)
            ////        {
            ////            List<Property> properties;
            ////            if (!allForeignKeyConstraints.TryGetValue(fkcId, out properties))
            ////            {
            ////                properties = new List<Property>();
            ////                allForeignKeyConstraints.Add(fkcId, properties);
            ////            }
            ////            if (!properties.Contains(prop))
            ////            {
            ////                properties.Add(prop);
            ////            }
            ////        }
            ////    }
            ////}

            ////// loop over all constraints constructing foreign key entry in OnModelCreating()
            ////if (allForeignKeyConstraints.Count > 0)
            ////{
            ////    foreach (var fkcEntry in allForeignKeyConstraints)
            ////    {
            ////        var constraintId = fkcEntry.Key;
            ////        var propertyList = fkcEntry.Value;
            ////        if (propertyList.Count > 0)
            ////        {
            ////            var targetEntity = propertyList
            ////                .ElementAt(0)[SqlServerMetadataModelProvider.GetForeignKeyTargetEntityTypeAnnotationName(constraintId)];
            ////            var targetEntityLastIndex = targetEntity
            ////                .LastIndexOf(SqlServerMetadataModelProvider.AnnotationNameTableIdSchemaTableSeparator);
            ////            if (targetEntityLastIndex > 0)
            ////            {
            ////                targetEntity = targetEntity.Substring(targetEntityLastIndex + 1);
            ////            }

            ////            var ordinalAnnotationName = SqlServerMetadataModelProvider
            ////                .GetForeignKeyOrdinalPositionAnnotationName(constraintId);

            ////            sb.AppendLine();
            ////            sb.Append("entity.ForeignKey<");
            ////            sb.Append(targetEntity);
            ////            sb.Append(">( e => ");
            ////            sb.Append(ModelUtilities.Instance
            ////                .GenerateLambdaToKey(propertyList, p => int.Parse(p[ordinalAnnotationName]), "e"));
            ////            sb.Append(" );");
            ////        }
            ////    }
            ////    sb.AppendLine();
            ////}
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
                    sb.IncrementIndent();
                    foreach (var facetConfig in forSqlServerEntityFacetsConfiguration)
                    {
                        sb.AppendLine();
                        sb.Append(facetConfig);
                    }
                    sb.DecrementIndent();
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
            var schemaNameFacetConfig = GenerateSchemaNameFacetConfiguration(entityType);
            if (schemaNameFacetConfig != null)
            {
                facetsConfig.Add(schemaNameFacetConfig);
            }

            return facetsConfig;
        }

        public virtual string GenerateTableNameFacetConfiguration(IEntityType entityType)
        {
            if (entityType.SqlServer().Table != null && entityType.SqlServer().Table != entityType.Name)
            {
                return string.Format(CultureInfo.InvariantCulture, ".Table(\"{0}\")", entityType.SqlServer().Table);
            }

            return null;
        }

        public virtual string GenerateSchemaNameFacetConfiguration(IEntityType entityType)
        {
            if (entityType.SqlServer().Schema != null && entityType.SqlServer().Schema != "dbo")
            {
                return string.Format(CultureInfo.InvariantCulture, ".Schema(\"{0}\")", entityType.SqlServer().Schema);
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
                sb.Append(Generator.PropertyToPropertyNameMap[property]);
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

            return facetsConfig;
        }

        public virtual string GenerateMaxLengthFacetConfiguration(IProperty property)
        {
            //Annotation maxLengthAnnotation = ((Property)property)
            //    .TryGetAnnotation(SqlServerMetadataModelProvider.AnnotationNameMaxLength);
            //if (maxLengthAnnotation != null
            //    && maxLengthAnnotation.Value != null
            //    && int.Parse(maxLengthAnnotation.Value) > 0
            //    && IsValidDataTypeForMaxLength(property))
            //{
            //    return string.Format(CultureInfo.InvariantCulture, ".MaxLength({0})", maxLengthAnnotation.Value);
            //}

            if (((Property)property).MaxLength.HasValue)
            {
                return string.Format(CultureInfo.InvariantCulture, ".MaxLength({0})", ((Property)property).MaxLength.Value);
            }

            return null;
        }

        public virtual string GenerateColumnNameFacetConfiguration(IProperty property)
        {
            if (property.SqlServer().Column != null && property.SqlServer().Column != property.Name)
            {
                return string.Format(CultureInfo.InvariantCulture, ".Column(\"{0}\")", property.SqlServer().Column);
            }

            return null;
        }

        public virtual string GenerateColumnTypeFacetConfiguration(IProperty property)
        {
            if (property.SqlServer().ColumnType != null)
            {
                return string.Format(CultureInfo.InvariantCulture, ".ColumnType(\"{0}\")", property.SqlServer().ColumnType);
            }

            return null;
        }

        private static bool IsValidDataTypeForMaxLength(IProperty property)
        {
            return true;
        }

        //public override int PrimaryKeyPropertyOrder(IProperty property)
        //{
        //    return int.Parse(property[SqlServerMetadataModelProvider.AnnotationNamePrimaryKeyOrdinal]);
        //}
    }
}