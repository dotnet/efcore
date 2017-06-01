// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Design
{
    public interface ICSharpHelper
    {
        string Identifier([NotNull] string name, [CanBeNull] ICollection<string> scope = null);
        string Lambda([NotNull] IReadOnlyList<string> properties);
        string Lambda([NotNull] string property, [NotNull] string variable);
        string Literal([NotNull] object[,] values);
        string Literal<T>([NotNull] T? value) where T : struct;
        string Literal([NotNull] byte[] values);
        string Literal(bool value);
        string Literal(byte value);
        string Literal(char value);
        string Literal(DateTime value);
        string Literal(DateTimeOffset value);
        string Literal(decimal value);
        string Literal(double value);
        string Literal([NotNull] Enum value);
        string Literal(float value);
        string Literal(Guid value);
        string Literal(int value);
        string Literal<T>([NotNull] IReadOnlyList<T> values);
        string Literal([NotNull] IReadOnlyList<object> values);
        string Literal(long value);
        string Literal(sbyte value);
        string Literal(short value);
        string Literal([NotNull] string value);
        string Literal(TimeSpan value);
        string Literal(uint value);
        string Literal(ulong value);
        string Literal(ushort value);
        string Literal([NotNull] IReadOnlyList<object> values, bool vertical);
        string Namespace([NotNull] params string[] name);
        string Reference([NotNull] Type type);
        string UnknownLiteral([CanBeNull] object value);
    }
}
