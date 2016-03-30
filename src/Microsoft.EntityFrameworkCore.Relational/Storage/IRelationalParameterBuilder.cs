// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public interface IRelationalParameterBuilder
    {
        IReadOnlyList<IRelationalParameter> Parameters { get; }

        void AddParameter(
            [NotNull] string invariantName,
            [NotNull] string name);

        void AddParameter(
            [NotNull] string invariantName,
            [NotNull] string name,
            [NotNull] Type type);

        void AddParameter(
            [NotNull] string invariantName,
            [NotNull] string name,
            [NotNull] IProperty property);

        void AddCompositeParameter(
            [NotNull] string invariantName,
            [NotNull] Action<IRelationalParameterBuilder> buildAction);
    }
}
