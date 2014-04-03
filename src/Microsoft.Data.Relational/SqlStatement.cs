// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational
{
    public class SqlStatement
    {
        private readonly string _sql;

        public SqlStatement([NotNull] string sql)
        {
            Check.NotNull(sql, "sql");

            _sql = sql;
        }

        public virtual string Sql
        {
            get { return _sql; }
        }

        public virtual bool SuppressTransaction { get; set; }
    }
}
