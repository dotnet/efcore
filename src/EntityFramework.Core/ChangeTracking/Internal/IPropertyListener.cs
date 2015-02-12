// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public interface IPropertyListener
    {
        void SidecarPropertyChanged([NotNull] InternalEntityEntry entry, [NotNull] IPropertyBase property);
        void SidecarPropertyChanging([NotNull] InternalEntityEntry entry, [NotNull] IPropertyBase property);
        void PropertyChanged([NotNull] InternalEntityEntry entry, [NotNull] IPropertyBase property);
        void PropertyChanging([NotNull] InternalEntityEntry entry, [NotNull] IPropertyBase property);
    }
}
