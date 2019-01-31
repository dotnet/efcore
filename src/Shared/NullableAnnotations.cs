// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Parameter,
                   AllowMultiple = false)]
    public class NotNullWhenTrueAttribute : Attribute
    {
        public NotNullWhenTrueAttribute() { }
    }
}
