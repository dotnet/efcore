// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Fake types used to test HierarchyId type detection in ThrowPropertyNotMappedException.
// These types exist solely to provide CLR types with the correct FullName values
// without requiring a dependency on the SqlServer.HierarchyId package.

// ReSharper disable CheckNamespace
namespace Microsoft.SqlServer.Types;

public class SqlHierarchyId;
