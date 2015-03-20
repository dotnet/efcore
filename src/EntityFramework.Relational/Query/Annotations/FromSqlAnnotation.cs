// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Query.Annotations
{
    public class FromSqlAnnotation
    {
        public FromSqlAnnotation([NotNull] string sql)
            : this(sql, new object[0])
        {
        }

        public FromSqlAnnotation([NotNull] string sql, [NotNull] object[] parameters)
        {
            Check.NotEmpty(sql, nameof(sql));
            Check.NotNull(parameters, nameof(parameters));

            Sql = sql;
            Parameters = parameters;
        }

        public virtual string Sql { get; }

        public virtual object[] Parameters { get; }

        public override string ToString()
        {
            return string.Format("\"{0}\" ({1})",
                Sql,
                string.Join(", ", Parameters.Select(p => p.ToString())));
        }
    }
}
