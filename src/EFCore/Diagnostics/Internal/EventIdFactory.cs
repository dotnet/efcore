// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    internal class EventIdFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static EventId Create(int id, [NotNull] string name)
        {
            if (AppContext.TryGetSwitch(CoreEventId.UseLegacyEventIdsSwitch, out var isEnabled)
                && isEnabled)
            {
                if (id >= CoreEventId.ProviderDesignBaseId)
                {
                    id = MassageId(id, CoreEventId.ProviderDesignBaseId);
                }
                else if (id >= CoreEventId.ProviderBaseId)
                {
                    id = MassageId(id, CoreEventId.ProviderBaseId);
                }
                else if (id >= CoreEventId.RelationalBaseId)
                {
                    id = MassageId(id, CoreEventId.RelationalBaseId);
                }
                else if (id >= CoreEventId.CoreBaseId)
                {
                    id = MassageId(id, CoreEventId.CoreBaseId);
                }
            }

            return new EventId(id, name);
        }

        private static int MassageId(int id, int baseId) => (id - baseId) + (baseId * 10);
    }
}
