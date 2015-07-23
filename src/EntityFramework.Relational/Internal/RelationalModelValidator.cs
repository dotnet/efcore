// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        }

        protected void EnsureDistinctTableNames(IModel model)
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
    }
}
