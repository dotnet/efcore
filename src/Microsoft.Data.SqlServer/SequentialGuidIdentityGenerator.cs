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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Identity;

namespace Microsoft.Data.SqlServer
{
    public class SequentialGuidIdentityGenerator : IIdentityGenerator<Guid>
    {
        private static long _counter;

        static SequentialGuidIdentityGenerator()
        {
            _counter = DateTime.UtcNow.Ticks;
        }

        public Task<Guid> NextAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var guidBytes = Guid.NewGuid().ToByteArray();
            var counterBytes = BitConverter.GetBytes(Interlocked.Increment(ref _counter));

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(counterBytes);
            }

            guidBytes[08] = counterBytes[1];
            guidBytes[09] = counterBytes[0];
            guidBytes[10] = counterBytes[7];
            guidBytes[11] = counterBytes[6];
            guidBytes[12] = counterBytes[5];
            guidBytes[13] = counterBytes[4];
            guidBytes[14] = counterBytes[3];
            guidBytes[15] = counterBytes[2];

            return Task.FromResult(new Guid(guidBytes));
        }

        async Task<object> IIdentityGenerator.NextAsync(CancellationToken cancellationToken)
        {
            return await NextAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
