// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CodeDom.Compiler;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CSharpModelGenerator : ModelCodeGenerator
{
    private readonly IOperationReporter _reporter;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CSharpModelGenerator(
        ModelCodeGeneratorDependencies dependencies,
        IOperationReporter reporter,
        IServiceProvider serviceProvider)
        : base(dependencies)
    {
        _reporter = reporter;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string Language
        => "C#";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override ScaffoldedModel GenerateModel(
        IModel model,
        ModelCodeGenerationOptions options)
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

        var host = new TextTemplatingEngineHost(_serviceProvider);
        var contextTemplate = new CSharpDbContextGenerator { Host = host, Session = host.CreateSession() };
        contextTemplate.Session.Add("Model", model);
        contextTemplate.Session.Add("Options", options);
        contextTemplate.Session.Add("NamespaceHint", options.ContextNamespace ?? options.ModelNamespace);
        contextTemplate.Session.Add("ProjectDefaultNamespace", options.RootNamespace);
        contextTemplate.Initialize();

        var generatedCode = ProcessTemplate(contextTemplate);

        // output DbContext .cs file
        var dbContextFileName = options.ContextName + host.Extension;
        var resultingFiles = new ScaffoldedModel
        {
            ContextFile = new ScaffoldedFile
            {
                Path = options.ContextDir != null
                    ? Path.Combine(options.ContextDir, dbContextFileName)
                    : dbContextFileName,
                Code = generatedCode
            }
        };

        foreach (var entityType in model.GetEntityTypes())
        {
            host.Initialize();
            var entityTypeTemplate = new CSharpEntityTypeGenerator { Host = host, Session = host.CreateSession() };
            entityTypeTemplate.Session.Add("EntityType", entityType);
            entityTypeTemplate.Session.Add("Options", options);
            entityTypeTemplate.Session.Add("NamespaceHint", options.ModelNamespace);
            entityTypeTemplate.Session.Add("ProjectDefaultNamespace", options.RootNamespace);
            entityTypeTemplate.Initialize();

            generatedCode = ProcessTemplate(entityTypeTemplate);
            if (string.IsNullOrWhiteSpace(generatedCode))
            {
                continue;
            }

            // output EntityType poco .cs file
            var entityTypeFileName = entityType.Name + host.Extension;
            resultingFiles.AdditionalFiles.Add(
                new ScaffoldedFile { Path = entityTypeFileName, Code = generatedCode });
        }

        return resultingFiles;
    }

    private string ProcessTemplate(ITextTransformation transformation)
    {
        var output = transformation.TransformText();

        foreach (CompilerError error in transformation.Errors)
        {
            _reporter.Write(error);
        }

        if (transformation.Errors.HasErrors)
        {
            throw new OperationException(DesignStrings.ErrorGeneratingOutput(transformation.GetType().Name));
        }

        return output;
    }
}
