// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class RelationalModelValidator : LoggingModelValidator
    {
        private readonly IRelationalAnnotationProvider _relationalExtensions;

        public RelationalModelValidator(
            [NotNull] ILogger<RelationalModelValidator> loggerFactory,
            [NotNull] IRelationalAnnotationProvider relationalExtensions)
            : base(loggerFactory)
        {
            _relationalExtensions = relationalExtensions;
        }

        public override void Validate(IModel model)
        {
            base.Validate(model);

            EnsureDistinctTableNames(model);
            EnsureDistinctColumnNames(model);
            ValidateInheritanceMapping(model);
        }

        protected virtual void EnsureDistinctTableNames([NotNull] IModel model)
        {
            var tables = new HashSet<string>();
            foreach (var entityType in model.GetEntityTypes().Where(et => et.BaseType == null))
            {
                var annotations = _relationalExtensions.For(entityType);

                var name = annotations.Schema + "." + annotations.TableName;

                if (!tables.Add(name))
                {
                    ShowError(RelationalStrings.DuplicateTableName(annotations.TableName, annotations.Schema, entityType.DisplayName()));
                }
            }
        }

        protected virtual void EnsureDistinctColumnNames([NotNull] IModel model)
        {
            foreach (var entityType in model.GetEntityTypes())
            {
                var columns = new HashSet<string>();
                foreach (var property in entityType.GetProperties())
                {
                    var name = _relationalExtensions.For(property).ColumnName;
                    if (!columns.Add(name))
                    {
                        ShowError(RelationalStrings.DuplicateColumnName(name, entityType.Name, property.Name));
                    }
                }
            }
        }

        protected virtual void ValidateInheritanceMapping([NotNull] IModel model)
        {
            var roots = new HashSet<IEntityType>();
            foreach (var entityType in model.GetEntityTypes().Where(et => et.BaseType != null))
            {
                ValidateDiscriminator(entityType);

                roots.Add(entityType.RootType());
            }

            foreach (var entityType in roots)
            {
                ValidateDiscriminator(entityType);
            }
        }

        private void ValidateDiscriminator(IEntityType entityType)
        {
            if (entityType.ClrType?.IsInstantiable() ?? false)
            {
                var annotations = _relationalExtensions.For(entityType);
                if (annotations.DiscriminatorProperty == null)
                {
                    ShowError(RelationalStrings.NoDiscriminatorProperty(entityType.DisplayName()));
                }
                if (annotations.DiscriminatorValue == null)
                {
                    ShowError(RelationalStrings.NoDiscriminatorValue(entityType.DisplayName()));
                }
            }
        }
    }
}
