// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CodeDom.Compiler;
using System.Text;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Engine = Mono.TextTemplating.TemplatingEngine;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class TextTemplatingModelGenerator : TemplatedModelGenerator
{
    private const string DbContextTemplate = "DbContext.t4";
    private const string EntityTypeTemplate = "EntityType.t4";
    private const string EntityTypeConfigurationTemplate = "EntityTypeConfiguration.t4";

    private readonly IOperationReporter _reporter;
    private readonly IServiceProvider _serviceProvider;
    private Engine? _engine;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TextTemplatingModelGenerator(
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
    protected virtual Engine Engine
        => _engine ??= new Engine();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool HasTemplates(string projectDir)
    {
        var hasContextTemplate = File.Exists(Path.Combine(projectDir, TemplatesDirectory, DbContextTemplate));
        var hasEntityTypeTemplate = File.Exists(Path.Combine(projectDir, TemplatesDirectory, EntityTypeTemplate));
        var hasConfigurationTemplate = File.Exists(Path.Combine(projectDir, TemplatesDirectory, EntityTypeConfigurationTemplate));

        if (hasConfigurationTemplate && !hasContextTemplate)
        {
            throw new OperationException(DesignStrings.NoContextTemplateButConfiguration);
        }

        return hasContextTemplate || hasEntityTypeTemplate || hasConfigurationTemplate;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
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

        var host = new TextTemplatingEngineHost(_serviceProvider)
        {
            Session =
            {
                { "Model", model },
                { "Options", options },
                { "NamespaceHint", options.ContextNamespace ?? options.ModelNamespace },
                { "ProjectDefaultNamespace", options.RootNamespace }
            }
        };
        var contextTemplate = Path.Combine(options.ProjectDir!, TemplatesDirectory, DbContextTemplate);

        string generatedCode;
        if (File.Exists(contextTemplate))
        {
            host.TemplateFile = contextTemplate;

            generatedCode = ProcessTemplate(contextTemplate, host);
        }
        else
        {
            // TODO: Use default generator when C#
            throw new OperationException(DesignStrings.NoContextTemplate);
        }

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

        var entityTypeTemplate = Path.Combine(options.ProjectDir!, TemplatesDirectory, EntityTypeTemplate);
        if (File.Exists(entityTypeTemplate))
        {
            host.TemplateFile = entityTypeTemplate;

            foreach (var entityType in model.GetEntityTypes())
            {
                host.Initialize();
                host.Session.Add("EntityType", entityType);
                host.Session.Add("Options", options);
                host.Session.Add("NamespaceHint", options.ModelNamespace);
                host.Session.Add("ProjectDefaultNamespace", options.RootNamespace);

                generatedCode = ProcessTemplate(entityTypeTemplate, host);
                if (string.IsNullOrWhiteSpace(generatedCode))
                {
                    continue;
                }

                var entityTypeFileName = entityType.Name + host.Extension;
                resultingFiles.AdditionalFiles.Add(
                    new ScaffoldedFile { Path = entityTypeFileName, Code = generatedCode });
            }
        }

        var configurationTemplate = Path.Combine(options.ProjectDir!, TemplatesDirectory, EntityTypeConfigurationTemplate);
        if (File.Exists(configurationTemplate))
        {
            host.TemplateFile = configurationTemplate;

            foreach (var entityType in model.GetEntityTypes())
            {
                host.Initialize();
                host.Session.Add("EntityType", entityType);
                host.Session.Add("Options", options);
                host.Session.Add("NamespaceHint", options.ContextNamespace ?? options.ModelNamespace);
                host.Session.Add("ProjectDefaultNamespace", options.RootNamespace);

                generatedCode = ProcessTemplate(configurationTemplate, host);
                if (string.IsNullOrWhiteSpace(generatedCode))
                {
                    continue;
                }

                var configurationFileName = entityType.Name + "Configuration" + host.Extension;
                resultingFiles.AdditionalFiles.Add(
                    new ScaffoldedFile
                    {
                        Path = options.ContextDir != null
                            ? Path.Combine(options.ContextDir, configurationFileName)
                            : configurationFileName,
                        Code = generatedCode
                    });
            }
        }

        return resultingFiles;
    }

    private string ProcessTemplate(string inputFile, TextTemplatingEngineHost host)
    {
        var output = Engine.ProcessTemplate(File.ReadAllText(inputFile), host);

        foreach (CompilerError error in host.Errors)
        {
            _reporter.Write(error);
        }

        if (host.OutputEncoding != Encoding.UTF8)
        {
            _reporter.WriteWarning(DesignStrings.EncodingIgnored(host.OutputEncoding.WebName));
        }

        if (host.Errors.HasErrors)
        {
            throw new OperationException(DesignStrings.ErrorGeneratingOutput(inputFile));
        }

        return output;
    }
}
