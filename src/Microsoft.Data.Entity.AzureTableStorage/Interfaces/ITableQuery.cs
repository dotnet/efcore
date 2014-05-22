// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.AzureTableStorage.Query;

namespace Microsoft.Data.Entity.AzureTableStorage.Interfaces
{
    public interface ITableQuery
    {
        string Where { get; }
        Type ResultType { get; }
        ITableQuery WithFilter(TableFilter filter);
    }
}
