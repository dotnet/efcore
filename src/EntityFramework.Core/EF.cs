// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;

namespace Microsoft.Data.Entity
{
    // ReSharper disable once InconsistentNaming
    public static class EF
    {
        public static TProperty Property<TProperty>([NotNull] object entity, [NotNull] string propertyName)
        {
            throw new InvalidOperationException(Strings.PropertyMethodInvoked);
        }
    }
}
