// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.ReverseEngineering
{
    public class SqlServerDbContextCodeGeneratorContext : DbContextCodeGeneratorContext
    {
        public SqlServerDbContextCodeGeneratorContext(
            IModel model, string namespaceName,
            string className, string connectionString)
            : base(model, namespaceName, className, connectionString)
        {
        }

        public override void GenerateForeignKeysConfiguration(IndentedStringBuilder sb, IEntityType entityType)
        {
            // construct dictionary mapping foreignKeyConstraintId to the list of Properties which constitute that foreign key
            var allForeignKeyConstraints = new Dictionary<string, List<Property>>(); // maps foreignKeyConstraintId to Properties 
            foreach (var prop in entityType.Properties.Cast<Property>())
            {
                var foreignKeyConstraintsAnnotation = prop.TryGetAnnotation(SqlServerMetadataModelProvider.AnnotationNameForeignKeyConstraints);
                if (foreignKeyConstraintsAnnotation != null)
                {
                    var foreignKeyConstraintIds = SqlServerMetadataModelProvider.SplitString(
                        SqlServerMetadataModelProvider.AnnotationFormatForeignKeyConstraintSeparator.ToCharArray()
                        , foreignKeyConstraintsAnnotation.Value);
                    foreach (var fkcId in foreignKeyConstraintIds)
                    {
                        List<Property> properties;
                        if (!allForeignKeyConstraints.TryGetValue(fkcId, out properties))
                        {
                            properties = new List<Property>();
                            allForeignKeyConstraints.Add(fkcId, properties);
                        }
                        if (!properties.Contains(prop))
                        {
                            properties.Add(prop);
                        }
                    }
                }
            }

            // loop over all constraints constructing foreign key entry in OnModelCreating()
            foreach (var fkcEntry in allForeignKeyConstraints)
            {
                var constraintId = fkcEntry.Key;
                var propertyList = fkcEntry.Value;
                if (propertyList.Count > 0)
                {
                    var targetEntity = propertyList.ElementAt(0)[SqlServerMetadataModelProvider.GetForeignKeyTargetEntityTypeAnnotationName(constraintId)];
                    var targetEntityLastIndex = targetEntity.LastIndexOf(SqlServerMetadataModelProvider.AnnotationNameTableIdSchemaTableSeparator);
                    if (targetEntityLastIndex > 0)
                    {
                        targetEntity = targetEntity.Substring(targetEntityLastIndex + 1);
                    }

                    var ordinalAnnotationName = SqlServerMetadataModelProvider.GetForeignKeyOrdinalPositionAnnotationName(constraintId);

                    sb.Append("entity.ForeignKey<");
                    sb.Append(targetEntity);
                    sb.Append(">( e => ");
                    sb.Append(ModelUtilities.Instance
                        .GenerateLambdaToKey(propertyList, p => int.Parse(p[ordinalAnnotationName]), "e"));
                    sb.AppendLine(" );");
                }
            }
        }

        public override int PrimaryKeyPropertyOrder(IProperty property)
        {
            return int.Parse(property[SqlServerMetadataModelProvider.AnnotationNamePrimaryKeyOrdinal]);
        }
    }
}