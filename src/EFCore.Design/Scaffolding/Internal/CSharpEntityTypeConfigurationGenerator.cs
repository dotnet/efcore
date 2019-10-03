// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{        /// <summary>
         ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
         ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
         ///     any release. You should only use it directly in your code with extreme caution and knowing that
         ///     doing so can result in application failures when updating to a new Entity Framework Core release.
         /// </summary>
    public class CSharpEntityTypeConfigurationGenerator : ICSharpEntityTypeConfigurationGenerator
    {
        private const string BuilderIdentifier = "builder";

        private readonly ICSharpHelper _code;
        private readonly ICSharpFluentConfigurationCodeGenerator _fluentGenerator;

        private IndentedStringBuilder _sb;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CSharpEntityTypeConfigurationGenerator(
            [NotNull] ICSharpHelper cSharpHelper, ICSharpFluentConfigurationCodeGenerator cSharpFluentConfigurationGenerator)
        {
            Check.NotNull(cSharpHelper, nameof(cSharpHelper));

            _code = cSharpHelper;
            _fluentGenerator = cSharpFluentConfigurationGenerator;
        }
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string WriteCode([NotNull] IEntityType entityType, [NotNull] string @namespace, string classSuffix)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(@namespace, nameof(@namespace));
            Check.NotNull(classSuffix, nameof(classSuffix));

            _sb = new IndentedStringBuilder();

            _sb.AppendLine("using Microsoft.EntityFrameworkCore;");
            _sb.AppendLine("using Microsoft.EntityFrameworkCore.Metadata.Builders;");

            _sb.AppendLine();
            _sb.AppendLine($"namespace {@namespace}");
            _sb.AppendLine("{");

            using (_sb.Indent())
            {
                GenerateClass(entityType, classSuffix);
            }

            _sb.AppendLine("}");

            return _sb.ToString();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void GenerateClass(IEntityType entityType, string classSuffix)
        {
            Check.NotNull(entityType, nameof(entityType));

            _sb.AppendLine($"public partial class {entityType.Name}{classSuffix} : IEntityTypeConfiguration<{entityType.Name}>");

            _sb.AppendLine("{");

            using (_sb.Indent())
            {
                _sb.AppendLine($"public void Configure(EntityTypeBuilder<{entityType.Name}> {BuilderIdentifier})");
                _sb.Append("{");

                using (_sb.Indent())
                {
                    GenerateTableOrView(entityType);

                    GeneratePrimaryKey(entityType);

                    foreach (var property in entityType.GetProperties())
                    {
                        GenerateProperty(property);
                    }

                    foreach (var foreignKey in entityType.GetForeignKeys())
                    {
                        GenerateRelationship(foreignKey);
                    }

                    foreach (var index in entityType.GetIndexes())
                    {
                        GenerateIndex(index);
                    }
                }

                _sb.AppendLine("}");
            }

            _sb.AppendLine("}");
        }

        private void GenerateTableOrView(IEntityType entityType)
        {
            var lines = _fluentGenerator.GenerateTableOrView(entityType);

            AppendMultiLineFluentApiToBuilder(lines);
        }

        private void GeneratePrimaryKey(IEntityType entityType)
        {
            var key = entityType.FindPrimaryKey();

            var lines = _fluentGenerator.GenerateKey(key, false, true);

            AppendMultiLineFluentApiToBuilder(lines);
        }

        private void GenerateProperty(IProperty property)
        {
            var lines = _fluentGenerator.GenerateProperty(property, true);

            AppendMultiLineFluentApiToBuilder(lines);
        }

        private void GenerateRelationship(IForeignKey foreignKey)
        {
            var lines = _fluentGenerator.GenerateRelationship(foreignKey, true);

            AppendMultiLineFluentApiToBuilder(lines);
        }

        private void GenerateIndex(IIndex index)
        {
            var lines = _fluentGenerator.GenerateIndex(index);

            AppendMultiLineFluentApiToBuilder(lines);
        }

        private void AppendMultiLineFluentApiToBuilder(IList<string> lines)
        {
            if (lines == null || lines.Count <= 0)
            {
                return;
            }

            //add a blank line at the top
            _sb.AppendLine();

            //append the first line directly to the builder
            _sb.Append($"{BuilderIdentifier}{lines[0]}");

            //each subsequent line is added as a fluent api style method call
            using (_sb.Indent())
            {
                foreach (var line in lines.Skip(1))
                {
                    _sb.AppendLine();
                    _sb.Append(line);
                }
            }

            _sb.AppendLine(";");
        }
    }
}
