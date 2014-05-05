// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
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

        public virtual IndentedStringBuilder Append([NotNull] object o)
        {
            Check.NotNull(o, "o");

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
            Check.NotNull(o, "o");

            DoIndent();

            _stringBuilder.AppendLine(o.ToString());

            _indentPending = true;

            return this;
        }

        public virtual IDisposable Indent()
        {
            return new Indenter(this);
        }

        public override string ToString()
        {
            return _stringBuilder.ToString();
        }

        private void DoIndent()
        {
            if (_indentPending && _indent > 0)
            {
                _stringBuilder.Append(new string(' ', _indent * IndentSize));
            }

            _indentPending = false;
        }

        private class Indenter : IDisposable
        {
            private readonly IndentedStringBuilder _stringBuilder;

            public Indenter(IndentedStringBuilder stringBuilder)
            {
                _stringBuilder = stringBuilder;

                _stringBuilder._indent++;
            }

            public void Dispose()
            {
                _stringBuilder._indent--;
            }
        }
    }
}
