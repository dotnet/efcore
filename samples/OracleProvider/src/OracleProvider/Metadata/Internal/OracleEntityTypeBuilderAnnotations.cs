// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Oracle.Metadata.Internal
{
    public class OracleEntityTypeBuilderAnnotations : RelationalEntityTypeAnnotations
    {
        public OracleEntityTypeBuilderAnnotations(
            [NotNull] InternalEntityTypeBuilder internalBuilder, ConfigurationSource configurationSource)
            : base(new RelationalAnnotationsBuilder(internalBuilder, configurationSource))
        {
        }

        public virtual bool ToTable([CanBeNull] string name)
            => SetTableName(Check.NullButNotEmpty(name, nameof(name)));
    }
}
