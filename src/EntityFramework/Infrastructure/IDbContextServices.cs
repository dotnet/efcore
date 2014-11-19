// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Entity.Infrastructure
{
    public interface IDbContextServices
    {
        IServiceProvider ScopedServiceProvider { get; }
    }
}
