// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

#nullable disable

public abstract class FanTpt
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ulong ULongVersion { get; init; }

    [NotMapped]
    public List<byte> BinaryVersion { get; init; }
}

public class MegaFanTpt : FanTpt
{
    public class MegaFanTptProxy : MegaFanTpt, IF1Proxy
    {
        public bool CreatedCalled { get; set; }
        public bool InitializingCalled { get; set; }
        public bool InitializedCalled { get; set; }
    }

    public string MegaStatus { get; set; }
    public SwagBag Swag { get; set; }
}

public class SuperFanTpt : FanTpt, ISuperFan
{
    public class SuperFanTptProxy : SuperFanTpt, IF1Proxy
    {
        public bool CreatedCalled { get; set; }
        public bool InitializingCalled { get; set; }
        public bool InitializedCalled { get; set; }
    }

    public string SuperStatus { get; set; }
    public SwagBag Swag { get; set; }
}
