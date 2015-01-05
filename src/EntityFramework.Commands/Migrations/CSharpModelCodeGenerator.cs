// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Commands.Utilities;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Commands.Migrations
{
    public class CSharpModelCodeGenerator : ModelCodeGenerator
    {
        private const string TableNameAnnotationName = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.TableName;
        private const string SchemaAnnotationName = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.Schema;
        private const string ColumnNameAnnotationName = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.ColumnName;
        private const string KeyNameAnnotationName = RelationalAnnotationNames.Prefix + RelationalAnnotationNames.Name;

        public override void GenerateModelSnapshotClass(
            string @namespace,
            string className,
            IModel model,
            Type contextType,
            IndentedStringBuilder stringBuilder)
        {
            Check.NotEmpty(className, "className");
            Check.NotEmpty(@namespace, "namespace");
            Check.NotNull(model, "model");
            Check.NotNull(stringBuilder, "stringBuilder");

            // TODO: Consider namespace ordering, for example putting System namespaces first
            foreach (var ns in GetNamespaces(model, contextType).OrderBy(n => n).Distinct())
            {
                stringBuilder
                    .Append("using ")
                    .Append(ns)
                    .AppendLine(";");
            }

            stringBuilder
                .AppendLine()
                .Append("namespace ")
                .AppendLine(@namespace)
                .AppendLine("{");

            using (stringBuilder.Indent())
            {
                stringBuilder
                    .Append("[ContextType(typeof(")
                    .Append(contextType.GetNestedName())
                    .AppendLine("))]")
                    .Append("public class ")
                    .Append(className)
                    .AppendLine(" : ModelSnapshot")
                    .AppendLine("{");

                using (stringBuilder.Indent())
                {
                    stringBuilder
                        .AppendLine("public override IModel Model")
                        .AppendLine("{");

                    using (stringBuilder.Indent())
                    {
                        stringBuilder
                            .AppendLine("get")
                            .AppendLine("{");

                        using (stringBuilder.Indent())
                        {
                            Generate(model, stringBuilder);
                        }

                        stringBuilder
                            .AppendLine()
                            .AppendLine("}");
                    }

                    stringBuilder.AppendLine("}");
                }

                stringBuilder.AppendLine("}");
            }

            stringBuilder.Append("}");
        }

        public override void Generate(IModel model, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(model, "model");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder.Append("var builder = new BasicModelBuilder()");

            using (stringBuilder.Indent())
            {
                GenerateAnnotations(model.Annotations.ToArray(), stringBuilder);
            }

            stringBuilder.AppendLine(";");

            GenerateEntityTypes(model.EntityTypes, stringBuilder);

            stringBuilder
                .AppendLine()
                .Append("return builder.Model;");
        }

        [Flags]
        protected enum GenerateEntityTypeOptions
        {
            Primary = 1,
            Secondary = 2,
            Full = Primary | Secondary
        }

        protected virtual void GenerateEntityTypes(
            IReadOnlyList<IEntityType> entityTypes, IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(entityTypes, "entityTypes");
            Check.NotNull(stringBuilder, "stringBuilder");

            foreach (var entityType in entityTypes)
            {
                stringBuilder.AppendLine();

                GenerateEntityType(entityType, stringBuilder, GenerateEntityTypeOptions.Primary);
            }

            foreach (var entityType in entityTypes.Where(e => e.ForeignKeys.Count > 0))
            {
                stringBuilder.AppendLine();

                GenerateEntityType(entityType, stringBuilder, GenerateEntityTypeOptions.Secondary);
            }
        }

        protected virtual void GenerateEntityType(
            [NotNull] IEntityType entityType, [NotNull] IndentedStringBuilder stringBuilder, GenerateEntityTypeOptions options)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("builder.Entity(")
                .Append(DelimitString(entityType.Name))
                .AppendLine(", b =>");

            using (stringBuilder.Indent())
            {
                stringBuilder.Append("{");

                using (stringBuilder.Indent())
                {
                    if ((options & GenerateEntityTypeOptions.Primary) != 0)
                    {
                        GenerateProperties(entityType.Properties, stringBuilder);

                        GenerateKey(entityType.GetPrimaryKey(), stringBuilder);
                    }

                    if ((options & GenerateEntityTypeOptions.Secondary) != 0)
                    {
                        GenerateForeignKeys(entityType.ForeignKeys, stringBuilder);
                    }

                    if ((options & GenerateEntityTypeOptions.Primary) != 0)
                    {
                        GenerateEntityTypeAnnotations(entityType, stringBuilder);
                    }
                }

                stringBuilder
                    .AppendLine()
                    .AppendLine("});");
            }
        }

        protected virtual void GenerateProperties(
            [NotNull] IReadOnlyList<IProperty> properties, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(properties, "properties");
            Check.NotNull(stringBuilder, "stringBuilder");

            foreach (var property in properties)
            {
                GenerateProperty(property, stringBuilder);
            }
        }

        protected virtual void GenerateProperty(
            [NotNull] IProperty property, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(property, "property");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .AppendLine()
                .Append("b.Property<")
                .Append(property.PropertyType.GetTypeName())
                .Append(">(")
                .Append(DelimitString(property.Name))
                .Append(")");

            using (stringBuilder.Indent())
            {
                if (property.IsConcurrencyToken)
                {
                    stringBuilder
                        .AppendLine()
                        .Append(".ConcurrencyToken()");
                }

                if (property.GenerateValueOnAdd)
                {
                    stringBuilder
                        .AppendLine()
                        .Append(".GenerateValueOnAdd()");
                }
                else if (property.IsStoreComputed)
                {
                    stringBuilder
                        .AppendLine()
                        .Append(".StoreComputed()");
                }

                GeneratePropertyAnnotations(property, stringBuilder);
            }

            stringBuilder.Append(";");
        }

        protected virtual void GeneratePropertyAnnotations([NotNull] IProperty property, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(property, "property");
            Check.NotNull(stringBuilder, "stringBuilder");

            var columnName = property[ColumnNameAnnotationName];
            if (columnName != null)
            {
                stringBuilder
                    .AppendLine()
                    .Append(".ColumnName(")
                    .Append(DelimitString(columnName))
                    .Append(")");
            }

            GenerateAnnotations(property.Annotations.Where(a => a.Name != ColumnNameAnnotationName).ToArray(), stringBuilder);
        }

        protected virtual void GenerateKey(
            [CanBeNull] IKey key, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(stringBuilder, "stringBuilder");

            if (key == null)
            {
                return;
            }

            stringBuilder
                .AppendLine()
                .Append("b.Key(")
                .Append(key.Properties.Select(p => DelimitString(p.Name)).Join())
                .Append(")");

            var keyName = key.Relational().Name;
            if (keyName != null)
            {
                using (stringBuilder.Indent())
                {
                    stringBuilder
                        .AppendLine()
                        .Append(".ForRelational(rb => rb.Name(")
                        .Append(DelimitString(keyName))
                        .Append("))");
                }
            }

            using (stringBuilder.Indent())
            {
                GenerateAnnotations(key.Annotations.Where(a => a.Name != KeyNameAnnotationName).ToArray(), stringBuilder);
            }

            stringBuilder.Append(";");
        }

        protected virtual void GenerateEntityTypeAnnotations([NotNull] IEntityType entityType, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotNull(stringBuilder, "stringBuilder");

            var tableName = entityType.Relational().Table;
            var schema = entityType.Relational().Schema;
            if (tableName != entityType.SimpleName
                || schema != null)
            {
                stringBuilder
                    .AppendLine()
                    .Append("b.ForRelational().Table(")
                    .Append(DelimitString(tableName));

                if (schema != null)
                {
                    stringBuilder
                        .Append(", ")
                        .Append(DelimitString(schema));
                }

                stringBuilder.Append(");");
            }

            var annotations = entityType.Annotations.Where(a => a.Name != TableNameAnnotationName && a.Name != SchemaAnnotationName).ToArray();
            if (annotations.Any())
            {
                foreach (var annotation in annotations)
                {
                    stringBuilder
                        .AppendLine()
                        .Append("b");

                    GenerateAnnotation(annotation, stringBuilder);

                    stringBuilder.Append(";");
                }
            }
        }

        protected virtual void GenerateForeignKeys(
            [NotNull] IReadOnlyList<IForeignKey> foreignKeys, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(foreignKeys, "foreignKeys");
            Check.NotNull(stringBuilder, "stringBuilder");

            foreach (var foreignKey in foreignKeys)
            {
                GenerateForeignKey(foreignKey, stringBuilder);
            }
        }

        protected virtual void GenerateForeignKey(
            [NotNull] IForeignKey foreignKey, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(foreignKey, "foreignKey");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .AppendLine()
                .Append("b.ForeignKey(")
                .Append(DelimitString(foreignKey.ReferencedEntityType.Name))
                .Append(", ")
                .Append(foreignKey.Properties.Select(p => DelimitString(p.Name)).Join())
                .Append(")");

            GenerateForeignKeyAnnotations(foreignKey, stringBuilder);

            stringBuilder.Append(";");
        }

        protected virtual void GenerateForeignKeyAnnotations([NotNull] IForeignKey foreignKey, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(foreignKey, "foreignKey");
            Check.NotNull(stringBuilder, "stringBuilder");

            var foreignKeyName = foreignKey.Relational().Name;
            if (foreignKeyName != null)
            {
                using (stringBuilder.Indent())
                {
                    stringBuilder
                        .AppendLine()
                        .Append(".ForRelational(rb => rb.Name(")
                        .Append(DelimitString(foreignKeyName))
                        .Append("))");
                }
            }

            using (stringBuilder.Indent())
            {
                GenerateAnnotations(foreignKey.Annotations.Where(a => a.Name != KeyNameAnnotationName).ToArray(), stringBuilder);
            }
        }

        protected virtual void GenerateAnnotations(
            [NotNull] IReadOnlyList<IAnnotation> annotations, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(annotations, "annotations");
            Check.NotNull(stringBuilder, "stringBuilder");

            foreach (var annotation in annotations)
            {
                stringBuilder.AppendLine();

                GenerateAnnotation(annotation, stringBuilder);
            }
        }

        protected virtual void GenerateAnnotation(
            [NotNull] IAnnotation annotation, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(annotation, "annotation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append(".Annotation(")
                .Append(DelimitString(annotation.Name))
                .Append(", ")
                .Append(DelimitString(annotation.Value))
                .Append(")");
        }

        protected virtual string DelimitString([NotNull] string value)
        {
            Check.NotNull(value, "value");

            return "\"" + EscapeString(value) + "\"";
        }

        protected virtual string EscapeString([NotNull] string value)
        {
            Check.NotEmpty(value, "value");

            return value.Replace("\"", "\\\"");
        }
    }
}
