// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Infrastructure
{
    public static class AccessorExtensions
    {
        public static TService GetService<TService>([NotNull] this IAccessor<IServiceProvider> accessor)
            => Check.NotNull(accessor, nameof(accessor)).Service.GetRequiredService<TService>();

        public static TService GetService<TService>([NotNull] this IAccessor<TService> accessor)
            => Check.NotNull(accessor, nameof(accessor)).Service;
    }
}
