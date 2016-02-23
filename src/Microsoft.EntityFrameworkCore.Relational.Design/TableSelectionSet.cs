// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    public class TableSelectionSet
    {
        public static readonly TableSelectionSet All = new TableSelectionSet();

        public TableSelectionSet()
            : this(Enumerable.Empty<string>(), Enumerable.Empty<string>())
        {
        }

        public TableSelectionSet([NotNull] IEnumerable<string> tables)
            : this(tables, Enumerable.Empty<string>())
        {
        }

        public TableSelectionSet([NotNull] IEnumerable<string> tables, [NotNull] IEnumerable<string> schemas)
        {
            Check.NotNull(tables, nameof(tables));
            Check.NotNull(schemas, nameof(schemas));

            Schemas = schemas.Select(schema => new Selection(schema)).ToList();
            Tables = tables.Select(table => new Selection(table)).ToList();
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
