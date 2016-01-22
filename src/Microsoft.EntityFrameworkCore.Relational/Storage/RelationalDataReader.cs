// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class RelationalDataReader : IDisposable
    {
        private readonly IRelationalConnection _connection;
        private readonly DbCommand _command;
        private readonly DbDataReader _reader;

        private bool _disposed;

        public RelationalDataReader(
            [CanBeNull] IRelationalConnection connection,
            [NotNull] DbCommand command,
            [NotNull] DbDataReader reader)
        {
            Check.NotNull(command, nameof(command));
            Check.NotNull(reader, nameof(reader));

            _connection = connection;
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
                _connection?.Close();

                _disposed = true;
            }
        }
    }
}
