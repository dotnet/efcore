// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
#if NET
using System.Diagnostics.CodeAnalysis;
#endif
using System.Reflection;
using Xunit.v3;

namespace Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

/// <summary>
///     An assembly-level attribute that conditionally marks all tests in the assembly to be skipped
///     based on the evaluation of one or more static boolean members. When any of the referenced
///     condition members evaluates to <c>false</c>, the attribute contributes a <c>category=failing</c>
///     trait so that the Arcade test runner can exclude the affected tests.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class ConditionalAssemblyAttribute : Attribute, ITraitAttribute
{
#if NET
    private const DynamicallyAccessedMemberTypes ConditionalMemberKinds =
        DynamicallyAccessedMemberTypes.PublicMethods
        | DynamicallyAccessedMemberTypes.NonPublicMethods
        | DynamicallyAccessedMemberTypes.PublicProperties
        | DynamicallyAccessedMemberTypes.NonPublicProperties
        | DynamicallyAccessedMemberTypes.PublicFields
        | DynamicallyAccessedMemberTypes.NonPublicFields;

    [DynamicallyAccessedMembers(ConditionalMemberKinds)]
#endif
    public Type CalleeType { get; }

    public string[] ConditionMemberNames { get; }

    public ConditionalAssemblyAttribute(
#if NET
        [DynamicallyAccessedMembers(ConditionalMemberKinds)]
#endif
        Type calleeType,
        params string[] conditionMemberNames)
    {
        CalleeType = calleeType;
        ConditionMemberNames = conditionMemberNames;
    }

    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits()
        => Evaluate()
            ? []
            : [new KeyValuePair<string, string>("category", "failing")];

    private bool Evaluate()
    {
        foreach (var memberName in ConditionMemberNames)
        {
            if (!EvaluateMember(CalleeType, memberName))
            {
                return false;
            }
        }

        return true;
    }

    private static bool EvaluateMember(Type type, string memberName)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        var property = type.GetProperty(memberName, flags);
        if (property is { PropertyType: { } pt } && pt == typeof(bool))
        {
            return (bool)property.GetValue(null)!;
        }

        var field = type.GetField(memberName, flags);
        if (field is { FieldType: { } ft } && ft == typeof(bool))
        {
            return (bool)field.GetValue(null)!;
        }

        var method = type.GetMethod(memberName, flags, binder: null, types: Type.EmptyTypes, modifiers: null);
        if (method is { ReturnType: { } rt } && rt == typeof(bool))
        {
            return (bool)method.Invoke(null, null)!;
        }

        throw new InvalidOperationException(
            $"Cannot find static bool property/field/method '{memberName}' on type '{type.FullName}'.");
    }
}
