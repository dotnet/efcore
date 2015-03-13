// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Framework.ConfigurationModel;

namespace Microsoft.Data.Entity.Internal
{
    public interface IDbContextOptionsParser
    {
        IReadOnlyDictionary<string, string> ReadRawOptions<TContext>(
            [CanBeNull] IConfiguration configuration)
            where TContext : DbContext;
    }
}
