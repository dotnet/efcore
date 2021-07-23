﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel
{
    public class ImplicitManyToManyA
    {
        public int Id;
        public string Name;

        public ICollection<ImplicitManyToManyB> Bs = new ObservableCollection<ImplicitManyToManyB>();
    }
}
