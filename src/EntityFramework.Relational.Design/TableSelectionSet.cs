// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Scaffolding
{
    public class TableSelectionSet
    {
        public static readonly TableSelectionSet InclusiveAll = new TableSelectionSet();

        private static readonly List<TableSelection> _inclusiveAll =
            new List <TableSelection> { TableSelection.InclusiveAll };
        private static readonly List<TableSelection> _exclusiveAll =
            new List<TableSelection> { TableSelection.ExclusiveAll };

        private List<TableSelection> _inclusiveSelections = new List<TableSelection>();
        private List<TableSelection> _exclusiveSelections = new List<TableSelection>();

        public virtual void AddSelections([CanBeNull] params TableSelection[] tableSelections)
        {
            if (tableSelections == null)
            {
                return;
            }

            foreach (var tableSelection in tableSelections)
            {
                if (TableSelection.Any == tableSelection.Schema
                    && TableSelection.Any == tableSelection.Table)
                {
                    if (tableSelection.Exclude)
                    {
                        _exclusiveSelections = _exclusiveAll;
                    }
                    else
                    {
                        _inclusiveSelections = _inclusiveAll;
                    }

                    return;
                }

                var listToAddTo =
                    tableSelection.Exclude ? _exclusiveSelections : _inclusiveSelections;

                if (TableSelection.Any == tableSelection.Table)
                {
                    if (listToAddTo.Contains(
                        tableSelection.Exclude ? TableSelection.ExclusiveAll : TableSelection.InclusiveAll))
                    {
                        // list already contains wider-scoped filter
                        return;
                    }
                    listToAddTo.RemoveAll(ts => ts.Schema == tableSelection.Schema);
                    listToAddTo.Add(tableSelection);
                }
                else if (TableSelection.Any == tableSelection.Schema)
                {
                    if (listToAddTo.Contains(
                        tableSelection.Exclude ? TableSelection.ExclusiveAll : TableSelection.InclusiveAll))
                    {
                        // list already contains wider-scoped filter
                        return;
                    }
                    listToAddTo.RemoveAll(ts => ts.Table == tableSelection.Table);
                    listToAddTo.Add(tableSelection);
                }
                else if (!listToAddTo.Exists(
                    ts => ts.Matches(tableSelection.Schema, tableSelection.Table)))
                {
                    listToAddTo.Add(tableSelection);
                }
            }
        }

        public virtual List<TableSelection> InclusiveSelections
        {
            get
            {
                return _inclusiveSelections.Count == 0
                    ? _inclusiveAll
                    : _inclusiveSelections;
            }
        }

        public virtual List<TableSelection> ExclusiveSelections
        {
            get
            {
                return _exclusiveSelections;
            }
        }

        public virtual bool Allows([NotNull] string schemaName, [NotNull] string tableName)
        {
            Check.NotEmpty(schemaName, nameof(schemaName));
            Check.NotEmpty(tableName, nameof(tableName));

            return InclusiveSelections.Exists(ts => ts.Matches(schemaName, tableName))
                && !ExclusiveSelections.Exists(ts => ts.Matches(schemaName, tableName));
        }
    }
}
