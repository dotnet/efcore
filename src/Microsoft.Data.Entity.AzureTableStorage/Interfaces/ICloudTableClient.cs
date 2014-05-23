// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Data.Entity.AzureTableStorage.Interfaces
{
    public interface ICloudTableClient
    {
        IEnumerable<ICloudTable> ListTables();
        ICloudTable GetTableReference(string tableName);
    }
}
