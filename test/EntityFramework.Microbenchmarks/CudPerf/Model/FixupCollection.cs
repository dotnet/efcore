// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if K10
using Cud.Utilities;
#endif
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EntityFramework.Microbenchmarks.CudPerf.Model
{
    // An System.Collections.ObjectModel.ObservableCollection that raises
    // individual item removal notifications on clear and prevents adding duplicates.
    public class FixupCollection<T> : ObservableCollection<T>
    {
        protected override void ClearItems()
        {
            var items = new List<T>(this);
            foreach (var item in items)
            {
                Remove(item);
            }
        }

        protected override void InsertItem(int index, T item)
        {
            if (!Contains(item))
            {
                base.InsertItem(index, item);
            }
        }
    }
}
