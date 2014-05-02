// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational.Model
{
    public class PrimaryKey
    {
        private readonly string _name;
        private readonly IReadOnlyList<Column> _columns;
        private readonly bool _isClustered;

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
