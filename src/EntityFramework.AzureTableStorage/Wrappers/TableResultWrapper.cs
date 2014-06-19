// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Interfaces;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Wrappers
{
    public class TableResultWrapper : ITableResult
    {
        private readonly TableResult _result;

        public TableResultWrapper([NotNull] TableResult result)
        {
            Check.NotNull(result, "result");
            _result = result;
        }

        public virtual string ETag
        {
            get { return _result.Etag; }
        }

        public virtual HttpStatusCode HttpStatusCode
        {
            get { return (HttpStatusCode)_result.HttpStatusCode; }
        }

        public virtual object Result
        {
            get { return _result.Result; }
        }
    }
}
