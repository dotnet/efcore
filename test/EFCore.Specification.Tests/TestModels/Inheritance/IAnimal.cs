// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels.Inheritance
{
    public interface IAnimal
    {
        int CountryId { get; set; }
        string Name { get; set; }
        string Species { get; set; }
    }
}