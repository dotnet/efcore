// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Storage
{
    public class RelationalCommandBuilder : IRelationalCommandBuilder
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IRelationalTypeMapper _typeMapper;

        public RelationalCommandBuilder(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IRelationalTypeMapper typeMapper)
        {
            Check.NotNull(loggerFactory, nameof(loggerFactory));
            Check.NotNull(typeMapper, nameof(typeMapper));

            _loggerFactory = loggerFactory;
            _typeMapper = typeMapper;
        }

        protected readonly IndentedStringBuilder StringBuilder = new IndentedStringBuilder();

        public virtual IRelationalCommandBuilder AppendLine()
        {
            StringBuilder.AppendLine();

            return this;
        }

        public virtual IRelationalCommandBuilder Append([NotNull] object o)
        {
            Check.NotNull(o, nameof(o));

            StringBuilder.Append(o);

            return this;
        }

        public virtual IRelationalCommandBuilder AppendLine([NotNull] object o)
        {
            Check.NotNull(o, nameof(o));

            StringBuilder.AppendLine(o);

            return this;
        }

        public virtual IRelationalCommandBuilder AppendLines([NotNull] object o)
        {
            Check.NotNull(o, nameof(o));

            StringBuilder.AppendLines(o);

            return this;
        }

        public virtual IRelationalCommand BuildRelationalCommand()
            => new RelationalCommand(
                _loggerFactory,
                _typeMapper,
                StringBuilder.ToString(),
                RelationalParameterList.RelationalParameters);

        public virtual RelationalParameterList RelationalParameterList { get; } = new RelationalParameterList();

        public virtual IDisposable Indent()
            => StringBuilder.Indent();

        public virtual int Length => StringBuilder.Length;

        public virtual IRelationalCommandBuilder Clear()
        {
            StringBuilder.Clear();

            return this;
        }

        public virtual IRelationalCommandBuilder IncrementIndent()
        {
            StringBuilder.IncrementIndent();

            return this;
        }

        public virtual IRelationalCommandBuilder DecrementIndent()
        {
            StringBuilder.DecrementIndent();

            return this;
        }

        public override string ToString() => StringBuilder.ToString();
    }
}
