// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public interface IDatabaseMetadataModelProvider
    {
        IModel GenerateMetadataModel(string connectionString, string filters);
        DbContextCodeGeneratorContext GetContextModelCodeGenerator(ContextTemplateModel contextTemplateModel);
        EntityTypeCodeGeneratorContext GetEntityTypeModelCodeGenerator(
            EntityTypeTemplateModel entityTypeTemplateModel, DbContextCodeGeneratorContext dbContextCodeGeneratorContext);
    }
}