// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class IndentedStringBuilder
    {
        private const byte IndentSize = 4;

        private readonly string _disconnectedIndent = new string(' ', IndentSize);
        private readonly string _suspendedIndent = "|" + new string(' ', IndentSize - 1);
        private readonly string _connectedIndent = "|" + new string('_', IndentSize - 2) + " ";

        private byte _indent;
        private bool _indentPending = true;

        private readonly StringBuilder _stringBuilder = new StringBuilder();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public IndentedStringBuilder()
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public IndentedStringBuilder([NotNull] IndentedStringBuilder from)
        {
            _indent = from._indent;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int Length => _stringBuilder.Length;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IndentedStringBuilder Append([NotNull] object o)
        {
            DoIndent();

            _stringBuilder.Append(o);

            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IndentedStringBuilder AppendLine()
        {
            AppendLine(string.Empty);

            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IndentedStringBuilder AppendLine([NotNull] object o)
        {
            var value = o.ToString();

            if (value.Length != 0)
            {
                DoIndent();
            }

            _stringBuilder.AppendLine(value);

            _indentPending = true;

            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IndentedStringBuilder AppendLines([NotNull] object o, bool skipFinalNewline = false)
        {
            using (var reader = new StringReader(o.ToString()))
            {
                var first = true;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        AppendLine();
                    }

                    if (line.Length != 0)
                    {
                        Append(line);
                    }
                }
            }

            if (!skipFinalNewline)
            {
                AppendLine();
            }

            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IndentedStringBuilder Clear()
        {
            _stringBuilder.Clear();

            return this;
        }

        private enum NodeConnectionState
        {
            Disconnected = 0,
            Connected = 1,
            Suspended = 2
        }

        private readonly List<NodeConnectionState> _nodeConnectionStates = new List<NodeConnectionState>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IndentedStringBuilder IncrementIndent()
            => IncrementIndent(false);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IndentedStringBuilder IncrementIndent(bool connectNode)
        {
            var state = connectNode ? NodeConnectionState.Connected : NodeConnectionState.Disconnected;
            if (_indent == _nodeConnectionStates.Count)
            {
                _nodeConnectionStates.Add(state);
            }
            else
            {
                _nodeConnectionStates[_indent] = state;
            }

            _indent++;

            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void DisconnectCurrentNode()
        {
            if (_indent > 0
                && _nodeConnectionStates.Count >= _indent)
            {
                _nodeConnectionStates[_indent - 1] = NodeConnectionState.Disconnected;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SuspendCurrentNode()
        {
            if (_indent > 0
                && _nodeConnectionStates.Count >= _indent
                && _nodeConnectionStates[_indent - 1] == NodeConnectionState.Connected)
            {
                _nodeConnectionStates[_indent - 1] = NodeConnectionState.Suspended;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ReconnectCurrentNode()
        {
            if (_indent > 0
                && _nodeConnectionStates.Count >= _indent
                && _nodeConnectionStates[_indent - 1] == NodeConnectionState.Suspended)
            {
                _nodeConnectionStates[_indent - 1] = NodeConnectionState.Connected;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IndentedStringBuilder DecrementIndent()
        {
            if (_indent > 0)
            {
                _indent--;

                _nodeConnectionStates.RemoveAt(_indent);
            }

            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IDisposable Indent() => new Indenter(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string ToString() => _stringBuilder.ToString();

        private void DoIndent()
        {
            if (_indentPending && (_indent > 0))
            {
                var indentString = string.Empty;
                for (var i = 0; i < _nodeConnectionStates.Count; i++)
                {
                    if (_nodeConnectionStates[i] == NodeConnectionState.Disconnected)
                    {
                        indentString += _disconnectedIndent;
                    }
                    else if (i == _nodeConnectionStates.Count - 1
                             && _nodeConnectionStates[i] == NodeConnectionState.Connected)
                    {
                        indentString += _connectedIndent;
                    }
                    else
                    {
                        indentString += _suspendedIndent;
                    }
                }

                _stringBuilder.Append(indentString);
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

            public void Dispose() => _stringBuilder.DecrementIndent();
        }
    }
}
