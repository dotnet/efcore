// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Model
{
    // TODO: Use this instead of the DefaultValue, DefaultSql pair.

    public struct DefaultConstraint
    {
        private object _valueOrSql;
        private bool _isSql;

        public static DefaultConstraint Value([NotNull] object value)
        {
            Check.NotNull(value, "value");

            return new DefaultConstraint { _valueOrSql = value, _isSql = false };
        }

        public static DefaultConstraint Sql([NotNull] string sql)
        {
            Check.NotEmpty(sql, "sql");

            return new DefaultConstraint { _valueOrSql = sql, _isSql = true };
        }

        public bool IsNull
        {
            get { return _valueOrSql == null; }
        }

        public object GetValue()
        {
            return _isSql ? null : _valueOrSql;
        }

        public string GetSql()
        {
            return _isSql ? (string)_valueOrSql : null;
        }
    }
}
