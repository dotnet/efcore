// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CodeDom.Compiler;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Microsoft.VisualStudio.TextTemplating;

namespace Microsoft.EntityFrameworkCore.TextTemplating.Internal;

[PlatformSkipCondition(TestPlatform.Linux, SkipReason = "CI time out")]
public class TextTemplatingServiceTest
{
    [ConditionalFact]
    public void Service_works()
    {
        var host = new TextTemplatingService(
            new ServiceCollection()
                .AddSingleton("Hello, Services!")
                .BuildServiceProvider());
        var callback = new TextTemplatingCallback();

        var result = host.ProcessTemplate(
            @"T:\test.tt",
            @"<#@ template hostSpecific=""true"" #><#= ((IServiceProvider)Host).GetService(typeof(string)) #>",
            callback);

        Assert.Empty(callback.Errors);
        Assert.Equal("Hello, Services!", result);
    }

    [ConditionalFact]
    public void Session_works()
    {
        var host = new TextTemplatingService(
            new ServiceCollection()
            .BuildServiceProvider());
        host.Session = new TextTemplatingSession
        {
            ["Value"] = "Hello, Session!"
        };
        var callback = new TextTemplatingCallback();

        var result = host.ProcessTemplate(
            @"T:\test.tt",
            @"<#= Session[""Value""] #>",
            callback);

        Assert.Empty(callback.Errors);
        Assert.Equal("Hello, Session!", result);
    }

    [ConditionalFact]
    public void Session_works_with_parameter()
    {
        var host = new TextTemplatingService(
            new ServiceCollection()
            .BuildServiceProvider());
        host.Session = new TextTemplatingSession
        {
            ["Value"] = "Hello, Session!"
        };
        var callback = new TextTemplatingCallback();

        var result = host.ProcessTemplate(
            @"T:\test.tt",
            @"<#@ parameter name=""Value"" type=""System.String"" #><#= Value #>",
            callback);

        Assert.Empty(callback.Errors);
        Assert.Equal("Hello, Session!", result);
    }

    [ConditionalFact]
    public void Include_works()
    {
        using var dir = new TempDirectory();
        File.WriteAllText(
            Path.Combine(dir, "test.ttinclude"),
            "Hello, Include!");

        var host = new TextTemplatingService(
            new ServiceCollection()
            .BuildServiceProvider());
        var callback = new TextTemplatingCallback();

        var result = host.ProcessTemplate(
            Path.Combine(dir, "test.tt"),
            @"<#@ include file=""test.ttinclude"" #>",
            callback);

        Assert.Empty(callback.Errors);
        Assert.Equal("Hello, Include!", result);
    }

    [ConditionalFact]
    public void Error_works()
    {
        var host = new TextTemplatingService(
            new ServiceCollection()
            .BuildServiceProvider());
        var callback = new TextTemplatingCallback();

        host.ProcessTemplate(
            @"T:\test.tt",
            @"<# Error(""Hello, Error!""); #>",
            callback);

        var error = Assert.Single(callback.Errors.Cast<CompilerError>());
        Assert.Equal("Hello, Error!", error.ErrorText);
    }

    [ConditionalFact]
    public void Directive_throws_when_processor_unknown()
    {
        var host = new TextTemplatingService(
            new ServiceCollection()
            .BuildServiceProvider());
        var callback = new TextTemplatingCallback();

        var ex = Assert.Throws<FileNotFoundException>(
            () => host.ProcessTemplate(
                @"T:\test.tt",
                @"<#@ test processor=""TestDirectiveProcessor"" #>",
                callback));

        Assert.Equal(DesignStrings.UnknownDirectiveProcessor("TestDirectiveProcessor"), ex.Message);
    }

    [ConditionalFact]
    public void ResolvePath_work()
    {
        using var dir = new TempDirectory();

        var host = new TextTemplatingService(
            new ServiceCollection()
                .BuildServiceProvider());
        var callback = new TextTemplatingCallback();

        var result = host.ProcessTemplate(
            Path.Combine(dir, "test.tt"),
            @"<#@ template hostSpecific=""true"" #><#= Host.ResolvePath(""data.json"") #>",
            callback);

        Assert.Empty(callback.Errors);
        Assert.Equal(Path.Combine(dir, "data.json"), result);
    }

    [ConditionalFact]
    public void Output_works()
    {
        var host = new TextTemplatingService(
            new ServiceCollection()
                .BuildServiceProvider());
        var callback = new TextTemplatingCallback();

        host.ProcessTemplate(
            @"T:\test.tt",
            @"<#@ output extension="".txt"" encoding=""us-ascii"" #>",
            callback);

        Assert.Empty(callback.Errors);
        Assert.Equal(".txt", callback.Extension);
        Assert.Equal(Encoding.ASCII, callback.OutputEncoding);
    }

    [ConditionalFact]
    public void Assembly_works()
    {
        var host = new TextTemplatingService(
            new ServiceCollection()
                .BuildServiceProvider());
        var callback = new TextTemplatingCallback();

        var result = host.ProcessTemplate(
            @"T:\test.tt",
            @"<#@ assembly name=""Microsoft.EntityFrameworkCore"" #><#= nameof(Microsoft.EntityFrameworkCore.DbContext) #>",
            callback);

        Assert.Empty(callback.Errors);
        Assert.Equal("DbContext", result);
    }
}
