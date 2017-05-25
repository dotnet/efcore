// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class ForwardingProxy
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static T Unwrap<T>([NotNull] object target)
            where T : class
        {
#if NET461
            return target as T ?? new ForwardingProxy<T>(target).GetTransparentProxy();
#elif NETSTANDARD2_0
            return (T)target;
#else
#error target frameworks need to be updated.
#endif
        }
    }
}
