// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNet.Razor;
using Microsoft.CodeAnalysis;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.Templating.Compilation;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.Templating
{
    public class RazorTemplating : ITemplating
    {
        private ICompilationService _compilationService;
        private MetadataReferencesProvider _metadataReferencesProvider;

        public RazorTemplating([NotNull]ICompilationService compilationService,
            [NotNull]MetadataReferencesProvider metadataReferencesProvider)
        {
            Check.NotNull(compilationService, nameof(compilationService));
            Check.NotNull(metadataReferencesProvider, nameof(metadataReferencesProvider));

            _compilationService = compilationService;
            _metadataReferencesProvider = metadataReferencesProvider;
        }

        public virtual async Task<TemplateResult> RunTemplateAsync(string content,
            dynamic templateModel, IDatabaseMetadataModelProvider provider,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            RazorTemplatingHost host = new RazorTemplatingHost(typeof(RazorReverseEngineeringBase));
            RazorTemplateEngine engine = new RazorTemplateEngine(host);

            using (var reader = new StringReader(content))
            {
                var generatorResults = engine.GenerateCode(reader);

                if (!generatorResults.Success)
                {
                    var messages = generatorResults.ParserErrors.Select(e => e.Message);
                    return new TemplateResult()
                    {
                        GeneratedText = string.Empty,
                        ProcessingException = new TemplateProcessingException(messages, generatorResults.GeneratedCode)
                    };
                }

                provider.AddReferencesForTemplates(_metadataReferencesProvider);
                var references = _metadataReferencesProvider.GetApplicationReferences();
                var templateResult = _compilationService.Compile(generatorResults.GeneratedCode, references);
                if (templateResult.Messages.Any())
                {
                    return new TemplateResult()
                    {
                        GeneratedText = string.Empty,
                        ProcessingException = new TemplateProcessingException(templateResult.Messages, generatorResults.GeneratedCode)
                    };
                }

                var compiledObject = Activator.CreateInstance(templateResult.CompiledType);
                var razorTemplate = compiledObject as RazorReverseEngineeringBase;

                string result = String.Empty;
                if (razorTemplate != null)
                {
                    razorTemplate.Model = templateModel;
                    //ToDo: If there are errors executing the code, they are missed here.
                    result = await razorTemplate.ExecuteTemplateAsync();
                }

                return new TemplateResult()
                {
                    GeneratedText = result,
                    ProcessingException = null
                };
            }
        }
    }
}