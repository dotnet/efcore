// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public interface IRelationshipListener
    {
        void ForeignKeyPropertyChanged([NotNull] StateEntry entry, [NotNull] IProperty property, [CanBeNull] object oldValue, [CanBeNull] object newValue);
        void NavigationReferenceChanged([NotNull] StateEntry entry, [NotNull] INavigation navigation, [CanBeNull] object oldValue, [CanBeNull] object newValue);
        void NavigationCollectionChanged([NotNull] StateEntry entry, [NotNull] INavigation navigation, [NotNull] ISet<object> added, [NotNull] ISet<object> removed);
        void PrincipalKeyPropertyChanged([NotNull] StateEntry entry, [NotNull] IProperty property, [CanBeNull] object oldValue, [CanBeNull] object newValue);
    }
}
