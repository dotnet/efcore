// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public static class ModelExtensions
    {
        public static IEntityType FindEntityType([NotNull] this IModel model, [NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            return model.FindEntityType(type.DisplayName());
        }
    }
}
