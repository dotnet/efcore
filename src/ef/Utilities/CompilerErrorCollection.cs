// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !NET461

namespace System.CodeDom.Compiler
{
    internal class CompilerErrorCollection
    {
        public bool HasErrors
            => false;

        public void Add(CompilerError error)
        {
        }
    }
}

#endif
