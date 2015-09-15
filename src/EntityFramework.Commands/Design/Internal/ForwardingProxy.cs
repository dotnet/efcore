// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !DNXCORE50

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Design.Internal
{
    public static class ForwardingProxy
    {
        public static T Unwrap<T>([NotNull] object target)
            where T : class
            => target as T ?? new ForwardingProxy<T>(target).GetTransparentProxy();
    }
}

#endif
