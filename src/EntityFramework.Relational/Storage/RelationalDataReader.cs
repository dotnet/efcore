// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public class RelationalDataReader : IDisposable
    {
        private readonly DbCommand _command;
        private readonly DbDataReader _reader;

        private bool _disposed;

        public RelationalDataReader(
            [NotNull] DbCommand command,
            [NotNull] DbDataReader reader)
        {
            Check.NotNull(command, nameof(command));
            Check.NotNull(reader, nameof(reader));

            _command = command;
            _reader = reader;
        }

        public virtual DbDataReader DbDataReader => _reader;

        public virtual void Dispose()
        {
            if (!_disposed)
            {
                _reader.Dispose();
                _command.Dispose();

                _disposed = true;
            }
        }
    }
}
