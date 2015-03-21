// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    public static class ModelExtensions
    {
        public static IEnumerable<INavigation> GetNavigations(
            [NotNull] this IModel model, [NotNull] IForeignKey foreignKey)
        {
            // TODO: Perf: consider not needing to do a full scan here
            return model.EntityTypes.SelectMany(e => e.GetNavigations()).Where(n => n.ForeignKey == foreignKey);
        }
    }
}
