// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Model
{
    public class UniqueConstraint
    {
        private readonly string _name;
        private readonly IReadOnlyList<Column> _columns;

        public UniqueConstraint(
            [NotNull] string name,
            [NotNull] IReadOnlyList<Column> columns)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(columns, "columns");

            _name = name;
            _columns = columns;
        }

        public virtual Table Table
        {
            get { return _columns[0].Table; }
        }

        public virtual string Name
        {
            get { return _name; }
        }

        public virtual IReadOnlyList<Column> Columns
        {
            get { return _columns; }
        }

        public virtual UniqueConstraint Clone([NotNull] CloneContext cloneContext)
        {
            Check.NotNull(cloneContext, "cloneContext");

            return
                new UniqueConstraint(
                    Name,
                    Columns.Select(column => column.Clone(cloneContext)).ToArray());
        }
    }
}
