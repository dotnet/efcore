// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CodeDom.Compiler;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.VisualStudio.TextTemplating;
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

        string? generatedCode = null;
        if (File.Exists(contextTemplate))
        {
            host.TemplateFile = contextTemplate;

            var compiledTemplate = Engine.CompileTemplateAsync(File.ReadAllText(contextTemplate), host, new()).GetAwaiter().GetResult();

            if (compiledTemplate != null)
            {
                generatedCode = ProcessTemplate(compiledTemplate, host);
            }

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
                Code = generatedCode!
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
                        compiledEntityTypeTemplate = Engine.CompileTemplateAsync(File.ReadAllText(entityTypeTemplate), host, new()).GetAwaiter().GetResult();;
                        entityTypeExtension = host.Extension;
                        CheckEncoding(host.OutputEncoding);
                    }

                    if (compiledEntityTypeTemplate != null)
                    {
                        generatedCode = ProcessTemplate(compiledEntityTypeTemplate, host);
                    }

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
                        compiledConfigurationTemplate = Engine.CompileTemplateAsync(File.ReadAllText(configurationTemplate), host, new()).GetAwaiter().GetResult();;
                        configurationExtension = host.Extension;
                        CheckEncoding(host.OutputEncoding);
                    }

                    if (compiledConfigurationTemplate != null)
                    {
                        generatedCode = ProcessTemplate(compiledConfigurationTemplate, host);
                    }

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

    private static string ProcessTemplate(CompiledTemplate compiledTemplate, TextTemplatingEngineHost host)
    {
        var templateAssemblyData = GetField(compiledTemplate, "templateAssemblyData")!;
        var templateClassFullName = (string)GetField(compiledTemplate, "templateClassFullName")!;
        var culture = GetField(compiledTemplate, "culture");
        var assemblyBytes = (byte[])templateAssemblyData.GetType().GetProperty("Assembly")!.GetValue(templateAssemblyData)!;

        var assembly = Assembly.Load(assemblyBytes);
        var transformType = assembly.GetType(templateClassFullName)!;
        var textTransformation = Activator.CreateInstance(transformType);

        var hostProp = transformType.GetProperty("Host", typeof(ITextTemplatingEngineHost));
        if (hostProp != null)
        {
            hostProp.SetValue(textTransformation, host, null);
        }

        var sessionProp = transformType.GetProperty("Session", typeof(IDictionary<string, object>));
        if (sessionProp != null)
        {
            sessionProp.SetValue(textTransformation, host.Session, null);
        }

        var errorProp = transformType.GetProperty("Errors", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var errorMethod = transformType.GetMethod("Error", new[] { typeof(string) })!;

        var errors = (CompilerErrorCollection)errorProp.GetValue(textTransformation, null)!;
        errors.Clear();

        ToStringHelper.FormatProvider = culture != null ? (IFormatProvider)culture : CultureInfo.InvariantCulture;

        string? output = null;

        var initMethod = transformType.GetMethod("Initialize")!;
        var transformMethod = transformType.GetMethod("TransformText")!;

        try
        {
            initMethod.Invoke(textTransformation, null);
            output = (string?)transformMethod.Invoke(textTransformation, null);
        }
        catch (Exception ex)
        {
            errorMethod.Invoke(textTransformation, new object[] { "Error running transform: " + ex });
        }

        host.LogErrors(errors);
        return output!;

        static object? GetField(CompiledTemplate compiledTemplate, string fieldName)
            => compiledTemplate.GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(compiledTemplate);
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
