// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using System;

namespace Microsoft.Data.Entity.Query
{
    public static class EF
    {
        public static TProperty Property<TProperty>([NotNull] object entity, [NotNull] string propertyName)
        {
            throw new InvalidOperationException(Strings.PropertyMethodInvoked);
        }
    }
}

