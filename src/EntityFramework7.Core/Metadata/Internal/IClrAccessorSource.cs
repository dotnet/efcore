// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public interface IClrAccessorSource<out TAccessor>
        where TAccessor : class
    {
        TAccessor GetAccessor([NotNull] IPropertyBase property);
        TAccessor GetAccessor([NotNull] Type declaringType, [NotNull] string propertyName);
    }
}
