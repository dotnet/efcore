// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.Templating.Compilation;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public interface IDatabaseMetadataModelProvider
    {
        IModel GenerateMetadataModel([NotNull] string connectionString);
        string DbContextTemplate { get; }
        string EntityTypeTemplate { get; }
        void AddReferencesForTemplates([NotNull] MetadataReferencesProvider metadataReferencesProvider);
    }
}
