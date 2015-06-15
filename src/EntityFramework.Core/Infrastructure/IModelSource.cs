// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;

namespace Microsoft.Data.Entity.Infrastructure
{
    public interface IModelSource
    {
        IModel GetModel([NotNull] DbContext context, [NotNull] IModelBuilderFactory builder, [NotNull] IModelValidator validator);
    }
}
