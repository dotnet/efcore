// Copyright (c) .NET Foundation. All rights reserved.
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

        public virtual SqlServerPropertyBuilder HasColumnName([CanBeNull] string name)
        {
            Check.NullButNotEmpty(name, nameof(name));

            _property.SqlServer().Column = name;

            return this;
        }

        public virtual SqlServerPropertyBuilder HasColumnType([CanBeNull] string typeName)
        {
            Check.NullButNotEmpty(typeName, nameof(typeName));

            _property.SqlServer().ColumnType = typeName;

            return this;
        }

        public virtual SqlServerPropertyBuilder DefaultValueSql([CanBeNull] string expression)
        {
            Check.NullButNotEmpty(expression, nameof(expression));

            _property.SqlServer().DefaultValueSql = expression;

            return this;
        }

        public virtual SqlServerPropertyBuilder DefaultValue([CanBeNull] object value)
        {
            _property.SqlServer().DefaultValue = value;

            return this;
        }

        public virtual SqlServerPropertyBuilder ComputedExpression([CanBeNull] string sql)
        {
            Check.NullButNotEmpty(sql, nameof(sql));

            _property.SqlServer().ComputedExpression = sql;

            return this;
        }

        public virtual SqlServerPropertyBuilder UseSequence()
        {
            var sequence = _property.EntityType.Model.SqlServer().GetOrAddSequence();

            _property.SqlServer().IdentityStrategy = SqlServerIdentityStrategy.SequenceHiLo;
            _property.StoreGeneratedPattern = StoreGeneratedPattern.Identity;
            _property.SqlServer().SequenceName = sequence.Name;
            _property.SqlServer().SequenceSchema = sequence.Schema;

            return this;
        }

        public virtual SqlServerPropertyBuilder UseSequence([NotNull] string name, [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var sequence = _property.EntityType.Model.SqlServer().GetOrAddSequence(name, schema);

            _property.SqlServer().IdentityStrategy = SqlServerIdentityStrategy.SequenceHiLo;
            _property.StoreGeneratedPattern = StoreGeneratedPattern.Identity;
            _property.SqlServer().SequenceName = sequence.Name;
            _property.SqlServer().SequenceSchema = sequence.Schema;

            return this;
        }

        public virtual SqlServerPropertyBuilder UseIdentity()
        {
            _property.SqlServer().IdentityStrategy = SqlServerIdentityStrategy.IdentityColumn;
            _property.StoreGeneratedPattern = StoreGeneratedPattern.Identity;
            _property.SqlServer().SequenceName = null;
            _property.SqlServer().SequenceSchema = null;

            return this;
        }
    }
}
