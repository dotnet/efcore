// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStoreTransaction : IDisposable
    {
        public abstract void Commit();

        public abstract void Rollback();

        public abstract void Dispose();
    }
}
