// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Storage
{
    public interface IRelationalCommandBuilder
    {
        IRelationalCommandBuilder AppendLine();

        IRelationalCommandBuilder Append([NotNull] object o);

        IRelationalCommandBuilder AppendLine([NotNull] object o);

        IRelationalCommandBuilder AppendLines([NotNull] object o);

        IDisposable Indent();

        IRelationalCommandBuilder IncrementIndent();

        IRelationalCommandBuilder DecrementIndent();

        IRelationalCommandBuilder Clear();

        int Length { get; }

        IRelationalCommand BuildRelationalCommand();

        RelationalParameterList RelationalParameterList { get; }
    }
}
