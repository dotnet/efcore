// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;

namespace System.Text;

internal static class StringBuilderExtensions
{
    public static StringBuilder AppendJoin(
        this StringBuilder stringBuilder,
        IEnumerable<string> values,
        string separator = ", ")
        => stringBuilder.AppendJoin(values, (sb, value) => sb.Append(value), separator);

    public static StringBuilder AppendJoin(
        this StringBuilder stringBuilder,
        string separator,
        params string[] values)
        => stringBuilder.AppendJoin(values, (sb, value) => sb.Append(value), separator);

    public static StringBuilder AppendJoin<T>(
        this StringBuilder stringBuilder,
        IEnumerable<T> values,
        Action<StringBuilder, T> joinAction,
        string separator = ", ")
    {
        var appended = false;

        foreach (var value in values)
        {
            joinAction(stringBuilder, value);
            stringBuilder.Append(separator);
            appended = true;
        }

        if (appended)
        {
            stringBuilder.Length -= separator.Length;
        }

        return stringBuilder;
    }

    public static StringBuilder AppendJoin<T>(
        this StringBuilder stringBuilder,
        IEnumerable<T> values,
        Func<StringBuilder, T, bool> joinFunc,
        string separator = ", ")
    {
        var appended = false;

        foreach (var value in values)
        {
            if (joinFunc(stringBuilder, value))
            {
                stringBuilder.Append(separator);
                appended = true;
            }
        }

        if (appended)
        {
            stringBuilder.Length -= separator.Length;
        }

        return stringBuilder;
    }

    public static StringBuilder AppendJoin<T, TParam>(
        this StringBuilder stringBuilder,
        IEnumerable<T> values,
        TParam param,
        Action<StringBuilder, T, TParam> joinAction,
        string separator = ", ")
    {
        var appended = false;

        foreach (var value in values)
        {
            joinAction(stringBuilder, value, param);
            stringBuilder.Append(separator);
            appended = true;
        }

        if (appended)
        {
            stringBuilder.Length -= separator.Length;
        }

        return stringBuilder;
    }

    public static void AppendBytes(this StringBuilder builder, byte[] bytes)
    {
        builder.Append("'0x");

        for (var i = 0; i < bytes.Length; i++)
        {
            if (i > 31)
            {
                builder.Append("...");
                break;
            }

            builder.Append(bytes[i].ToString("X2", CultureInfo.InvariantCulture));
        }

        builder.Append('\'');
    }
}
