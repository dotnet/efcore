// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CSharpEntityTypeGenerator : ICSharpEntityTypeGenerator
    {
        private readonly IAnnotationCodeGenerator _annotationCodeGenerator;
        private readonly ICSharpHelper _code;

        private IndentedStringBuilder _sb = null!;
        private bool _useDataAnnotations;
        private bool _useNullableReferenceTypes;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CSharpEntityTypeGenerator(
            IAnnotationCodeGenerator annotationCodeGenerator,
            ICSharpHelper cSharpHelper)
        {
            Check.NotNull(cSharpHelper, nameof(cSharpHelper));

            _annotationCodeGenerator = annotationCodeGenerator;
            _code = cSharpHelper;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string WriteCode(IEntityType entityType, string? @namespace, bool useDataAnnotations, bool useNullableReferenceTypes)
        {
            Check.NotNull(entityType, nameof(entityType));

            _useDataAnnotations = useDataAnnotations;
            _useNullableReferenceTypes = useNullableReferenceTypes;

            _sb = new IndentedStringBuilder();

            _sb.AppendLine("using System;");
            _sb.AppendLine("using System.Collections.Generic;");

            if (_useDataAnnotations)
            {
                _sb.AppendLine("using System.ComponentModel.DataAnnotations;");
                _sb.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
                _sb.AppendLine("using Microsoft.EntityFrameworkCore;"); // For attributes coming out of Abstractions
            }

            foreach (var ns in entityType.GetProperties()
                .SelectMany(p => p.ClrType.GetNamespaces())
                .Where(ns => ns != "System" && ns != "System.Collections.Generic")
                .Distinct()
                .OrderBy(x => x, new NamespaceComparer()))
            {
                _sb.AppendLine($"using {ns};");
            }

            _sb.AppendLine();

            if (!string.IsNullOrEmpty(@namespace))
            {
                _sb.AppendLine($"namespace {@namespace}");
                _sb.AppendLine("{");
                _sb.IncrementIndent();
            }

            GenerateClass(entityType);

            if (!string.IsNullOrEmpty(@namespace))
            {
                _sb.DecrementIndent();
                _sb.AppendLine("}");
            }

            return _sb.ToString();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void GenerateClass(IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            GenerateComment(entityType.GetComment());

            if (_useDataAnnotations)
            {
                GenerateEntityTypeDataAnnotations(entityType);
            }

            _sb.AppendLine($"public partial class {entityType.Name}");

            _sb.AppendLine("{");

            using (_sb.Indent())
            {
                GenerateConstructor(entityType);
                GenerateProperties(entityType);
                GenerateNavigationProperties(entityType);
            }

            _sb.AppendLine("}");
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void GenerateEntityTypeDataAnnotations(IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            GenerateKeylessAttribute(entityType);
            GenerateTableAttribute(entityType);
            GenerateIndexAttributes(entityType);

            var annotations = _annotationCodeGenerator
                .FilterIgnoredAnnotations(entityType.GetAnnotations())
                .ToDictionary(a => a.Name, a => a);
            _annotationCodeGenerator.RemoveAnnotationsHandledByConventions(entityType, annotations);

            foreach (var attribute in _annotationCodeGenerator.GenerateDataAnnotationAttributes(entityType, annotations))
            {
                var attributeWriter = new AttributeWriter(attribute.Type.Name);
                foreach (var argument in attribute.Arguments)
                {
                    attributeWriter.AddParameter(_code.UnknownLiteral(argument));
                }

                _sb.AppendLine(attributeWriter.ToString());
            }
        }

        private void GenerateKeylessAttribute(IEntityType entityType)
        {
            if (entityType.FindPrimaryKey() == null)
            {
                _sb.AppendLine(new AttributeWriter(nameof(KeylessAttribute)).ToString());
            }
        }

        private void GenerateTableAttribute(IEntityType entityType)
        {
            var tableName = entityType.GetTableName();
            var schema = entityType.GetSchema();
            var defaultSchema = entityType.Model.GetDefaultSchema();

            var schemaParameterNeeded = schema != null && schema != defaultSchema;
            var isView = entityType.GetViewName() != null;
            var tableAttributeNeeded = !isView && (schemaParameterNeeded || tableName != null && tableName != entityType.GetDbSetName());
            if (tableAttributeNeeded)
            {
                var tableAttribute = new AttributeWriter(nameof(TableAttribute));

                tableAttribute.AddParameter(_code.Literal(tableName!));

                if (schemaParameterNeeded)
                {
                    tableAttribute.AddParameter($"{nameof(TableAttribute.Schema)} = {_code.Literal(schema!)}");
                }

                _sb.AppendLine(tableAttribute.ToString());
            }
        }

        private void GenerateIndexAttributes(IEntityType entityType)
        {
            // Do not generate IndexAttributes for indexes which
            // would be generated anyway by convention.
            foreach (var index in entityType.GetIndexes().Where(
                i => ConfigurationSource.Convention != ((IConventionIndex)i).GetConfigurationSource()))
            {
                // If there are annotations that cannot be represented using an IndexAttribute then use fluent API instead.
                var annotations = _annotationCodeGenerator
                    .FilterIgnoredAnnotations(index.GetAnnotations())
                    .ToDictionary(a => a.Name, a => a);
                _annotationCodeGenerator.RemoveAnnotationsHandledByConventions(index, annotations);

                if (annotations.Count == 0)
                {
                    var indexAttribute = new AttributeWriter(nameof(IndexAttribute));
                    foreach (var property in index.Properties)
                    {
                        indexAttribute.AddParameter($"nameof({property.Name})");
                    }

                    if (index.Name != null)
                    {
                        indexAttribute.AddParameter($"{nameof(IndexAttribute.Name)} = {_code.Literal(index.Name)}");
                    }

                    if (index.IsUnique)
                    {
                        indexAttribute.AddParameter($"{nameof(IndexAttribute.IsUnique)} = {_code.Literal(index.IsUnique)}");
                    }

                    _sb.AppendLine(indexAttribute.ToString());
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void GenerateConstructor(IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            var collectionNavigations = entityType.GetNavigations().Where(n => n.IsCollection).ToList();

            if (collectionNavigations.Count > 0)
            {
                _sb.AppendLine($"public {entityType.Name}()");
                _sb.AppendLine("{");

                using (_sb.Indent())
                {
                    foreach (var navigation in collectionNavigations)
                    {
                        _sb.AppendLine($"{navigation.Name} = new HashSet<{navigation.TargetEntityType.Name}>();");
                    }
                }

                _sb.AppendLine("}");
                _sb.AppendLine();
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void GenerateProperties(IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            foreach (var property in entityType.GetProperties().OrderBy(p => p.GetColumnOrdinal()))
            {
                GenerateComment(property.GetComment());

                if (_useDataAnnotations)
                {
                    GeneratePropertyDataAnnotations(property);
                }

                _sb.AppendLine(
                    !_useNullableReferenceTypes || property.ClrType.IsValueType
                        ? $"public {_code.Reference(property.ClrType)} {property.Name} {{ get; set; }}"
                        : property.IsNullable
                            ? $"public {_code.Reference(property.ClrType)}? {property.Name} {{ get; set; }}"
                            : $"public {_code.Reference(property.ClrType)} {property.Name} {{ get; set; }} = null!;");
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void GeneratePropertyDataAnnotations(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            GenerateKeyAttribute(property);
            GenerateRequiredAttribute(property);
            GenerateColumnAttribute(property);
            GenerateMaxLengthAttribute(property);
            GenerateUnicodeAttribute(property);
            GeneratePrecisionAttribute(property);

            var annotations = _annotationCodeGenerator
                .FilterIgnoredAnnotations(property.GetAnnotations())
                .ToDictionary(a => a.Name, a => a);
            _annotationCodeGenerator.RemoveAnnotationsHandledByConventions(property, annotations);

            foreach (var attribute in _annotationCodeGenerator.GenerateDataAnnotationAttributes(property, annotations))
            {
                var attributeWriter = new AttributeWriter(attribute.Type.Name);
                foreach (var argument in attribute.Arguments)
                {
                    attributeWriter.AddParameter(_code.UnknownLiteral(argument));
                }

                _sb.AppendLine(attributeWriter.ToString());
            }
        }

        private void GenerateKeyAttribute(IProperty property)
        {
            var key = property.FindContainingPrimaryKey();
            if (key != null)
            {
                _sb.AppendLine(new AttributeWriter(nameof(KeyAttribute)).ToString());
            }
        }

        private void GenerateColumnAttribute(IProperty property)
        {
            var columnName = property.GetColumnBaseName();
            var columnType = property.GetConfiguredColumnType();

            var delimitedColumnName = columnName != null && columnName != property.Name ? _code.Literal(columnName) : null;
            var delimitedColumnType = columnType != null ? _code.Literal(columnType) : null;

            if ((delimitedColumnName ?? delimitedColumnType) != null)
            {
                var columnAttribute = new AttributeWriter(nameof(ColumnAttribute));

                if (delimitedColumnName != null)
                {
                    columnAttribute.AddParameter(delimitedColumnName);
                }

                if (delimitedColumnType != null)
                {
                    columnAttribute.AddParameter($"{nameof(ColumnAttribute.TypeName)} = {delimitedColumnType}");
                }

                _sb.AppendLine(columnAttribute.ToString());
            }
        }

        private void GenerateRequiredAttribute(IProperty property)
        {
            if ((!_useNullableReferenceTypes || property.ClrType.IsValueType)
                && !property.IsNullable
                && property.ClrType.IsNullableType()
                && !property.IsPrimaryKey())
            {
                _sb.AppendLine(new AttributeWriter(nameof(RequiredAttribute)).ToString());
            }
        }

        private void GenerateMaxLengthAttribute(IProperty property)
        {
            var maxLength = property.GetMaxLength();

            if (maxLength.HasValue)
            {
                var lengthAttribute = new AttributeWriter(
                    property.ClrType == typeof(string)
                        ? nameof(StringLengthAttribute)
                        : nameof(MaxLengthAttribute));

                lengthAttribute.AddParameter(_code.Literal(maxLength.Value));

                _sb.AppendLine(lengthAttribute.ToString());
            }
        }

        private void GenerateUnicodeAttribute(IProperty property)
        {
            if (property.ClrType != typeof(string))
            {
                return;
            }

            var unicode = property.IsUnicode();
            if (unicode.HasValue)
            {
                var unicodeAttribute = new AttributeWriter(nameof(UnicodeAttribute));
                if (!unicode.Value)
                {
                    unicodeAttribute.AddParameter(_code.Literal(false));
                }
                _sb.AppendLine(unicodeAttribute.ToString());
            }
        }

        private void GeneratePrecisionAttribute(IProperty property)
        {
            var precision = property.GetPrecision();
            if (precision.HasValue)
            {
                var precisionAttribute = new AttributeWriter(nameof(PrecisionAttribute));
                precisionAttribute.AddParameter(_code.Literal(precision.Value));

                var scale = property.GetScale();
                if (scale.HasValue)
                {
                    precisionAttribute.AddParameter(_code.Literal(scale.Value));
                }

                _sb.AppendLine(precisionAttribute.ToString());
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void GenerateNavigationProperties(IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            var sortedNavigations = entityType.GetNavigations()
                .OrderBy(n => n.IsOnDependent ? 0 : 1)
                .ThenBy(n => n.IsCollection ? 1 : 0)
                .ToList();

            if (sortedNavigations.Any())
            {
                _sb.AppendLine();

                foreach (var navigation in sortedNavigations)
                {
                    if (_useDataAnnotations)
                    {
                        GenerateNavigationDataAnnotations(navigation);
                    }

                    var referencedTypeName = navigation.TargetEntityType.Name;
                    var navigationType = navigation.IsCollection ? $"ICollection<{referencedTypeName}>" : referencedTypeName;

                    _sb.AppendLine(
                        !_useNullableReferenceTypes || navigation.IsCollection
                            ? $"public virtual {navigationType} {navigation.Name} {{ get; set; }}"
                            : navigation.ForeignKey.IsRequired
                                ? $"public virtual {navigationType} {navigation.Name} {{ get; set; }} = null!;"
                                : $"public virtual {navigationType}? {navigation.Name} {{ get; set; }}");
                }
            }
        }

        private void GenerateNavigationDataAnnotations(INavigation navigation)
        {
            GenerateForeignKeyAttribute(navigation);
            GenerateInversePropertyAttribute(navigation);
        }

        private void GenerateForeignKeyAttribute(INavigation navigation)
        {
            if (navigation.IsOnDependent)
            {
                if (navigation.ForeignKey.PrincipalKey.IsPrimaryKey())
                {
                    var foreignKeyAttribute = new AttributeWriter(nameof(ForeignKeyAttribute));

                    if (navigation.ForeignKey.Properties.Count > 1)
                    {
                        foreignKeyAttribute.AddParameter(
                            _code.Literal(
                                string.Join(",", navigation.ForeignKey.Properties.Select(p => p.Name))));
                    }
                    else
                    {
                        foreignKeyAttribute.AddParameter($"nameof({navigation.ForeignKey.Properties.First().Name})");
                    }

                    _sb.AppendLine(foreignKeyAttribute.ToString());
                }
            }
        }

        private void GenerateInversePropertyAttribute(INavigation navigation)
        {
            if (navigation.ForeignKey.PrincipalKey.IsPrimaryKey())
            {
                var inverseNavigation = navigation.Inverse;

                if (inverseNavigation != null)
                {
                    var inversePropertyAttribute = new AttributeWriter(nameof(InversePropertyAttribute));

                    inversePropertyAttribute.AddParameter(
                        !navigation.DeclaringEntityType.GetPropertiesAndNavigations().Any(
                            m => m.Name == inverseNavigation.DeclaringEntityType.Name)
                            ? $"nameof({inverseNavigation.DeclaringEntityType.Name}.{inverseNavigation.Name})"
                            : _code.Literal(inverseNavigation.Name));

                    _sb.AppendLine(inversePropertyAttribute.ToString());
                }
            }
        }

        private void GenerateComment(string? comment)
        {
            if (!string.IsNullOrWhiteSpace(comment))
            {
                _sb.AppendLine("/// <summary>");

                foreach (var line in comment.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
                {
                    _sb.AppendLine($"/// {System.Security.SecurityElement.Escape(line)}");
                }

                _sb.AppendLine("/// </summary>");
            }
        }

        private sealed class AttributeWriter
        {
            private readonly string _attributeName;
            private readonly List<string> _parameters = new();

            public AttributeWriter(string attributeName)
            {
                Check.NotEmpty(attributeName, nameof(attributeName));

                _attributeName = attributeName;
            }

            public void AddParameter(string parameter)
            {
                Check.NotEmpty(parameter, nameof(parameter));

                _parameters.Add(parameter);
            }

            public override string ToString()
                => "["
                    + (_parameters.Count == 0
                        ? StripAttribute(_attributeName)
                        : StripAttribute(_attributeName) + "(" + string.Join(", ", _parameters) + ")")
                    + "]";

            private static string StripAttribute(string attributeName)
                => attributeName.EndsWith("Attribute", StringComparison.Ordinal)
                    ? attributeName[..^9]
                    : attributeName;
        }
    }
}
