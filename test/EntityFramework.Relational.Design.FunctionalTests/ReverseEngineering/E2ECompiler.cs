// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal.Templating.Compilation;

namespace Microsoft.Data.Entity.Relational.Design.FunctionalTests.ReverseEngineering
{
    public class E2ECompiler
    {
        public List<string> NamedReferences = new List<string>();

        public List<MetadataReference> References = new List<MetadataReference>();

        public CompilationResult Compile(List<string> fileContents)
        {
            var references = GetReferences();
            var roslynCompilationService = new RoslynCompilationService();
            return roslynCompilationService.Compile(fileContents, references);
        }

        public virtual List<MetadataReference> GetReferences()
        {
            var provider = new MetadataReferencesProvider();
            NamedReferences.ForEach(name => provider.AddReferenceFromName(name));
            References.ForEach(r => provider.Add(r));
            return provider.GetApplicationReferences();
        }
    }
}
