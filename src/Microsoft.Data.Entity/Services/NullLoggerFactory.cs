// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using Microsoft.AspNet.Logging;

namespace Microsoft.Data.Entity.Services
{
    public sealed class NullLoggerFactory : ILoggerFactory
    {
        public ILogger Create(string _)
        {
            return NullLogger.Instance;
        }
    }

    public sealed class NullLogger : ILogger
    {
        public static readonly ILogger Instance = new NullLogger();

        private NullLogger()
        {
        }

        public bool WriteCore(
            TraceType eventType, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            return false;
        }
    }
}
