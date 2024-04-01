// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

#nullable disable

public class Sponsor
{
    public class SponsorDoubleProxy : SponsorProxy
    {
        public SponsorDoubleProxy(SponsorProxy copyFrom)
        {
            Id = copyFrom.Id;
            Name = copyFrom.Name;
            CreatedCalled = copyFrom.CreatedCalled;
            InitializingCalled = copyFrom.InitializingCalled;
            InitializedCalled = copyFrom.InitializedCalled;
        }
    }

    public class SponsorProxy : Sponsor, IF1Proxy
    {
        public bool CreatedCalled { get; set; }
        public bool InitializingCalled { get; set; }
        public bool InitializedCalled { get; set; }
    }

    public static readonly string ClientTokenPropertyName = "ClientToken";

    private readonly ObservableCollection<Team> _teams = [];

    public int Id { get; set; }
    public string Name { get; set; }

    public virtual ICollection<Team> Teams
        => _teams;
}
