// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

public class TestDriver : Driver
{
    public class TestDriverProxy(
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
        int teamId) : TestDriver(loader, id, name, carNumber, championships, races, wins, podiums, poles, fastestLaps, teamId), IF1Proxy
    {
        public bool CreatedCalled { get; set; }
        public bool InitializingCalled { get; set; }
        public bool InitializedCalled { get; set; }
    }

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
        Assert.IsType<TestDriverProxy>(this);
    }
}
