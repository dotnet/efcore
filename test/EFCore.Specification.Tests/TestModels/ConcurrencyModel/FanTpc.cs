// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

#nullable disable

public abstract class FanTpc
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ulong ULongVersion { get; init; }

    [NotMapped]
    public List<byte> BinaryVersion { get; init; }
}

public class MegaFanTpc : FanTpc
{
    public class MegaFanTpcProxy : MegaFanTpc, IF1Proxy
    {
        public bool CreatedCalled { get; set; }
        public bool InitializingCalled { get; set; }
        public bool InitializedCalled { get; set; }
    }

    public string MegaStatus { get; set; }
    public SwagBag Swag { get; set; }
}

public class SuperFanTpc : FanTpc, ISuperFan
{
    public class SuperFanTpcProxy : SuperFanTpc, IF1Proxy
    {
        public bool CreatedCalled { get; set; }
        public bool InitializingCalled { get; set; }
        public bool InitializedCalled { get; set; }
    }

    public string SuperStatus { get; set; }
    public SwagBag Swag { get; set; }
}
