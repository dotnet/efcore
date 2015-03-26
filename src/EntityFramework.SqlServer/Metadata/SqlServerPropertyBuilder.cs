// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerPropertyBuilder
    {
        private readonly Property _property;

        public SqlServerPropertyBuilder([NotNull] Property property)
        {
            Check.NotNull(property, nameof(property));

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

        public virtual SqlServerPropertyBuilder ComputedExpression([CanBeNull] string expression)
        {
            Check.NullButNotEmpty(expression, nameof(expression));

            _property.SqlServer().ComputedExpression = expression;

            return this;
        }

        public virtual SqlServerPropertyBuilder UseSequence()
        {
            _property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Sequence;
            _property.StoreGeneratedPattern = StoreGeneratedPattern.Identity;
            _property.SqlServer().SequenceName = null;
            _property.SqlServer().SequenceSchema = null;

            return this;
        }

        public virtual SqlServerPropertyBuilder UseSequence([NotNull] string name, [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, "schema");

            var sequence = _property.EntityType.Model.SqlServer().GetOrAddSequence(name, schema);

            _property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Sequence;
            _property.StoreGeneratedPattern = StoreGeneratedPattern.Identity;
            _property.SqlServer().SequenceName = sequence.Name;
            _property.SqlServer().SequenceSchema = sequence.Schema;

            return this;
        }

        public virtual SqlServerPropertyBuilder UseIdentity()
        {
            _property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Identity;
            _property.StoreGeneratedPattern = StoreGeneratedPattern.Identity;
            _property.SqlServer().SequenceName = null;
            _property.SqlServer().SequenceSchema = null;

            return this;
        }

        public virtual SqlServerPropertyBuilder UseDefaultValueGeneration()
        {
            _property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Default;
            _property.StoreGeneratedPattern = StoreGeneratedPattern.Identity;
            _property.SqlServer().SequenceName = null;
            _property.SqlServer().SequenceSchema = null;

            return this;
        }

        public virtual SqlServerPropertyBuilder UseNoValueGeneration()
        {
            _property.SqlServer().ValueGenerationStrategy = null;
            _property.SqlServer().SequenceName = null;
            _property.SqlServer().SequenceSchema = null;

            return this;
        }
    }
}
