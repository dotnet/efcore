// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Tools;

internal static class CommandData
{
    public static JsonDocument ReadJson(ToolingProcessResult result)
    {
        result.EnsureSuccess();
        var data = string.Join(
            Environment.NewLine,
            result.StandardOutput.Split('\n')
                .Where(line => line.StartsWith("data:", StringComparison.Ordinal))
                .Select(line => line["data:".Length..]));
        Assert.False(string.IsNullOrWhiteSpace(data), $"No EF data output. Logs: {result.LogPath}.*");
        return JsonDocument.Parse(data);
    }
}
