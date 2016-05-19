// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Storage
{
    public class DbCommandLogData
    {
        public DbCommandLogData(
            [NotNull] string commandText,
            CommandType commandType,
            int commandTimeout,
            [NotNull] IReadOnlyList<DbParameterLogData> parameters,
            long? elapsedMilliseconds)
        {
            CommandText = commandText;
            CommandType = commandType;
            CommandTimeout = commandTimeout;
            Parameters = parameters;
            ElapsedMilliseconds = elapsedMilliseconds;
        }

        public virtual string CommandText { get; }
        public virtual CommandType CommandType { get; }
        public virtual int CommandTimeout { get; }
        public virtual IReadOnlyList<DbParameterLogData> Parameters { get; }
        public virtual long? ElapsedMilliseconds { get; }
    }
}
