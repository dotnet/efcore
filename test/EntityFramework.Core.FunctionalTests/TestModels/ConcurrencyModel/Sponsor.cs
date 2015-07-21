// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Data.Entity.FunctionalTests.TestModels.ConcurrencyModel
{
    public class Sponsor
    {
        private readonly ObservableCollection<Team> _teams = new ObservableCollection<Team>();

        public byte[] Version { get; set; }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Team> Teams
        {
            get { return _teams; }
        }
    }
}
