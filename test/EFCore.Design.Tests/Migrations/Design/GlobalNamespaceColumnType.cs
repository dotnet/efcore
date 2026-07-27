// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// This type is intentionally declared in the global namespace to test that the
// migrations code generator does not emit an invalid empty `using ;` statement
// for types that have no namespace.
public enum GlobalNamespaceColumnType
{
    Default,
    Other
}
