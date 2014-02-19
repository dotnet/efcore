// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.Data.Relational
{
    public interface IDbCommandExecutor
    {
        Task<T> ExecuteScalarAsync<T>([NotNull] string commandText, [NotNull] params object[] parameters);
    }
}
