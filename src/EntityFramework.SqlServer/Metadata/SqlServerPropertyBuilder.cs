// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.SqlServer.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerPropertyBuilder
    {
        private readonly Property _property;

        public SqlServerPropertyBuilder([NotNull] Property property)
        {
            Check.NotNull(property, "property");

            _property = property;
        }

        public virtual SqlServerPropertyBuilder Column([CanBeNull] string columnName)
        {
            Check.NullButNotEmpty(columnName, "columnName");

            _property.SqlServer().Column = columnName;

            return this;
        }

        public virtual SqlServerPropertyBuilder ColumnType([CanBeNull] string columnType)
        {
            Check.NullButNotEmpty(columnType, "columnType");

            _property.SqlServer().ColumnType = columnType;

            return this;
        }

        public virtual SqlServerPropertyBuilder DefaultExpression([CanBeNull] string expression)
        {
            Check.NullButNotEmpty(expression, "expression");

            _property.SqlServer().DefaultExpression = expression;

            return this;
        }
    }
}
