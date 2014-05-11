// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Model
{
    public class PrimaryKey
    {
        private readonly string _name;
        private readonly IReadOnlyList<Column> _columns;
        private readonly bool _isClustered = true;

        public PrimaryKey(
            [NotNull] string name,
            [NotNull] IReadOnlyList<Column> columns,
            bool isClustered)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(columns, "columns");

            // TODO: Validate input.

            _name = name;
            _columns = columns;
            _isClustered = isClustered;
        }

        public PrimaryKey(
            [NotNull] string name,
            [NotNull] IReadOnlyList<Column> columns)
            : this(name, columns, isClustered: true)
        {
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

        public virtual bool IsClustered
        {
            get { return _isClustered; }
        }
    }
}
