// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Commands.Utilities;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Commands.Migrations
{
    // TODO: Replace usage of BasicModelBuilder with direct calls to core metadata (See #1484)
    public class CSharpModelGenerator
    {
        private readonly CSharpHelper _code;

        public CSharpModelGenerator([NotNull] CSharpHelper code)
        {
            Check.NotNull(code, nameof(code));

            _code = code;
        }

        public virtual void Generate([NotNull] IModel model, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            stringBuilder.Append("var builder = new BasicModelBuilder()");

            using (stringBuilder.Indent())
            {
                GenerateAnnotations(model.Annotations.ToArray(), stringBuilder);
            }

            stringBuilder.AppendLine(";");

            GenerateEntityTypes(model.EntityTypes, stringBuilder);

            stringBuilder
                .AppendLine()
                .AppendLine("return builder.Model;");
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
            Check.NotNull(entityTypes, nameof(entityTypes));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            foreach (var entityType in entityTypes)
            {
                stringBuilder.AppendLine();

                GenerateEntityType(entityType, stringBuilder, GenerateEntityTypeOptions.Primary);
            }

            foreach (var entityType in entityTypes.Where(e => e.GetForeignKeys().Any()))
            {
                stringBuilder.AppendLine();

                GenerateEntityType(entityType, stringBuilder, GenerateEntityTypeOptions.Secondary);
            }
        }

        protected virtual void GenerateEntityType(
            [NotNull] IEntityType entityType, [NotNull] IndentedStringBuilder stringBuilder, GenerateEntityTypeOptions options)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            stringBuilder
                .Append("builder.Entity(")
                .Append(_code.Literal(entityType.Name))
                .AppendLine(", b =>");

            using (stringBuilder.Indent())
            {
                stringBuilder.Append("{");

                using (stringBuilder.Indent())
                {
                    if ((options & GenerateEntityTypeOptions.Primary) != 0)
                    {
                        GenerateProperties(entityType.GetProperties(), stringBuilder);

                        GenerateKey(entityType.GetPrimaryKey(), stringBuilder);
                    }

                    if ((options & GenerateEntityTypeOptions.Secondary) != 0)
                    {
                        GenerateForeignKeys(entityType.GetForeignKeys(), stringBuilder);
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
            [NotNull] IEnumerable<IProperty> properties, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(properties, nameof(properties));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            foreach (var property in properties)
            {
                GenerateProperty(property, stringBuilder);
            }
        }

        protected virtual void GenerateProperty(
            [NotNull] IProperty property, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            stringBuilder
                .AppendLine()
                .Append("b.Property<")
                .Append(_code.Reference(property.ClrType.UnwrapEnumType()))
                .Append(">(")
                .Append(_code.Literal(property.Name))
                .Append(")");

            using (stringBuilder.Indent())
            {
                if (property.IsConcurrencyToken)
                {
                    stringBuilder
                        .AppendLine()
                        .Append(".ConcurrencyToken()");
                }

                if (property.IsValueGeneratedOnAdd)
                {
                    stringBuilder
                        .AppendLine()
                        .Append(".GenerateValueOnAdd()");
                }
                else if (property.StoreGeneratedPattern != StoreGeneratedPattern.None)
                {
                    stringBuilder
                        .AppendLine()
                        .Append(".StoreGeneratedPattern(StoreGeneratedPattern.")
                        .Append(property.StoreGeneratedPattern.ToString())
                        .Append(")");
                }

                GeneratePropertyAnnotations(property, stringBuilder);
            }

            stringBuilder.Append(";");
        }

        protected virtual void GeneratePropertyAnnotations([NotNull] IProperty property, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            GenerateAnnotations(property.Annotations.ToArray(), stringBuilder);
        }

        protected virtual void GenerateKey(
            [CanBeNull] IKey key, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            if (key == null)
            {
                return;
            }

            stringBuilder
                .AppendLine()
                .Append("b.Key(")
                .Append(string.Join(", ", key.Properties.Select(p => _code.Literal(p.Name))))
                .Append(")");

            using (stringBuilder.Indent())
            {
                GenerateAnnotations(key.Annotations.ToArray(), stringBuilder);
            }

            stringBuilder.Append(";");
        }

        protected virtual void GenerateEntityTypeAnnotations([NotNull] IEntityType entityType, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            var annotations = entityType.Annotations.ToArray();
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
            [NotNull] IEnumerable<IForeignKey> foreignKeys, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(foreignKeys, nameof(foreignKeys));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            foreach (var foreignKey in foreignKeys)
            {
                GenerateForeignKey(foreignKey, stringBuilder);
            }
        }

        protected virtual void GenerateForeignKey(
            [NotNull] IForeignKey foreignKey, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            stringBuilder
                .AppendLine()
                .Append("b.ForeignKey(")
                .Append(_code.Literal(foreignKey.PrincipalEntityType.Name))
                .Append(", ")
                .Append(string.Join(", ", foreignKey.Properties.Select(p => _code.Literal(p.Name))))
                .Append(")");

            GenerateForeignKeyAnnotations(foreignKey, stringBuilder);

            stringBuilder.Append(";");
        }

        protected virtual void GenerateForeignKeyAnnotations([NotNull] IForeignKey foreignKey, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            using (stringBuilder.Indent())
            {
                GenerateAnnotations(foreignKey.Annotations.ToArray(), stringBuilder);
            }
        }

        protected virtual void GenerateAnnotations(
            [NotNull] IReadOnlyList<IAnnotation> annotations, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(annotations, nameof(annotations));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            foreach (var annotation in annotations)
            {
                stringBuilder.AppendLine();

                GenerateAnnotation(annotation, stringBuilder);
            }
        }

        protected virtual void GenerateAnnotation(
            [NotNull] IAnnotation annotation, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(annotation, nameof(annotation));
            Check.NotNull(stringBuilder, nameof(stringBuilder));

            stringBuilder
                .Append(".Annotation(")
                .Append(_code.Literal(annotation.Name))
                .Append(", ")
                .Append(_code.UnknownLiteral(annotation.Value))
                .Append(")");
        }
    }
}
