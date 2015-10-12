// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal
{
    public class DbContextWriter
    {
        private readonly ModelUtilities _modelUtilities;
        private readonly CSharpUtilities _cSharpUtilities;
        private IndentedStringBuilder _sb;
        private ModelConfiguration _model;

        public DbContextWriter(
            [NotNull] ModelUtilities modelUtilities,
            [NotNull] CSharpUtilities cSharpUtilities)
        {
            Check.NotNull(modelUtilities, nameof(modelUtilities));
            Check.NotNull(cSharpUtilities, nameof(cSharpUtilities));

            _modelUtilities = modelUtilities;
            _cSharpUtilities = cSharpUtilities;
        }

        public virtual string WriteCode(
            [NotNull] ModelConfiguration modelConfiguration)
        {
            Check.NotNull(modelConfiguration, nameof(modelConfiguration));

            _model = modelConfiguration;
            _sb = new IndentedStringBuilder();

            _sb.AppendLine("using Microsoft.Data.Entity;");
            _sb.AppendLine("using Microsoft.Data.Entity.Metadata;");
            _sb.AppendLine();
            _sb.AppendLine("namespace " + _model.Namespace());
            _sb.AppendLine("{");
            using (_sb.Indent())
            {
                AddClass();
            }
            _sb.Append("}");

            return _sb.ToString();
        }

        public virtual void AddClass()
        {
            var className =
                string.IsNullOrWhiteSpace(_model.CustomConfiguration.ContextClassName)
                    ? _model.ClassName()
                    : _model.CustomConfiguration.ContextClassName;
            _sb.AppendLine("public partial class " + className + " : DbContext");
            _sb.AppendLine("{");
            using (_sb.Indent())
            {
                AddOnConfiguring();
                AddOnModelCreating();
                AddDbSetProperties();
            }
            _sb.AppendLine("}");
        }

        public virtual void AddOnConfiguring()
        {
            _sb.AppendLine("protected override void OnConfiguring(DbContextOptionsBuilder options)");
            _sb.AppendLine("{");
            using (_sb.Indent())
            {
                foreach (var optionsBuilderConfig in _model.OnConfiguringConfigurations)
                {
                    _sb.AppendLine("options." + optionsBuilderConfig.FluentApi + ";");
                }
            }
            _sb.AppendLine("}");
        }

        public virtual void AddOnModelCreating()
        {
            _sb.AppendLine();
            _sb.AppendLine("protected override void OnModelCreating(ModelBuilder modelBuilder)");
            _sb.AppendLine("{");

            using (_sb.Indent())
            {
                var first = true;
                foreach (var entityConfig in _model.OrderedEntityConfigurations())
                {
                    var fluentApiConfigurations = entityConfig.GetFluentApiConfigurations(_model.CustomConfiguration.UseFluentApiOnly);
                    var propertyConfigurations = entityConfig.GetPropertyConfigurations(_model.CustomConfiguration.UseFluentApiOnly);
                    var relationshipConfigurations = entityConfig.GetRelationshipConfigurations(_model.CustomConfiguration.UseFluentApiOnly);
                    if (fluentApiConfigurations.Count == 0
                        && propertyConfigurations.Count == 0
                        && relationshipConfigurations.Count == 0)
                    {
                        continue;
                    }

                    if (!first)
                    {
                        _sb.AppendLine();
                    }
                    first = false;

                    _sb.AppendLine("modelBuilder.Entity<" + entityConfig.EntityType.Name + ">(entity =>");
                    _sb.AppendLine("{");
                    using (_sb.Indent())
                    {
                        var shouldAddNewLine = AddEntityFluentApi(fluentApiConfigurations);
                        shouldAddNewLine = AddPropertyConfigurations(shouldAddNewLine, propertyConfigurations);
                        shouldAddNewLine = AddRelationshipConfigurations(shouldAddNewLine, relationshipConfigurations);
                    }
                    _sb.AppendLine("});");
                }
            }

            _sb.AppendLine("}");
        }

        public virtual bool AddEntityFluentApi(
            [NotNull] List<IFluentApiConfiguration> fluentApiConfigurations)
        {
            Check.NotNull(fluentApiConfigurations, nameof(fluentApiConfigurations));

            var shouldAddNewLine = false;
            foreach (var entityFluentApi in fluentApiConfigurations)
            {
                if (shouldAddNewLine)
                {
                    _sb.AppendLine();
                }
                shouldAddNewLine = true;
                _sb.AppendLine("entity." + entityFluentApi.FluentApi + ";");
            }

            return shouldAddNewLine;
        }

        public virtual bool AddPropertyConfigurations(
            bool shouldAddNewLine,
            [NotNull] List<PropertyConfiguration> propertyConfigurations)
        {
            Check.NotNull(propertyConfigurations, nameof(propertyConfigurations));

            foreach (var propertyConfig in propertyConfigurations)
            {
                var propertyConfigurationLines =
                    _modelUtilities.LayoutPropertyConfigurationLines(
                        propertyConfig, "property", "    ", _model.CustomConfiguration.UseFluentApiOnly);
                if (shouldAddNewLine)
                {
                    _sb.AppendLine();
                }
                shouldAddNewLine = true;

                if (propertyConfigurationLines.Count == 1)
                {
                    foreach (var line in propertyConfigurationLines)
                    {
                        _sb.AppendLine("entity.Property(e => e." + propertyConfig.Property.Name + ")" + line + ";");
                    }
                }
                else
                {
                    _sb.AppendLine("entity.Property(e => e." + propertyConfig.Property.Name + ")");
                    using (_sb.Indent())
                    {
                        var lineCount = 0;
                        foreach (string line in propertyConfigurationLines)
                        {
                            var outputLine = line;
                            if (++lineCount == propertyConfigurationLines.Count)
                            {
                                outputLine = line + ";";
                            }
                            _sb.AppendLine(outputLine);
                        }
                    }
                }
            }

            return shouldAddNewLine;
        }

        public virtual bool AddRelationshipConfigurations(
            bool shouldAddNewLine,
            [NotNull] List<RelationshipConfiguration> relationshipConfigurations)
        {
            Check.NotNull(relationshipConfigurations, nameof(relationshipConfigurations));

            foreach (var relationshipConfig in relationshipConfigurations)
            {
                if (shouldAddNewLine)
                {
                    _sb.AppendLine();
                }
                shouldAddNewLine = true;
                _sb.AppendLine("entity."
                    + _modelUtilities.LayoutRelationshipConfigurationLine(
                        relationshipConfig, "d", "p")
                    + ";");
            }

            return shouldAddNewLine;
        }

        public virtual void AddDbSetProperties()
        {
            _sb.AppendLine();
            foreach(var entityConfig in _model.OrderedEntityConfigurations())
            {
                _sb.AppendLine("public virtual DbSet<"
                    + entityConfig.EntityType.Name
                    + "> " + entityConfig.EntityType.Name
                    + " { get; set; }");
            }
        }
    }
}
