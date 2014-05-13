// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using Microsoft.Data.Entity.AzureTableStorage.Interfaces;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Wrappers
{
    public class TableResultWrapper : ITableResult
    {
        private readonly TableResult _result;

        public TableResultWrapper(TableResult result)
        {
            _result = result;
        }

        public string ETag
        {
            get { return _result.Etag; }
        }

        public HttpStatusCode HttpStatusCode
        {
            get { return (HttpStatusCode)_result.HttpStatusCode; }
        }

        public object Result
        {
            get { return _result.Result; }
        }
    }
}
