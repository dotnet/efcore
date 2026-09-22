// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if !NET472
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
