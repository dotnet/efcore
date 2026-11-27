// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Tools;

public class ExeStartInfoTest
{
    [Fact]
    public void CreateStartInfo_decodes_redirected_streams_with_the_requested_encoding()
    {
        var startInfo = Exe.CreateStartInfo(
            "dotnet", "--info", workingDirectory: null, redirectOutput: true, redirectError: true, Encoding.UTF8);

        Assert.True(startInfo.RedirectStandardOutput);
        Assert.True(startInfo.RedirectStandardError);
        Assert.Equal(Encoding.UTF8, startInfo.StandardOutputEncoding);
        Assert.Equal(Encoding.UTF8, startInfo.StandardErrorEncoding);
    }

    [Fact]
    public void CreateStartInfo_leaves_the_default_encoding_when_none_is_requested()
    {
        var startInfo = Exe.CreateStartInfo(
            "dotnet", "--info", workingDirectory: null, redirectOutput: true, redirectError: true, outputEncoding: null);

        Assert.Null(startInfo.StandardOutputEncoding);
        Assert.Null(startInfo.StandardErrorEncoding);
    }

    [Fact]
    public void CreateStartInfo_does_not_set_encodings_for_streams_that_are_not_redirected()
    {
        var startInfo = Exe.CreateStartInfo(
            "dotnet", "--info", workingDirectory: null, redirectOutput: false, redirectError: false, Encoding.UTF8);

        Assert.False(startInfo.RedirectStandardOutput);
        Assert.Null(startInfo.StandardOutputEncoding);
        Assert.Null(startInfo.StandardErrorEncoding);
    }
}
