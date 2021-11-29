// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A base type for conventions that configure model aspects based on whether the member type
///     is a non-nullable reference type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public abstract class NonNullableConventionBase : IModelFinalizingConvention
{
    // For the interpretation of nullability metadata, see
    // https://github.com/dotnet/roslyn/blob/master/docs/features/nullable-metadata.md

    private const string StateAnnotationName = "NonNullableConventionState";
    private const string NullableAttributeFullName = "System.Runtime.CompilerServices.NullableAttribute";
    private const string NullableContextAttributeFullName = "System.Runtime.CompilerServices.NullableContextAttribute";

    /// <summary>
    ///     Creates a new instance of <see cref="NonNullableConventionBase" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    protected NonNullableConventionBase(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Returns a value indicating whether the member type is a non-nullable reference type.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to build the model.</param>
    /// <param name="memberInfo">The member info.</param>
    /// <returns><see langword="true" /> if the member type is a non-nullable reference type.</returns>
    protected virtual bool IsNonNullableReferenceType(
        IConventionModelBuilder modelBuilder,
        MemberInfo memberInfo)
    {
        if (memberInfo.GetMemberType().IsValueType)
        {
            return false;
        }

        var state = GetOrInitializeState(modelBuilder);

        // First check for [MaybeNull] on the return value. If it exists, the member is nullable.
        // Note: avoid using GetCustomAttribute<> below because of https://github.com/mono/mono/issues/17477
        var isMaybeNull = memberInfo switch
        {
            FieldInfo f
                => f.CustomAttributes.Any(a => a.AttributeType == typeof(MaybeNullAttribute)),
            PropertyInfo p
                => p.GetMethod?.ReturnParameter?.CustomAttributes?.Any(a => a.AttributeType == typeof(MaybeNullAttribute)) == true,
            _ => false
        };

        if (isMaybeNull)
        {
            return false;
        }

        // For C# 8.0 nullable types, the C# compiler currently synthesizes a NullableAttribute that expresses nullability into
        // assemblies it produces. If the model is spread across more than one assembly, there will be multiple versions of this
        // attribute, so look for it by name, caching to avoid reflection on every check.
        // Note that this may change - if https://github.com/dotnet/corefx/issues/36222 is done we can remove all of this.

        // First look for NullableAttribute on the member itself
        if (Attribute.GetCustomAttributes(memberInfo)
                .FirstOrDefault(a => a.GetType().FullName == NullableAttributeFullName) is Attribute attribute)
        {
            var attributeType = attribute.GetType();

            if (attributeType != state.NullableAttrType)
            {
                state.NullableFlagsFieldInfo = attributeType.GetField("NullableFlags");
                state.NullableAttrType = attributeType;
            }

            if (state.NullableFlagsFieldInfo?.GetValue(attribute) is byte[] flags)
            {
                return flags.FirstOrDefault() == 1;
            }
        }

        // No attribute on the member, try to find a NullableContextAttribute on the declaring type
        var type = memberInfo.DeclaringType;
        if (type is not null)
        {
            // We currently don't calculate support nullability for generic properties, since calculating that is complex
            // (depends on the nullability of generic type argument).
            // However, we special case Dictionary as it's used for property bags, and specifically don't identify its indexer
            // as non-nullable.
            if (memberInfo is PropertyInfo property
                && property.IsIndexerProperty()
                && type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                return false;
            }

            return DoesTypeHaveNonNullableContext(type, state);
        }

        return false;
    }

    private bool DoesTypeHaveNonNullableContext(Type type, NonNullabilityConventionState state)
    {
        if (state.TypeCache.TryGetValue(type, out var cachedTypeNonNullable))
        {
            return cachedTypeNonNullable;
        }

        if (Attribute.GetCustomAttributes(type)
                .FirstOrDefault(a => a.GetType().FullName == NullableContextAttributeFullName) is Attribute contextAttr)
        {
            var attributeType = contextAttr.GetType();

            if (attributeType != state.NullableContextAttrType)
            {
                state.NullableContextFlagFieldInfo = attributeType.GetField("Flag");
                state.NullableContextAttrType = attributeType;
            }

            if (state.NullableContextFlagFieldInfo?.GetValue(contextAttr) is byte flag)
            {
                return state.TypeCache[type] = flag == 1;
            }
        }
        else if (type.IsNested)
        {
            return state.TypeCache[type] = DoesTypeHaveNonNullableContext(type.DeclaringType!, state);
        }

        return state.TypeCache[type] = false;
    }

    private NonNullabilityConventionState GetOrInitializeState(IConventionModelBuilder modelBuilder)
        => (NonNullabilityConventionState)(
            modelBuilder.Metadata.FindAnnotation(StateAnnotationName)
            ?? modelBuilder.Metadata.AddAnnotation(StateAnnotationName, new NonNullabilityConventionState())
        ).Value!;

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
        => modelBuilder.Metadata.RemoveAnnotation(StateAnnotationName);

    private sealed class NonNullabilityConventionState
    {
        public Type? NullableAttrType;
        public Type? NullableContextAttrType;
        public FieldInfo? NullableFlagsFieldInfo;
        public FieldInfo? NullableContextFlagFieldInfo;
        public Dictionary<Type, bool> TypeCache { get; } = new();
    }
}
