// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Microsoft.Data.Relational.Update
{
    internal class ModificationCommand
    {
        private readonly string _tableName;
        private readonly KeyValuePair<string, object>[] _columnValues;
        private readonly KeyValuePair<string, object>[] _whereClauses;

        public ModificationCommand(string tableName,
            IEnumerable<KeyValuePair<string, object>> columnValues, IEnumerable<KeyValuePair<string, object>> whereClauses)
        {
            Contract.Assert(columnValues != null || whereClauses != null, "both columnValues and whereClauses are null");

            _tableName = tableName;
            _columnValues = columnValues != null ? columnValues.ToArray() : null;
            _whereClauses = whereClauses != null ? whereClauses.ToArray() : null;
        }

        public ModificationOperation Operation
        {
            get
            {
                return
                    _columnValues != null && _whereClauses != null
                        ? ModificationOperation.Update
                        : _columnValues != null
                            ? ModificationOperation.Insert
                            : ModificationOperation.Delete;
            }
        }

        public string TableName
        {
            get { return _tableName; }
        }

        public KeyValuePair<string, object>[] ColumnValues
        {
            get { return _columnValues; }
        }

        public KeyValuePair<string, object>[] WhereClauses
        {
            get { return _whereClauses; }
        }
    }
}
