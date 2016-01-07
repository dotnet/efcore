// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Scaffolding
{
    public class TableSelectionSet
    {
        public readonly static TableSelectionSet All = new TableSelectionSet();

        public TableSelectionSet()
            : this(null, null)
        {
        }

        public TableSelectionSet([CanBeNull] IReadOnlyList<string> tables)
            : this(tables, null)
        {
        }

        public TableSelectionSet([CanBeNull] IReadOnlyList<string> tables, [CanBeNull] IReadOnlyList<string> schemas)
        {
            Schemas = schemas?.Select(schema => new Selection(schema)).ToList().AsReadOnly() ?? new List<Selection>().AsReadOnly();
            Tables = tables?.Select(table => new Selection(table)).ToList().AsReadOnly() ?? new List<Selection>().AsReadOnly();
        }

        public virtual IReadOnlyList<Selection> Schemas { get; }
        public virtual IReadOnlyList<Selection> Tables { get; }

        public class Selection
        {
            public Selection([NotNull] string selectionText)
            {
                Check.NotEmpty(selectionText, nameof(selectionText));

                Text = selectionText;
            }

            public virtual string Text { get; }
            public virtual bool IsMatched { get; [param: NotNull] set; }
        }
    }
}
