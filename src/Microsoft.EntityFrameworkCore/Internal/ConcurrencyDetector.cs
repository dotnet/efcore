// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class ConcurrencyDetector : IConcurrencyDetector
    {
        private bool _isInCriticalSection;

        public virtual void EnterCriticalSection()
        {
            if(_isInCriticalSection)
            {
                throw new NotSupportedException(CoreStrings.ConcurrentMethodInvocation);
            }

            _isInCriticalSection = true;
        }

        public virtual void ExitCriticalSection()
        {
            Debug.Assert(_isInCriticalSection, "Expected to be in a critical section");

            _isInCriticalSection = false;
        }
    }
}
