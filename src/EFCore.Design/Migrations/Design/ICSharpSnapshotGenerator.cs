// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Migrations.Design
{
    public interface ICSharpSnapshotGenerator
    {
        void Generate(
            [NotNull] string builderName,
            [NotNull] IModel model,
            [NotNull] IndentedStringBuilder stringBuilder);
    }
}
