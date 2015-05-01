// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Utilities
{
    public class IndentedStringBuilder
    {
        private const byte IndentSize = 4;

        private byte _indent;
        private bool _indentPending = true;

        private readonly StringBuilder _stringBuilder = new StringBuilder();

        public IndentedStringBuilder()
        {
        }

        public IndentedStringBuilder([NotNull] IndentedStringBuilder from)
        {
            Check.NotNull(from, nameof(from));

            _indent = from._indent;
        }

        public virtual IndentedStringBuilder Append([NotNull] object o)
        {
            Check.NotNull(o, nameof(o));

            DoIndent();

            _stringBuilder.Append(o);

            return this;
        }

        public virtual IndentedStringBuilder AppendLine()
        {
            AppendLine(string.Empty);

            return this;
        }

        public virtual IndentedStringBuilder AppendLine([NotNull] object o)
        {
            Check.NotNull(o, nameof(o));

            DoIndent();

            _stringBuilder.AppendLine(o.ToString());

            _indentPending = true;

            return this;
        }

        public virtual IndentedStringBuilder AppendLines([NotNull] object o)
        {
            Check.NotNull(o, nameof(o));

            using (var reader = new StringReader(o.ToString()))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    AppendLine(line);
                }
            }

            return this;
        }

        public virtual IndentedStringBuilder IncrementIndent()
        {
            _indent++;
            return this;
        }

        public virtual IndentedStringBuilder DecrementIndent()
        {
            if (_indent > 0)
            {
                _indent--;
            }
            return this;
        }

        public virtual IDisposable Indent() => new Indenter(this);

        public override string ToString() => _stringBuilder.ToString();

        private void DoIndent()
        {
            if (_indentPending && _indent > 0)
            {
                _stringBuilder.Append(new string(' ', _indent * IndentSize));
            }

            _indentPending = false;
        }

        private sealed class Indenter : IDisposable
        {
            private readonly IndentedStringBuilder _stringBuilder;

            public Indenter(IndentedStringBuilder stringBuilder)
            {
                _stringBuilder = stringBuilder;

                _stringBuilder.IncrementIndent();
            }

            public void Dispose()
            {
                _stringBuilder.DecrementIndent();
            }
        }
    }
}
