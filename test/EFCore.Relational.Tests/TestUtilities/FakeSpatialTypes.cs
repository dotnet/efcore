// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Fake types used to test spatial/HierarchyId type detection in ThrowPropertyNotMappedException.
// These types exist solely to provide CLR types with the correct FullName values
// without requiring a dependency on NetTopologySuite or SqlServer.HierarchyId packages.

// ReSharper disable CheckNamespace
namespace NetTopologySuite.Geometries;

public class FakePoint;
