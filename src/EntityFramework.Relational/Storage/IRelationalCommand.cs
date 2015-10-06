// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Storage
{
    public interface IRelationalCommand
    {
        string CommandText { get; }

        IReadOnlyList<RelationalParameter> Parameters { get; }

        DbCommand CreateCommand([NotNull] IRelationalConnection connection);
    }
}
