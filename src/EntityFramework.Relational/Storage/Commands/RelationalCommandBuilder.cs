// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage.Commands
{
    public class RelationalCommandBuilder
    {
        private readonly IndentedStringBuilder _stringBuilder = new IndentedStringBuilder();
        private readonly RelationalParameterList _parameterList = new RelationalParameterList();

        public virtual RelationalCommandBuilder AppendLine()
            => AppendLine(string.Empty);

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
                _parameterList.RelationalParameters.ToArray());

        public virtual RelationalParameterList RelationalParameterList
            => _parameterList;

        public virtual IDisposable Indent()
            => _stringBuilder.Indent();
    }
}
