// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Internal
{
    public class RelationalModelValidator : LoggingModelValidator
    {
        private readonly IRelationalMetadataExtensionProvider _relationalExtensions;

        public RelationalModelValidator(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IRelationalMetadataExtensionProvider relationalExtensions)
            : base(loggerFactory)
        {
            Check.NotNull(relationalExtensions, nameof(relationalExtensions));

            _relationalExtensions = relationalExtensions;
        }

        public override void Validate(IModel model)
        {
            base.Validate(model);

            EnsureDistinctTableNames(model);
            ValidateInheritanceMapping(model);
        }

        protected virtual void EnsureDistinctTableNames([NotNull] IModel model)
        {
            var tables = new HashSet<string>();
            foreach (var entityType in model.EntityTypes.Where(et => et.BaseType == null))
            {
                var annotations = _relationalExtensions.For(entityType);

                var name = annotations.Schema + "." + annotations.TableName;

                if (!tables.Add(name))
                {
                    ShowError(Relational.Internal.Strings.DuplicateTableName(annotations.TableName, annotations.Schema, entityType.DisplayName()));
                }
            }
        }

        protected virtual void ValidateInheritanceMapping([NotNull] IModel model)
        {
            var roots = new HashSet<IEntityType>();
            foreach (var entityType in model.EntityTypes.Where(et => et.BaseType != null))
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
                    ShowError(Relational.Internal.Strings.NoDiscriminatorProperty(entityType.DisplayName()));
                }
                if (annotations.DiscriminatorValue == null)
                {
                    ShowError(Relational.Internal.Strings.NoDiscriminatorValue(entityType.DisplayName()));
                }
            }
        }
    }
}
