// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CodeDom.Compiler;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.VisualStudio.TextTemplating;
using Engine = Mono.TextTemplating.TemplatingEngine;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

[SkipOnPlatform(TestPlatforms.Linux | TestPlatforms.OSX, "Test does not run on Linux or macOS")]
public class TextTemplatingEngineHostTest
{
    public static readonly Engine _engine = new();

    [Fact]
    public void Service_works()
    {
        var host = new TextTemplatingEngineHost(
            new ServiceCollection()
                .AddSingleton("Hello, Services!")
                .BuildServiceProvider());

        var result = _engine.ProcessTemplateAsync(
            @"<#@ template hostSpecific=""true"" #><#= ((IServiceProvider)Host).GetService(typeof(string)) #>",
            host).GetAwaiter().GetResult();

        Assert.Empty(host.Errors);
        Assert.Equal("Hello, Services!", result);
    }

    [Fact]
    public void Session_works()
    {
        var host = new TextTemplatingEngineHost { Session = new TextTemplatingSession { ["Value"] = "Hello, Session!" } };

        var result = _engine.ProcessTemplateAsync(
            @"<#= Session[""Value""] #>",
            host).GetAwaiter().GetResult();

        Assert.Empty(host.Errors);
        Assert.Equal("Hello, Session!", result);
    }

    [Fact]
    public void Session_works_with_parameter()
    {
        var host = new TextTemplatingEngineHost { Session = new TextTemplatingSession { ["Value"] = "Hello, Session!" } };

        var result = _engine.ProcessTemplateAsync(
            @"<#@ parameter name=""Value"" type=""System.String"" #><#= Value #>",
            host).GetAwaiter().GetResult();

        Assert.Empty(host.Errors);
        Assert.Equal("Hello, Session!", result);
    }

    [Fact]
    public void Include_works()
    {
        using var dir = new TempDirectory();
        File.WriteAllText(
            Path.Combine(dir, "test.ttinclude"),
            "Hello, Include!");

        var host = new TextTemplatingEngineHost { TemplateFile = Path.Combine(dir, "test.tt") };

        var result = _engine.ProcessTemplateAsync(
            @"<#@ include file=""test.ttinclude"" #>",
            host).GetAwaiter().GetResult();

        Assert.Empty(host.Errors);
        Assert.Equal("Hello, Include!", result);
    }

    [Fact]
    public void Error_works()
    {
        var host = new TextTemplatingEngineHost();

        _engine.ProcessTemplateAsync(
            @"<# Error(""Hello, Error!""); #>",
            host).GetAwaiter().GetResult();

        var error = Assert.Single(host.Errors.Cast<CompilerError>());
        Assert.Equal("Hello, Error!", error.ErrorText);
    }

    [Fact]
    public void Directive_throws_when_processor_unknown()
    {
        var host = new TextTemplatingEngineHost();

        var ex = Assert.Throws<FileNotFoundException>(() => _engine.ProcessTemplateAsync(
            @"<#@ test processor=""TestDirectiveProcessor"" #>",
            host).GetAwaiter().GetResult());

        Assert.Equal(DesignStrings.UnknownDirectiveProcessor("TestDirectiveProcessor"), ex.Message);
    }

    [Fact]
    public void ResolvePath_work()
    {
        using var dir = new TempDirectory();

        var host = new TextTemplatingEngineHost { TemplateFile = Path.Combine(dir, "test.tt") };

        var result = _engine.ProcessTemplateAsync(
            @"<#@ template hostSpecific=""true"" #><#= Host.ResolvePath(""data.json"") #>",
            host).GetAwaiter().GetResult();

        Assert.Empty(host.Errors);
        Assert.Equal(Path.Combine(dir, "data.json"), result);
    }

    [Fact]
    public void Output_works()
    {
        var host = new TextTemplatingEngineHost();

        _engine.ProcessTemplateAsync(
            @"<#@ output extension="".txt"" encoding=""us-ascii"" #>",
            host).GetAwaiter().GetResult();

        Assert.Empty(host.Errors);
        Assert.Equal(".txt", host.Extension);
        Assert.Equal(Encoding.ASCII, host.OutputEncoding);
    }

    [Fact]
    public void Assembly_works()
    {
        var host = new TextTemplatingEngineHost();

        var result = _engine.ProcessTemplateAsync(
            @"<#@ assembly name=""Microsoft.EntityFrameworkCore"" #><#= nameof(Microsoft.EntityFrameworkCore.DbContext) #>",
            host).GetAwaiter().GetResult();

        Assert.Empty(host.Errors);
        Assert.Equal("DbContext", result);
    }
}
