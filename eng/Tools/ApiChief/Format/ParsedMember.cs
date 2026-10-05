// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using ApiChief.Model;
using ICSharpCode.Decompiler.TypeSystem;

namespace ApiChief.Format;

/// <remarks>
/// Represents information retrieved by analyzing annotations of the entity.
/// </remarks>
internal class ParsedMember
{
    private const string ObsoleteAttribute = "System.ObsoleteAttribute";
    private const string ExperimentalAttribute = "System.Diagnostics.CodeAnalysis.ExperimentalAttribute";
    private const string ReadonlyRefStructMessage = "Types with embedded references are not supported in this version of your compiler.";

    public ApiStage Stage { get; }

    public ParsedMember(IEntity entity, MetadataModule? metadata = null, ApiStage? parentStage = null)
    {
        Stage = ExtractStage(entity, metadata, parentStage);
    }

    /// <remarks>
    /// C# compiler includes <c>ObsoleteAttribute</c> for readonly ref structs with the
    /// <see cref="ReadonlyRefStructMessage" /> message to block usage on older compilers.
    /// ApiChief should ignore that compiler-generated attribute so readonly ref structs do not
    /// appear deprecated unless they are explicitly marked as such.
    /// </remarks>
    private static ApiStage ExtractStage(IEntity entity, MetadataModule? metadata, ApiStage? parentStage)
    {
        if (AssemblyIsAnnotatedAsExperimental(metadata))
        {
            return ApiStage.Experimental;
        }

        var type = entity as ITypeDefinition;
        var isReadonlyRefStruct = type != null && type.Kind == TypeKind.Struct && type.IsReadOnly && type.IsByRefLike;

        foreach (var attribute in entity.GetAttributes())
        {
            if (attribute.AttributeType.FullName == ObsoleteAttribute)
            {
                var firstAttributeParameter = (string?)attribute.FixedArguments.FirstOrDefault().Value ?? string.Empty;
                var isGeneratedAttribute = firstAttributeParameter == ReadonlyRefStructMessage;

                if (isReadonlyRefStruct && isGeneratedAttribute)
                {
                    continue;
                }

                return ApiStage.Obsolete;
            }

            if (attribute.AttributeType.FullName == ExperimentalAttribute)
            {
                return ApiStage.Experimental;
            }
        }

        if (parentStage != null && parentStage != ApiStage.Stable)
        {
            return (ApiStage)parentStage;
        }

        return ApiStage.Stable;

        static bool AssemblyIsAnnotatedAsExperimental(MetadataModule? metadataModule)
            => metadataModule != null
                && metadataModule.GetAssemblyAttributes().Any(x => x.AttributeType.FullName == ExperimentalAttribute);
    }
}
