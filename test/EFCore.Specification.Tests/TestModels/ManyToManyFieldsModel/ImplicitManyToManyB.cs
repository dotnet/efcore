// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel
{
    public class ImplicitManyToManyB
    {
        public int Id;
        public string Name;

        public ICollection<ImplicitManyToManyA> As = new ObservableCollection<ImplicitManyToManyA>();
    }
}
