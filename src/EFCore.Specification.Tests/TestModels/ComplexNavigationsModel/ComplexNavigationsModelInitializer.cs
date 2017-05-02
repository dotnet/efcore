// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.ComplexNavigationsModel
{
    public class ComplexNavigationsModelInitializer
    {
        public static void Seed(ComplexNavigationsContext context, bool tableSplitting = false)
        {
            var l1s = ComplexNavigationsData.CreateLevelOnes(tableSplitting);
            var l2s = ComplexNavigationsData.CreateLevelTwos(tableSplitting);
            var l3s = ComplexNavigationsData.CreateLevelThrees(tableSplitting);
            var l4s = ComplexNavigationsData.CreateLevelFours(tableSplitting);

            context.LevelOne.AddRange(l1s);

            ComplexNavigationsData.WireUpPart1(l1s, l2s, l3s, l4s, tableSplitting);

            context.SaveChanges();

            ComplexNavigationsData.WireUpPart2(l1s, l2s, l3s, l4s, tableSplitting);

            var globalizations = new List<ComplexNavigationGlobalization>();
            for (var i = 0; i < 10; i++)
            {
                var language = new ComplexNavigationLanguage { Name = "Language" + i, CultureString = "Foo" + i };
                var globalization = new ComplexNavigationGlobalization { Text = "Globalization" + i, Language = language };
                globalizations.Add(globalization);

                context.Languages.Add(language);
                context.Globalizations.Add(globalization);
            }

            var mls1 = new ComplexNavigationString { DefaultText = "MLS1", Globalizations = globalizations.Take(3).ToList() };
            var mls2 = new ComplexNavigationString { DefaultText = "MLS2", Globalizations = globalizations.Skip(3).Take(3).ToList() };
            var mls3 = new ComplexNavigationString { DefaultText = "MLS3", Globalizations = globalizations.Skip(6).Take(3).ToList() };
            var mls4 = new ComplexNavigationString { DefaultText = "MLS4", Globalizations = globalizations.Skip(9).ToList() };

            context.MultilingualStrings.AddRange(mls1, mls2, mls3, mls4);

            var field1 = new ComplexNavigationField { Name = "Field1", Label = mls1, Placeholder = null };
            var field2 = new ComplexNavigationField { Name = "Field2", Label = mls3, Placeholder = mls4 };

            context.Fields.AddRange(field1, field2);
            context.SaveChanges();
        }
    }
}
