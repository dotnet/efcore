// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
    private const string StateAnnotationName = "NonNullableConventionState";

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

        var annotation =
            modelBuilder.Metadata.FindAnnotation(StateAnnotationName)
            ?? modelBuilder.Metadata.AddAnnotation(StateAnnotationName, new NullabilityInfoContext());

        var nullabilityInfoContext = (NullabilityInfoContext)annotation.Value!;

        if (memberInfo is PropertyInfo propertyInfo)
        {
            var nullabilityInfo = nullabilityInfoContext.Create(propertyInfo);

            if (nullabilityInfo.ReadState == NullabilityState.NotNull)
            {
                return true;
            }

            // In order for us to configure a property as non-nullable, it must be:
            // 1. Non-nullable for both read and write, or
            // 2. Non-nullable for read and read-only, or
            // 3. Non-nullable for write and write-only
            // if (nullabilityInfo.ReadState == NullabilityState.NotNull
            //     && (nullabilityInfo.WriteState == NullabilityState.NotNull || !propertyInfo.CanWrite)
            //     || nullabilityInfo.WriteState == NullabilityState.NotNull && !propertyInfo.CanRead)
            // {
            //     return true;
            // }
        }
        else if (memberInfo is FieldInfo fieldInfo)
        {
            var nullabilityInfo = nullabilityInfoContext.Create(fieldInfo);

            if (nullabilityInfo.ReadState == NullabilityState.NotNull /* && nullabilityInfo.WriteState == NullabilityState.NotNull */)
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
        => modelBuilder.Metadata.RemoveAnnotation(StateAnnotationName);
}
