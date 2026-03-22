// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools;

internal sealed record DotNetEfConfig(
    string Path,
    string? Project,
    string? StartupProject,
    string? Context,
    string? Framework,
    string? Configuration);

internal static class DotNetEfConfigLoader
{
    private const string ConfigDirectoryName = ".config";
    private const string ConfigFileName = "dotnet-ef.json";

    public static DotNetEfConfig? Load(string currentDirectory)
    {
        var configPath = Discover(currentDirectory);
        return configPath == null ? null : LoadFile(configPath);
    }

    private static string? Discover(string currentDirectory)
    {
        var directory = new DirectoryInfo(Path.GetFullPath(currentDirectory));

        while (directory != null)
        {
            var configPath = Path.Combine(directory.FullName, ConfigDirectoryName, ConfigFileName);
            if (File.Exists(configPath))
            {
                return configPath;
            }

            directory = directory.Parent;
        }

        return null;
    }

    internal static DotNetEfConfig LoadFile(string configPath)
    {
        var fullPath = Path.GetFullPath(configPath);

        JsonDocument document;
        try
        {
            using var stream = File.OpenRead(fullPath);
            document = JsonDocument.Parse(stream);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new CommandException(Resources.DotNetEfConfigReadFailed(fullPath, exception.Message), exception);
        }
        catch (JsonException exception)
        {
            throw new CommandException(Resources.DotNetEfConfigInvalidJson(fullPath, exception.Message), exception);
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new CommandException(Resources.DotNetEfConfigInvalidRoot(fullPath));
            }

            var configDirectory = Directory.GetParent(Path.GetDirectoryName(fullPath)!)!.FullName;
            string? project = null;
            string? startupProject = null;
            string? context = null;
            string? framework = null;
            string? configuration = null;

            foreach (var property in document.RootElement.EnumerateObject())
            {
                var value = ValidateValue(fullPath, property);

                switch (property.Name)
                {
                    case "project":
                        project = ResolvePath(configDirectory, value);
                        break;
                    case "startupProject":
                        startupProject = ResolvePath(configDirectory, value);
                        break;
                    case "context":
                        context = value;
                        break;
                    case "framework":
                        framework = value;
                        break;
                    case "configuration":
                        configuration = value;
                        break;
                    case "connection":
                    case "connectionString":
                    case "provider":
                    case "runtime":
                        throw new CommandException(Resources.DotNetEfConfigForbiddenProperty(fullPath, property.Name));
                    default:
                        throw new CommandException(Resources.DotNetEfConfigUnknownProperty(fullPath, property.Name));
                }
            }

            return new DotNetEfConfig(fullPath, project, startupProject, context, framework, configuration);
        }
    }

    private static string ValidateValue(string fullPath, JsonProperty property)
        => property.Value.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(property.Value.GetString())
                ? property.Value.GetString()!
                : throw new CommandException(Resources.DotNetEfConfigInvalidValue(fullPath, property.Name));

    private static string ResolvePath(string configDirectory, string path)
        => Path.IsPathRooted(path)
            ? path
            : Path.GetFullPath(Path.Combine(configDirectory, path));
}
