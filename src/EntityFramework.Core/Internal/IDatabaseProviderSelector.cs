// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Internal
{
    public interface IDatabaseProviderSelector
    {
        IDatabaseProviderServices SelectServices(ServiceProviderSource providerSource);
    }
}
