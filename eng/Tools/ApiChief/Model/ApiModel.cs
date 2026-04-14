// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using ApiChief.Format;
using ApiChief.Processing;

namespace ApiChief.Model;

public sealed class ApiModel
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        IncludeFields = true
    };

    static ApiModel()
    {
        _serializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public string Name { get; set; } = string.Empty;
    public ISet<ApiType> Types { get; set; } = new HashSet<ApiType>();

    public static ApiModel LoadFromAssembly(string path)
    {
        var decompiler = DecompilerFactory.Create(path);
        var finalApi = new ApiModel { Name = decompiler.TypeSystem.MainModule.AssemblyName };

        ApiProcessor.Process(finalApi, decompiler);

        return finalApi;
    }

    public static ApiModel LoadFromFile(string path)
        => JsonSerializer.Deserialize<ApiModel>(File.ReadAllText(path), _serializerOptions)!;

    public void EvaluateDelta(ApiModel current, bool includeSharedMembers = false)
        => current.Types = FindChanges(this, current, includeSharedMembers);

    public bool HasRemovals()
        => Types.Any(static type => type.Removals != null);

    public override string ToString()
        => JsonSerializer.Serialize(this, _serializerOptions).ReplaceLineEndings(Environment.NewLine);

    private static ISet<ApiType> FindChanges(ApiModel baseline, ApiModel current, bool includeSharedMembers)
    {
        ISet<ApiType> result = new HashSet<ApiType>();

        foreach (var currentType in current.Types)
        {
            var baselineType = baseline.Types.FirstOrDefault(type => type.Equals(currentType));
            if (baselineType == null)
            {
                var delta = CreateTypeDelta(currentType, currentType, null, includeSharedMembers: false)!;
                delta.IsNew = true;
                result.Add(delta);
                continue;
            }

            var typeDelta = CreateTypeDelta(currentType, currentType, baselineType, includeSharedMembers);
            if (typeDelta != null)
            {
                result.Add(typeDelta);
            }
        }

        foreach (var baselineType in baseline.Types)
        {
            if (current.Types.Any(type => type.Equals(baselineType)))
            {
                continue;
            }

            var removedDelta = CreateTypeDelta(baselineType, null, baselineType, includeSharedMembers: false)!;
            removedDelta.IsRemoved = true;
            result.Add(removedDelta);
        }

        return result;
    }

    private static ApiType? CreateTypeDelta(ApiType outputType, ApiType? currentType, ApiType? baselineType, bool includeSharedMembers)
    {
        var stageChanged = currentType != null && baselineType != null && currentType.Stage != baselineType.Stage;
        if (stageChanged)
        {
            includeSharedMembers = true;
        }

        var (addedMethods, removedMethods, sharedMethods) = PartitionMembers(currentType?.Methods, baselineType?.Methods, includeSharedMembers);
        var (addedFields, removedFields, sharedFields) = PartitionMembers(currentType?.Fields, baselineType?.Fields, includeSharedMembers);
        var (addedProperties, removedProperties, sharedProperties) = PartitionMembers(currentType?.Properties, baselineType?.Properties, includeSharedMembers);

        var addedStage = currentType != null && (baselineType == null || currentType.Stage != baselineType.Stage)
            ? currentType.Stage
            : ApiStage.Stable;
        var removedStage = baselineType != null && (currentType == null || baselineType.Stage != currentType.Stage)
            ? baselineType.Stage
            : ApiStage.Stable;

        var additions = CreateChangeSet(addedStage, addedMethods, addedFields, addedProperties);
        var removals = CreateChangeSet(removedStage, removedMethods, removedFields, removedProperties);

        if (addedStage == ApiStage.Stable
            && removedStage == ApiStage.Stable
            && additions == null
            && removals == null)
        {
            return null;
        }

        return new ApiType
        {
            Type = outputType.Type,
            Stage = currentType?.Stage ?? baselineType?.Stage ?? ApiStage.Stable,
            Methods = ToDisplayMembers(sharedMethods),
            Fields = ToDisplayMembers(sharedFields),
            Properties = ToDisplayMembers(sharedProperties),
            Additions = additions,
            Removals = removals,
        };
    }

    private static ApiType? CreateChangeSet(
        ApiStage stage,
        ISet<ApiMember>? methods,
        ISet<ApiMember>? fields,
        ISet<ApiMember>? properties)
        => stage != ApiStage.Stable || methods != null || fields != null || properties != null
            ? new ApiType
            {
                Stage = stage,
                Methods = ToDisplayMembers(methods),
                Fields = ToDisplayMembers(fields),
                Properties = ToDisplayMembers(properties),
            }
            : null;

    private static (ISet<ApiMember>? added, ISet<ApiMember>? removed, ISet<ApiMember>? shared) PartitionMembers(
        ISet<ApiMember>? currentMembers,
        ISet<ApiMember>? baselineMembers,
        bool includeSharedMembers)
    {
        if (currentMembers == null && baselineMembers == null)
        {
            return (null, null, null);
        }

        HashSet<ApiMember>? added = currentMembers != null ? [.. currentMembers] : null;
        HashSet<ApiMember>? deleted = baselineMembers != null ? [.. baselineMembers] : null;
        HashSet<ApiMember>? shared = null;

        if (currentMembers != null && deleted != null)
        {
            foreach (var member in currentMembers)
            {
                if (!deleted.Contains(member))
                {
                    continue;
                }

                added?.Remove(member);
                deleted.Remove(member);

                if (includeSharedMembers)
                {
                    shared ??= new HashSet<ApiMember>();
                    shared.Add(member);
                }
            }
        }

        return (
            added is { Count: > 0 } ? added : null,
            deleted is { Count: > 0 } ? deleted : null,
            shared is { Count: > 0 } ? shared : null);
    }

    private static ISet<ApiMember>? ToDisplayMembers(ISet<ApiMember>? members)
        => members?.Select(CreateDisplayMember).ToHashSet();

    private static ApiMember CreateDisplayMember(ApiMember member)
        => new()
        {
            Member = member.Member.WithSimpleTypeNames(),
            Stage = member.Stage,
            Value = member.Value,
        };
}
