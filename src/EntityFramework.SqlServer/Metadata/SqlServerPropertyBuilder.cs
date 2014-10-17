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

        public virtual SqlServerPropertyBuilder DefaultValue([CanBeNull] object value)
        {
            _property.SqlServer().DefaultValue = value;

            return this;
        }

        public virtual SqlServerPropertyBuilder UseSequence()
        {
            _property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Sequence;
            _property.SqlServer().SequenceName = null;
            _property.SqlServer().SequenceSchema = null;

            return this;
        }

        public virtual SqlServerPropertyBuilder UseSequence([NotNull] string name, [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, "name");
            Check.NullButNotEmpty(schema, "schema");

            var sequence = _property.EntityType.Model.SqlServer().GetOrAddSequence(name, schema);

            _property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Sequence;
            _property.SqlServer().SequenceName = sequence.Name;
            _property.SqlServer().SequenceSchema = sequence.Schema;

            return this;
        }

        public virtual SqlServerPropertyBuilder UseIdentity()
        {
            _property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Identity;
            _property.SqlServer().SequenceName = null;
            _property.SqlServer().SequenceSchema = null;

            return this;
        }
    }
}
