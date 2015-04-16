// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public class RelationalPropertyBuilder
    {
        private readonly Property _property;

        public RelationalPropertyBuilder([NotNull] Property property)
        {
            Check.NotNull(property, nameof(property));

            _property = property;
        }

        public virtual RelationalPropertyBuilder Column([CanBeNull] string columnName)
        {
            Check.NullButNotEmpty(columnName, nameof(columnName));

            _property.Relational().Column = columnName;

            return this;
        }

        public virtual RelationalPropertyBuilder ColumnType([CanBeNull] string columnType)
        {
            Check.NullButNotEmpty(columnType, nameof(columnType));

            _property.Relational().ColumnType = columnType;

            return this;
        }

        public virtual RelationalPropertyBuilder DefaultExpression([CanBeNull] string expression)
        {
            Check.NullButNotEmpty(expression, nameof(expression));

            _property.Relational().DefaultExpression = expression;

            return this;
        }

        public virtual RelationalPropertyBuilder DefaultValue([CanBeNull] object value)
        {
            _property.Relational().DefaultValue = value;

            return this;
        }
    }
}
