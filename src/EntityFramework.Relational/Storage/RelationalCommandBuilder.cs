// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public class RelationalCommandBuilder
    {
        private readonly IRelationalTypeMapper _typeMapper;
        private readonly IndentedStringBuilder _stringBuilder = new IndentedStringBuilder();
        private readonly List<RelationalParameter> _parameters = new List<RelationalParameter>();

        public RelationalCommandBuilder([NotNull] IRelationalTypeMapper typeMapper)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));

            _typeMapper = typeMapper;
        }

        public virtual RelationalCommandBuilder AppendLine()
        {
            _stringBuilder.AppendLine();

            return this;
        }

        public virtual RelationalCommandBuilder Append([NotNull] object o)
        {
            Check.NotNull(o, nameof(o));

            _stringBuilder.Append(o);

            return this;
        }

        public virtual RelationalCommandBuilder AppendLine([NotNull] object o)
        {
            Check.NotNull(o, nameof(o));

            _stringBuilder.AppendLine(o);

            return this;
        }

        public virtual RelationalCommandBuilder AppendLines([NotNull] object o)
        {
            Check.NotNull(o, nameof(o));

            _stringBuilder.AppendLines(o);

            return this;
        }

        public virtual RelationalCommandBuilder AddParameter(
            [NotNull] string name,
            [CanBeNull] object value)
        {
            Check.NotEmpty(name, nameof(name));

            _parameters.Add(
                new RelationalParameter(
                    name,
                    value,
                    _typeMapper.GetMapping(value),
                    value?.GetType().IsNullableType() ?? null));

            return this;
        }

        public virtual RelationalCommandBuilder AddParameter(
            [NotNull] string name,
            [CanBeNull] object value,
            [NotNull] Type type)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(type, nameof(type));

            bool? isNullable = null;

            if (type.IsNullableType())
            {
                isNullable = true;
                type = type.UnwrapNullableType();
            }

            _parameters.Add(
                new RelationalParameter(
                    name,
                    value,
                    _typeMapper.GetMapping(type),
                    isNullable));

            return this;
        }

        public virtual RelationalCommandBuilder AddParameter(
            [NotNull] string name,
            [CanBeNull] object value,
            [NotNull] IProperty property)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(property, nameof(property));

            _parameters.Add(
                new RelationalParameter(
                    name,
                    value,
                    _typeMapper.GetMapping(property),
                    property.IsNullable));

            return this;
        }

        public virtual RelationalCommand BuildRelationalCommand()
                => new RelationalCommand(
                    _stringBuilder.ToString(),
                    _parameters);

        public virtual IDisposable Indent()
            => _stringBuilder.Indent();

        public virtual int Length => _stringBuilder.Length;

        public virtual RelationalCommandBuilder IncrementIndent()
        {
            _stringBuilder.IncrementIndent();

            return this;
        }

        public virtual RelationalCommandBuilder DecrementIndent()
        {
            _stringBuilder.DecrementIndent();

            return this;
        }

        public override string ToString() => _stringBuilder.ToString();
    }
}
