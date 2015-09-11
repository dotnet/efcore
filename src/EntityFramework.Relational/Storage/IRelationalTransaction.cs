// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.Storage
{
    public interface IRelationalTransaction : IDisposable, IAccessor<DbTransaction>
    {
        IRelationalConnection Connection { get; }
        void Commit();
        void Rollback();
    }
}
