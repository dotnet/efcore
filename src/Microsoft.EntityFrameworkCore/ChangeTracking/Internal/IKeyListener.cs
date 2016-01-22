// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public interface IKeyListener
    {
        void KeyPropertyChanged(
            [NotNull] InternalEntityEntry entry,
            [NotNull] IProperty property,
            [CanBeNull] object oldValue,
            [CanBeNull] object newValue);
    }
}
