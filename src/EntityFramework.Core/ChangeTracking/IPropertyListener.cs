// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public interface IPropertyListener
    {
        void SidecarPropertyChanged([NotNull] StateEntry entry, [NotNull] IPropertyBase property);
        void SidecarPropertyChanging([NotNull] StateEntry entry, [NotNull] IPropertyBase property);
        void PropertyChanged([NotNull] StateEntry entry, [NotNull] IPropertyBase property);
        void PropertyChanging([NotNull] StateEntry entry, [NotNull] IPropertyBase property);
    }
}
