// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CodeDom.Compiler;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Microsoft.VisualStudio.TextTemplating;
using Engine = Mono.TextTemplating.TemplatingEngine;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

[PlatformSkipCondition(TestPlatform.Linux, SkipReason = "CI time out")]
public class TextTemplatingEngineHostTest
{
    public static readonly Engine _engine = new Engine();

    [ConditionalFact]
    public void Service_works()
    {
        var host = new TextTemplatingEngineHost(
            new ServiceCollection()
                .AddSingleton("Hello, Services!")
                .BuildServiceProvider());

        var result = _engine.ProcessTemplate(
            @"<#@ template hostSpecific=""true"" #><#= ((IServiceProvider)Host).GetService(typeof(string)) #>",
            host);

        Assert.Empty(host.Errors);
        Assert.Equal("Hello, Services!", result);
    }

    [ConditionalFact]
    public void Session_works()
    {
        var host = new TextTemplatingEngineHost
        {
            Session = new TextTemplatingSession
            {
                ["Value"] = "Hello, Session!"
            }
        };

        var result = _engine.ProcessTemplate(
            @"<#= Session[""Value""] #>",
            host);

        Assert.Empty(host.Errors);
        Assert.Equal("Hello, Session!", result);
    }

    [ConditionalFact]
    public void Session_works_with_parameter()
    {
        var host = new TextTemplatingEngineHost
        {
            Session = new TextTemplatingSession
            {
                ["Value"] = "Hello, Session!"
            }
        };

        var result = _engine.ProcessTemplate(
            @"<#@ parameter name=""Value"" type=""System.String"" #><#= Value #>",
            host);

        Assert.Empty(host.Errors);
        Assert.Equal("Hello, Session!", result);
    }

    [ConditionalFact]
    public void Include_works()
    {
        using var dir = new TempDirectory();
        File.WriteAllText(
            Path.Combine(dir, "test.ttinclude"),
            "Hello, Include!");

        var host = new TextTemplatingEngineHost
        {
            TemplateFile = Path.Combine(dir, "test.tt")
        };

        var result = _engine.ProcessTemplate(
            @"<#@ include file=""test.ttinclude"" #>",
            host);

        Assert.Empty(host.Errors);
        Assert.Equal("Hello, Include!", result);
    }

    [ConditionalFact]
    public void Error_works()
    {
        var host = new TextTemplatingEngineHost();

        _engine.ProcessTemplate(
            @"<# Error(""Hello, Error!""); #>",
            host);

        var error = Assert.Single(host.Errors.Cast<CompilerError>());
        Assert.Equal("Hello, Error!", error.ErrorText);
    }

    [ConditionalFact]
    public void Directive_throws_when_processor_unknown()
    {
        var host = new TextTemplatingEngineHost();

        var ex = Assert.Throws<FileNotFoundException>(
            () => _engine.ProcessTemplate(
                @"<#@ test processor=""TestDirectiveProcessor"" #>",
                host));

        Assert.Equal(DesignStrings.UnknownDirectiveProcessor("TestDirectiveProcessor"), ex.Message);
    }

    [ConditionalFact]
    public void ResolvePath_work()
    {
        using var dir = new TempDirectory();

        var host = new TextTemplatingEngineHost
        {
            TemplateFile = Path.Combine(dir, "test.tt")
        };

        var result = _engine.ProcessTemplate(
            @"<#@ template hostSpecific=""true"" #><#= Host.ResolvePath(""data.json"") #>",
            host);

        Assert.Empty(host.Errors);
        Assert.Equal(Path.Combine(dir, "data.json"), result);
    }

    [ConditionalFact]
    public void Output_works()
    {
        var host = new TextTemplatingEngineHost();

        _engine.ProcessTemplate(
            @"<#@ output extension="".txt"" encoding=""us-ascii"" #>",
            host);

        Assert.Empty(host.Errors);
        Assert.Equal(".txt", host.Extension);
        Assert.Equal(Encoding.ASCII, host.OutputEncoding);
    }

    [ConditionalFact]
    public void Assembly_works()
    {
        var host = new TextTemplatingEngineHost();

        var result = _engine.ProcessTemplate(
            @"<#@ assembly name=""Microsoft.EntityFrameworkCore"" #><#= nameof(Microsoft.EntityFrameworkCore.DbContext) #>",
            host);

        Assert.Empty(host.Errors);
        Assert.Equal("DbContext", result);
    }
}
