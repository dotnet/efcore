// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CSharpEntityTypeGenerator : ICSharpEntityTypeGenerator
    {
        private ICSharpUtilities CSharpUtilities { get; }

        private IndentedStringBuilder _sb;
        private bool _useDataAnnotations;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CSharpEntityTypeGenerator(
            [NotNull] ICSharpUtilities cSharpUtilities)
        {
            Check.NotNull(cSharpUtilities, nameof(cSharpUtilities));

            CSharpUtilities = cSharpUtilities;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string WriteCode(IEntityType entityType, string @namespace, bool useDataAnnotations)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(@namespace, nameof(@namespace));

            _sb = new IndentedStringBuilder();
            _useDataAnnotations = useDataAnnotations;

            _sb.AppendLine("using System;");
            _sb.AppendLine("using System.Collections.Generic;");

            if (_useDataAnnotations)
            {
                _sb.AppendLine("using System.ComponentModel.DataAnnotations;");
                _sb.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
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
            _sb.AppendLine($"namespace {@namespace}");
            _sb.AppendLine("{");

            using (_sb.Indent())
            {
                GenerateClass(entityType);
            }

            _sb.AppendLine("}");

            return _sb.ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void GenerateClass(
            [NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void GenerateEntityTypeDataAnnotations(
            [NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            GenerateTableAttribute(entityType);
        }

        private void GenerateTableAttribute(IEntityType entityType)
        {
            var tableName = entityType.Relational().TableName;
            var schema = entityType.Relational().Schema;
            var defaultSchema = entityType.Model.Relational().DefaultSchema;

            var schemaParameterNeeded = schema != null && schema != defaultSchema;
            var tableAttributeNeeded = schemaParameterNeeded || tableName != null && tableName != entityType.Scaffolding().DbSetName;

            if (tableAttributeNeeded)
            {
                var tableAttribute = new AttributeWriter(nameof(TableAttribute));

                tableAttribute.AddParameter(CSharpUtilities.DelimitString(tableName));

                if (schemaParameterNeeded)
                {
                    tableAttribute.AddParameter($"{nameof(TableAttribute.Schema)} = {CSharpUtilities.DelimitString(schema)}");
                }

                _sb.AppendLine(tableAttribute.ToString());
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void GenerateConstructor(
            [NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            var collectionNavigations = entityType.GetNavigations().Where(n => n.IsCollection()).ToList();

            if (collectionNavigations.Count > 0)
            {
                _sb.AppendLine($"public {entityType.Name}()");
                _sb.AppendLine("{");

                using (_sb.Indent())
                {
                    foreach (var navigation in collectionNavigations)
                    {
                        _sb.AppendLine($"{navigation.Name} = new HashSet<{navigation.GetTargetType().Name}>();");
                    }
                }

                _sb.AppendLine("}");
                _sb.AppendLine();
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void GenerateProperties(
            [NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            foreach (var property in entityType.GetProperties().OrderBy(p => p.Scaffolding().ColumnOrdinal))
            {
                if (_useDataAnnotations)
                {
                    GeneratePropertyDataAnnotations(property);
                }

                _sb.AppendLine($"public {CSharpUtilities.GetTypeName(property.ClrType)} {property.Name} {{ get; set; }}");
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void GeneratePropertyDataAnnotations(
            [NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            GenerateKeyAttribute(property);
            GenerateRequiredAttribute(property);
            GenerateColumnAttribute(property);
            GenerateMaxLengthAttribute(property);
        }

        private void GenerateKeyAttribute(IProperty property)
        {
            var key = property.AsProperty().PrimaryKey;

            if (key?.Properties.Count == 1)
            {
                if (key is Key concreteKey
                    && key.Properties.SequenceEqual(new KeyDiscoveryConvention(null).DiscoverKeyProperties(concreteKey.DeclaringEntityType, concreteKey.DeclaringEntityType.GetProperties().ToList())))
                {
                    return;
                }

                if (key.Relational().Name != ConstraintNamer.GetDefaultName(key))
                {
                    return;
                }

                _sb.AppendLine(new AttributeWriter(nameof(KeyAttribute)));
            }
        }

        private void GenerateColumnAttribute(IProperty property)
        {
            var columnName = property.Relational().ColumnName;
            var columnType = property.GetConfiguredColumnType();

            var delimitedColumnName = columnName != null && columnName != property.Name ? CSharpUtilities.DelimitString(columnName) : null;
            var delimitedColumnType = columnType != null ? CSharpUtilities.DelimitString(columnType) : null;

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

                _sb.AppendLine(columnAttribute);
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

                lengthAttribute.AddParameter(CSharpUtilities.GenerateLiteral(maxLength.Value));

                _sb.AppendLine(lengthAttribute.ToString());
            }
        }

        private void GenerateRequiredAttribute(IProperty property)
        {
            if (!property.IsNullable
                && property.ClrType.IsNullableType()
                && !property.IsPrimaryKey())
            {
                _sb.AppendLine(new AttributeWriter(nameof(RequiredAttribute)).ToString());
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual void GenerateNavigationProperties(
            [NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            var sortedNavigations = entityType.GetNavigations()
                .OrderBy(n => n.IsDependentToPrincipal() ? 0 : 1)
                .ThenBy(n => n.IsCollection() ? 1 : 0);

            if (sortedNavigations.Any())
            {
                _sb.AppendLine();

                foreach (var navigation in sortedNavigations)
                {
                    if (_useDataAnnotations)
                    {
                        GenerateNavigationDataAnnotations(navigation);
                    }

                    var referencedTypeName = navigation.GetTargetType().Name;
                    var navigationType = navigation.IsCollection() ? $"ICollection<{referencedTypeName}>" : referencedTypeName;
                    _sb.AppendLine($"public {navigationType} {navigation.Name} {{ get; set; }}");
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
            if (navigation.IsDependentToPrincipal())
            {
                if (navigation.ForeignKey.PrincipalKey.IsPrimaryKey())
                {
                    var foreignKeyAttribute = new AttributeWriter(nameof(ForeignKeyAttribute));

                    foreignKeyAttribute.AddParameter(
                        CSharpUtilities.DelimitString(
                            string.Join(",", navigation.ForeignKey.Properties.Select(p => p.Name))));

                    _sb.AppendLine(foreignKeyAttribute.ToString());
                }
            }
        }

        private void GenerateInversePropertyAttribute(INavigation navigation)
        {
            if (navigation.ForeignKey.PrincipalKey.IsPrimaryKey())
            {
                var inverseNavigation = navigation.FindInverse();

                if (inverseNavigation != null)
                {
                    var inversePropertyAttribute = new AttributeWriter(nameof(InversePropertyAttribute));

                    inversePropertyAttribute.AddParameter(CSharpUtilities.DelimitString(inverseNavigation.Name));

                    _sb.AppendLine(inversePropertyAttribute.ToString());
                }
            }
        }

        private class AttributeWriter
        {
            private readonly string _attibuteName;
            private readonly List<string> _parameters = new List<string>();

            public AttributeWriter([NotNull] string attributeName)
            {
                Check.NotEmpty(attributeName, nameof(attributeName));

                _attibuteName = attributeName;
            }

            public void AddParameter([NotNull] string parameter)
            {
                Check.NotEmpty(parameter, nameof(parameter));

                _parameters.Add(parameter);
            }

            public override string ToString()
                => "[" + (_parameters.Count == 0
                       ? StripAttribute(_attibuteName)
                       : StripAttribute(_attibuteName) + "(" + string.Join(", ", _parameters) + ")") + "]";

            private static string StripAttribute([NotNull] string attributeName)
                => attributeName.EndsWith("Attribute", StringComparison.Ordinal)
                    ? attributeName.Substring(0, attributeName.Length - 9)
                    : attributeName;
        }
    }
}
