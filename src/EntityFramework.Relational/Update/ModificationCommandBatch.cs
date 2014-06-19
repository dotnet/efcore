// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Update
{
    public class ModificationCommandBatch
    {
        private readonly IReadOnlyList<ModificationCommand> _modificationCommands;

        public ModificationCommandBatch([NotNull] IReadOnlyList<ModificationCommand> modificationCommands)
        {
            Check.NotNull(modificationCommands, "modificationCommands");

            _modificationCommands = modificationCommands;
        }

        public virtual string CompileBatch([NotNull] SqlGenerator sqlGenerator)
        {
            Check.NotNull(sqlGenerator, "sqlGenerator");

            var stringBuilder = new StringBuilder();

            sqlGenerator.AppendBatchHeader(stringBuilder);
            if (stringBuilder.Length > 0)
            {
                stringBuilder.Append(sqlGenerator.BatchCommandSeparator).AppendLine();
            }

            foreach (var modificationCommand in _modificationCommands)
            {
                var entityState = modificationCommand.EntityState;
                var operations = modificationCommand.ColumnModifications;
                var tableName = modificationCommand.TableName;

                if (entityState == EntityState.Added)
                {
                    sqlGenerator.AppendInsertOperation(stringBuilder, tableName, operations);
                }

                if (entityState == EntityState.Modified)
                {
                    sqlGenerator.AppendUpdateOperation(stringBuilder, tableName, operations);
                }

                if (entityState == EntityState.Deleted)
                {
                    sqlGenerator.AppendDeleteOperation(stringBuilder, tableName, operations);
                }

                stringBuilder.Append(sqlGenerator.BatchCommandSeparator).AppendLine();
            }

            return stringBuilder.ToString();
        }

        public virtual IReadOnlyList<ModificationCommand> ModificationCommands
        {
            get { return _modificationCommands; }
        }
    }
}
