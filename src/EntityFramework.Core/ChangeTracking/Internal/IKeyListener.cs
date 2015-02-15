// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public interface IKeyListener
    {
        void KeyPropertyChanged([NotNull] InternalEntityEntry entry, [NotNull] IProperty property, [CanBeNull] object oldValue, [CanBeNull] object newValue);
    }
}
