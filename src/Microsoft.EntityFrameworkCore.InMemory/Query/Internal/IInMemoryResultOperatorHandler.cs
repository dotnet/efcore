// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     An in-memory-specific handler for <see cref="ResultOperatorBase" /> instances.
    /// </summary>
    public interface IInMemoryResultOperatorHandler : IResultOperatorHandler
    {
    }
}
