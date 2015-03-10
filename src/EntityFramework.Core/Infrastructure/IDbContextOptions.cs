// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Infrastructure
{
    public interface IDbContextOptions
    {
        IReadOnlyDictionary<string, string> RawOptions { get; }

        IEnumerable<IDbContextOptionsExtension> Extensions { get; }

        TExtension FindExtension<TExtension>() where TExtension : class, IDbContextOptionsExtension;

        TValue FindRawOption<TValue>([NotNull] string key);
    }
}
