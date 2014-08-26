// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.AspNet.Diagnostics.Entity.Views
{
    public class DatabaseErrorPageModel
    {
        public virtual DatabaseErrorPageOptions Options { get; set; }
        public virtual DataStoreException Exception { get; set; }
        public virtual bool DatabaseExists { get; set; }
        public virtual bool PendingModelChanges { get; set; }
        public virtual IEnumerable<string> PendingMigrations { get; set; }
    }
}
