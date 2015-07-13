// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;

namespace Microsoft.Data.Entity.Storage
{
    public interface IRelationalTransaction : IDisposable
    {
        IRelationalConnection Connection { get; }
        DbTransaction DbTransaction { get; }
        void Commit();
        void Rollback();
    }
}
