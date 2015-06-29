// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Storage
{
    public interface IRelationalValueBufferFactory
    {
        ValueBuffer Create([NotNull] DbDataReader dataReader);
    }
}
