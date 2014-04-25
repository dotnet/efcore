// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations
{
    public abstract class ModelCodeGenerator
    {
        public virtual IReadOnlyList<string> GetNamespaces([NotNull] IModel model)
        {
            return GetDefaultNamespaces();
        }

        public virtual IReadOnlyList<string> GetDefaultNamespaces()
        {
            return new[]
                {
                    "Microsoft.Data.Entity.Metadata",
                    "System"
                };
        }

        public abstract void Generate(
            [NotNull] IModel model,
            [NotNull] IndentedStringBuilder stringBuilder);

        public abstract void GenerateModelSnapshotClass(
            [NotNull] string @namespace,
            [NotNull] string className,
            [NotNull] IModel model,
            [NotNull] IndentedStringBuilder stringBuilder);
    }
}
