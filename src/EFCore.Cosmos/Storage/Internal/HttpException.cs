// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal
{
    public class HttpException : Exception
    {
        public HttpException(HttpResponseMessage response)
            : base(response.StatusCode.ToString())
        {
            // An error occurred while sending the request.
            Response = response;
        }

        public HttpResponseMessage Response { get; }
    }
}
