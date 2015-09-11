// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public class RelationalCommandBuilder
    {
        private readonly IndentedStringBuilder _stringBuilder = new IndentedStringBuilder();

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

        public virtual RelationalCommand RelationalCommand
            => new RelationalCommand(
                _stringBuilder.ToString(),
                RelationalParameterList.RelationalParameters.ToArray());

        public virtual RelationalParameterList RelationalParameterList { get; } = new RelationalParameterList();

        public virtual IDisposable Indent()
            => _stringBuilder.Indent();

        public virtual int Length => _stringBuilder.Length;

        public virtual RelationalCommandBuilder Clear()
        {
            _stringBuilder.Clear();

            return this;
        }

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
