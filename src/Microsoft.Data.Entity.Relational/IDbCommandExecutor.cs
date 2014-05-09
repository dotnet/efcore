// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational
{
    public interface IDbCommandExecutor
    {
        Task<T> ExecuteScalarAsync<T>(
            [NotNull] string commandText, CancellationToken cancellationToken, [NotNull] params object[] parameters);
    }
}
