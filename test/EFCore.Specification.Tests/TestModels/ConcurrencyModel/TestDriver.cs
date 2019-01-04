// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel
{
    public class TestDriver : Driver
    {
        public TestDriver()
        {
        }

        private TestDriver(
            ILazyLoader loader,
            int id,
            string name,
            int? carNumber,
            int championships,
            int races,
            int wins,
            int podiums,
            int poles,
            int fastestLaps,
            int teamId)
            : base(loader, id, name, carNumber, championships, races, wins, podiums, poles, fastestLaps, teamId)
        {
        }
    }
}
