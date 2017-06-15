// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public interface ICSharpUtilities
    {
        string DelimitString([NotNull] string value);
        string EscapeString([NotNull] string str);
        string EscapeVerbatimString([NotNull] string str);
        string GenerateCSharpIdentifier([NotNull] string identifier, [CanBeNull] ICollection<string> existingIdentifiers, [CanBeNull] Func<string, string> singularizePluralizer);
        string GenerateCSharpIdentifier([NotNull] string identifier, [CanBeNull] ICollection<string> existingIdentifiers, [CanBeNull] Func<string, string> singularizePluralizer, [NotNull] Func<string, ICollection<string>, string> uniquifier);
        string GenerateLiteral(bool value);
        string GenerateLiteral([NotNull] byte[] value);
        string GenerateLiteral(DateTime value);
        string GenerateLiteral(DateTimeOffset value);
        string GenerateLiteral(decimal value);
        string GenerateLiteral(double value);
        string GenerateLiteral(float value);
        string GenerateLiteral(Guid value);
        string GenerateLiteral(int value);
        string GenerateLiteral(long value);
        string GenerateLiteral([NotNull] object value);
        string GenerateLiteral([NotNull] string value);
        string GenerateLiteral(TimeSpan value);
        string GenerateVerbatimStringLiteral([NotNull] string value);
        string GetTypeName([NotNull] Type type);
        bool IsCSharpKeyword([NotNull] string identifier);
        bool IsValidIdentifier([CanBeNull] string name);
        string Uniquifier([NotNull] string proposedIdentifier, [CanBeNull] ICollection<string> existingIdentifiers);
    }
}