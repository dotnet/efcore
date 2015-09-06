// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNet.Razor;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.Templating.Compilation;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.Templating
{
    public class RazorTemplating : ITemplating
    {
        private readonly ICompilationService _compilationService;
        private readonly MetadataReferencesProvider _metadataReferencesProvider;
        private readonly ModelUtilities _modelUtilities;
        private readonly CSharpUtilities _csharpUtiliies;

        public RazorTemplating(
            [NotNull] ICompilationService compilationService,
            [NotNull] MetadataReferencesProvider metadataReferencesProvider, 
            [NotNull] ModelUtilities modelUtilities, 
            [NotNull] CSharpUtilities csharpUtiliies)
        {
            Check.NotNull(compilationService, nameof(compilationService));
            Check.NotNull(metadataReferencesProvider, nameof(metadataReferencesProvider));
            Check.NotNull(modelUtilities, nameof(modelUtilities));
            Check.NotNull(csharpUtiliies, nameof(csharpUtiliies));

            _compilationService = compilationService;
            _metadataReferencesProvider = metadataReferencesProvider;
            _modelUtilities = modelUtilities;
            _csharpUtiliies = csharpUtiliies;
        }

        public virtual async Task<TemplateResult> RunTemplateAsync(string content,
            dynamic templateModel, IDatabaseMetadataModelProvider provider,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var host = new RazorTemplatingHost(typeof(RazorReverseEngineeringBase));
            var engine = new RazorTemplateEngine(host);

            using (var reader = new StringReader(content))
            {
                var generatorResults = engine.GenerateCode(reader);

                if (!generatorResults.Success)
                {
                    var messages = generatorResults.ParserErrors.Select(
                        e => Strings.ErrorMessageWithLineNumber(e.Location.LineIndex, e.Message));
                    return new TemplateResult
                    {
                        GeneratedText = string.Empty,
                        ProcessingException = new TemplateProcessingException(messages)
                    };
                }

                provider.AddReferencesForTemplates(_metadataReferencesProvider);
                var references = _metadataReferencesProvider.GetApplicationReferences();
                var templateResult = _compilationService.Compile(
                    new List<string> { generatorResults.GeneratedCode }, references);
                if (templateResult.Messages.Any())
                {
                    return new TemplateResult
                    {
                        GeneratedText = string.Empty,
                        ProcessingException = new TemplateProcessingException(templateResult.Messages)
                    };
                }

                var compiledObject = Activator.CreateInstance(templateResult.CompiledType);
                var razorTemplate = compiledObject as RazorReverseEngineeringBase;

                var result = String.Empty;
                if (razorTemplate != null)
                {
                    razorTemplate.Model = templateModel;
                    razorTemplate.ModelUtilities = _modelUtilities;
                    razorTemplate.CSharpUtilities = _csharpUtiliies;
                    //ToDo: If there are errors executing the code, they are missed here.
                    result = await razorTemplate.ExecuteTemplateAsync(cancellationToken);
                }

                return new TemplateResult
                {
                    GeneratedText = result,
                    ProcessingException = null
                };
            }
        }
    }
}
