// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CodeDom.Compiler;
using System.Text;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TextTemplating;
using Microsoft.EntityFrameworkCore.TextTemplating.Internal;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

internal class TextTemplatingModelGenerator : TemplatedModelGenerator
{
    private readonly ITextTemplating _host;
    private readonly IOperationReporter _reporter;

    public TextTemplatingModelGenerator(
        ModelCodeGeneratorDependencies dependencies,
        ITextTemplating textTemplatingService,
        IOperationReporter reporter)
        : base(dependencies)
    {
        _host = textTemplatingService;
        _reporter = reporter;
    }

    public override bool HasTemplates(string projectDir)
        => File.Exists(Path.Combine(projectDir, TemplatesDirectory, "DbContext.t4"));

    public override ScaffoldedModel GenerateModel(IModel model, ModelCodeGenerationOptions options)
    {
        if (options.ContextName == null)
        {
            throw new ArgumentException(
                CoreStrings.ArgumentPropertyNull(nameof(options.ContextName), nameof(options)), nameof(options));
        }

        if (options.ConnectionString == null)
        {
            throw new ArgumentException(
                CoreStrings.ArgumentPropertyNull(nameof(options.ConnectionString), nameof(options)), nameof(options));
        }

        var resultingFiles = new ScaffoldedModel();

        var contextTemplate = Path.Combine(options.ProjectDir!, TemplatesDirectory, "DbContext.t4");

        Check.DebugAssert(_host.Session == null, "Session is not null.");
        _host.Session = _host.CreateSession();
        try
        {
            _host.Session.Add("Model", model);
            _host.Session.Add("Options", options);
            _host.Session.Add("NamespaceHint", options.ContextNamespace ?? options.ModelNamespace);
            _host.Session.Add("ProjectDefaultNamespace", options.RootNamespace);

            var handler = new TextTemplatingCallback();
            var generatedCode = ProcessTemplate(contextTemplate, handler);

            var dbContextFileName = options.ContextName + handler.Extension;
            resultingFiles.ContextFile = new ScaffoldedFile
            {
                Path = options.ContextDir != null
                    ? Path.Combine(options.ContextDir, dbContextFileName)
                    : dbContextFileName,
                Code = generatedCode
            };
        }
        finally
        {
            _host.Session = null;
        }

        var entityTypeTemplate = Path.Combine(options.ProjectDir!, TemplatesDirectory, "EntityType.t4");
        if (File.Exists(entityTypeTemplate))
        {
            foreach (var entityType in model.GetEntityTypes())
            {
                // TODO: Should this be handled inside the template?
                if (CSharpDbContextGenerator.IsManyToManyJoinEntityType(entityType))
                {
                    continue;
                }

                _host.Session = _host.CreateSession();
                try
                {
                    _host.Session.Add("EntityType", entityType);
                    _host.Session.Add("Options", options);
                    _host.Session.Add("NamespaceHint", options.ModelNamespace);
                    _host.Session.Add("ProjectDefaultNamespace", options.RootNamespace);

                    var handler = new TextTemplatingCallback();
                    var generatedCode = ProcessTemplate(entityTypeTemplate, handler);
                    if (string.IsNullOrWhiteSpace(generatedCode))
                    {
                        continue;
                    }

                    var entityTypeFileName = entityType.Name + handler.Extension;
                    resultingFiles.AdditionalFiles.Add(
                        new ScaffoldedFile { Path = entityTypeFileName, Code = generatedCode });
                }
                finally
                {
                    _host.Session = null;
                }
            }
        }

        return resultingFiles;
    }

    private string ProcessTemplate(string inputFile, TextTemplatingCallback handler)
    {
        var output = _host.ProcessTemplate(
            inputFile,
            File.ReadAllText(inputFile),
            handler);

        foreach (CompilerError error in handler.Errors)
        {
            var builder = new StringBuilder();

            if (!string.IsNullOrEmpty(error.FileName))
            {
                builder.Append(error.FileName);

                if (error.Line > 0)
                {
                    builder
                        .Append("(")
                        .Append(error.Line);

                    if (error.Column > 0)
                    {
                        builder
                            .Append(",")
                            .Append(error.Line);
                    }
                    builder.Append(")");
                }

                builder.Append(" : ");
            }

            builder
                .Append(error.IsWarning ? "warning" : "error")
                .Append(" ")
                .Append(error.ErrorNumber)
                .Append(": ")
                .AppendLine(error.ErrorText);

            if (error.IsWarning)
            {
                _reporter.WriteWarning(builder.ToString());
            }
            else
            {
                _reporter.WriteError(builder.ToString());
            }
        }

        if (handler.OutputEncoding != Encoding.UTF8)
        {
            _reporter.WriteWarning(DesignStrings.EncodingIgnored(handler.OutputEncoding.WebName));
        }

        return output;
    }
}
