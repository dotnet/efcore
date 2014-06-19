// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using Microsoft.Data.Entity.AzureTableStorage.Interfaces;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers
{
    public class TestTableResult : ITableResult
    {
        public string ETag { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
        public object Result { get; set; }

        public static ITableResult OK()
        {
            return WithStatus(HttpStatusCode.OK);
        }

        public static ITableResult BadRequest()
        {
            return WithStatus(HttpStatusCode.BadRequest);
        }

        public static ITableResult WithStatus(HttpStatusCode code)
        {
            return new TestTableResult { HttpStatusCode = code };
        }
    }
}
