// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public interface IPrincipalKeyValueFactorySource
    {
        IPrincipalKeyValueFactory<TKey> GetPrincipalKeyValueFactory<TKey>();
    }
}
