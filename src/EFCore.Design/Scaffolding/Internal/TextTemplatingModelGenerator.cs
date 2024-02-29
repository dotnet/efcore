// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CodeDom.Compiler;
using System.Text;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Mono.TextTemplating;

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
    private TemplatingEngine? _engine;

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
    protected virtual TemplatingEngine Engine
        => _engine ??= new TemplatingEngine();

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

            generatedCode = Engine.ProcessTemplateAsync(File.ReadAllText(contextTemplate), host).GetAwaiter().GetResult();
            CheckEncoding(host.OutputEncoding);
            HandleErrors(host);
        }
        else
        {
            if (!string.Equals(options.Language, "C#", StringComparison.OrdinalIgnoreCase))
            {
                throw new OperationException(DesignStrings.NoContextTemplate);
            }

            var defaultContextTemplate = new CSharpDbContextGenerator { Host = host, Session = host.Session };
            defaultContextTemplate.Initialize();

            generatedCode = defaultContextTemplate.TransformText();

            foreach (CompilerError error in defaultContextTemplate.Errors)
            {
                _reporter.Write(error);
            }

            if (defaultContextTemplate.Errors.HasErrors)
            {
                throw new OperationException(DesignStrings.ErrorGeneratingOutput(defaultContextTemplate.GetType().Name));
            }
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

            CompiledTemplate? compiledEntityTypeTemplate = null;
            string? entityTypeExtension = null;
            try
            {
                foreach (var entityType in model.GetEntityTypes())
                {
                    host.Initialize();
                    host.Session.Add("EntityType", entityType);
                    host.Session.Add("Options", options);
                    host.Session.Add("NamespaceHint", options.ModelNamespace);
                    host.Session.Add("ProjectDefaultNamespace", options.RootNamespace);

                    if (compiledEntityTypeTemplate is null)
                    {
                        compiledEntityTypeTemplate = Engine.CompileTemplateAsync(File.ReadAllText(entityTypeTemplate), host, default)
                            .GetAwaiter().GetResult();
                        entityTypeExtension = host.Extension;
                        CheckEncoding(host.OutputEncoding);
                    }

                    generatedCode = compiledEntityTypeTemplate.Process();
                    HandleErrors(host);

                    if (string.IsNullOrWhiteSpace(generatedCode))
                    {
                        continue;
                    }

                    var entityTypeFileName = entityType.Name + entityTypeExtension;
                    resultingFiles.AdditionalFiles.Add(
                        new ScaffoldedFile { Path = entityTypeFileName, Code = generatedCode });
                }
            }
            finally
            {
                compiledEntityTypeTemplate?.Dispose();
            }
        }

        var configurationTemplate = Path.Combine(options.ProjectDir!, TemplatesDirectory, EntityTypeConfigurationTemplate);
        if (File.Exists(configurationTemplate))
        {
            host.TemplateFile = configurationTemplate;

            CompiledTemplate? compiledConfigurationTemplate = null;
            string? configurationExtension = null;
            try
            {
                foreach (var entityType in model.GetEntityTypes())
                {
                    host.Initialize();
                    host.Session.Add("EntityType", entityType);
                    host.Session.Add("Options", options);
                    host.Session.Add("NamespaceHint", options.ContextNamespace ?? options.ModelNamespace);
                    host.Session.Add("ProjectDefaultNamespace", options.RootNamespace);

                    if (compiledConfigurationTemplate is null)
                    {
                        compiledConfigurationTemplate = Engine.CompileTemplateAsync(File.ReadAllText(configurationTemplate), host, default)
                            .GetAwaiter().GetResult();
                        configurationExtension = host.Extension;
                        CheckEncoding(host.OutputEncoding);
                    }

                    generatedCode = compiledConfigurationTemplate.Process();
                    HandleErrors(host);

                    if (string.IsNullOrWhiteSpace(generatedCode))
                    {
                        continue;
                    }

                    var configurationFileName = entityType.Name + "Configuration" + configurationExtension;
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
            finally
            {
                compiledConfigurationTemplate?.Dispose();
            }
        }

        return resultingFiles;
    }

    private void CheckEncoding(Encoding outputEncoding)
    {
        if (outputEncoding != Encoding.UTF8)
        {
            _reporter.WriteWarning(DesignStrings.EncodingIgnored(outputEncoding.WebName));
        }
    }

    private void HandleErrors(TextTemplatingEngineHost host)
    {
        foreach (CompilerError error in host.Errors)
        {
            _reporter.Write(error);
        }

        if (host.Errors.HasErrors)
        {
            throw new OperationException(DesignStrings.ErrorGeneratingOutput(host.TemplateFile));
        }
    }
}
