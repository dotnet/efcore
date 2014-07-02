// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers
{
    public static class TestTableResult
    {
        public static TableResult OK()
        {
            return WithStatus(HttpStatusCode.OK);
        }

        public static TableResult BadRequest()
        {
            return WithStatus(HttpStatusCode.BadRequest);
        }

        public static TableResult WithStatus(HttpStatusCode code)
        {
            return new TableResult { HttpStatusCode = (int)code };
        }
    }
}
