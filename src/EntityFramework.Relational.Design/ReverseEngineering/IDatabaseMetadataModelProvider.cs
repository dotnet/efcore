// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public interface IDatabaseMetadataModelProvider
    {
        IModel GenerateMetadataModel([NotNull] string connectionString);
        DbContextCodeGenerator GetContextModelCodeGenerator(
            [NotNull] ReverseEngineeringGenerator generator,
            [NotNull] DbContextGeneratorModel dbContextGeneratorModel);
        EntityTypeCodeGenerator GetEntityTypeModelCodeGenerator(
            [NotNull] ReverseEngineeringGenerator generator,
            [NotNull] EntityTypeGeneratorModel entityTypeGeneratorModel);
    }
}