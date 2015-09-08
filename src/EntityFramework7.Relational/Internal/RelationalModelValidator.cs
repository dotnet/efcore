// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Internal
{
    public class RelationalModelValidator : LoggingModelValidator
    {
        public RelationalModelValidator([NotNull] ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
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
                var name = entityType.Relational().Schema + "." + entityType.Relational().Table;
                if (!tables.Add(name))
                {
                    ShowError(Relational.Internal.Strings.DuplicateTableName(entityType.Relational().Table, entityType.Relational().Schema, entityType.DisplayName()));
                }
            }
        }
    }
}
