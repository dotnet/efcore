// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Entity.Commands.Utilities
{
    public class ColorScope : IDisposable
    {
        private readonly ConsoleColor _originalColor;

        public ColorScope(ConsoleColor color)
        {
            _originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
        }

        public virtual void Dispose()
        {
            Console.ForegroundColor = _originalColor;
        }
    }
}
