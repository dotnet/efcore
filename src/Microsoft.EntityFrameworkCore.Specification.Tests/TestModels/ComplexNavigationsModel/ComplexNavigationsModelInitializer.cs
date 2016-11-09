// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.ComplexNavigationsModel
{
    public class ComplexNavigationsModelInitializer
    {
        public static void Seed(ComplexNavigationsContext context)
        {
            var l1s = ComplexNavigationsData.CreateLevelOnes();
            var l2s = ComplexNavigationsData.CreateLevelTwos();
            var l3s = ComplexNavigationsData.CreateLevelThrees();
            var l4s = ComplexNavigationsData.CreateLevelFours();

            context.LevelOne.AddRange(l1s);
            context.LevelTwo.AddRange(l2s);
            context.LevelThree.AddRange(l3s);
            context.LevelFour.AddRange(l4s);

            ComplexNavigationsData.WireUpPart1(l1s, l2s, l3s, l4s);

            context.SaveChanges();

            ComplexNavigationsData.WireUpPart2(l1s, l2s, l3s, l4s);

            context.SaveChanges();

            for (var i = 0; i < 10; i++)
            {
                var language = new ComplexNavigationLanguage { Name = "Language" + i, CultureString = "Foo" + i };

                context.Languages.Add(language);
            }

            context.SaveChanges();

            var ii = 0;
            foreach (var l in context.Languages)
            {
                var globalization = new ComplexNavigationGlobalization { Text = "Globalization" + ii, Language = l };
                ii++;

                context.Globalizations.Add(globalization);
            }

            context.SaveChanges();

            var globalizations = context.Globalizations.ToList();

            var mls1 = new ComplexNavigationString { DefaultText = "MLS1", Globalizations = globalizations.Take(3).ToList() };
            var mls2 = new ComplexNavigationString { DefaultText = "MLS2", Globalizations = globalizations.Skip(3).Take(3).ToList() };
            var mls3 = new ComplexNavigationString { DefaultText = "MLS3", Globalizations = globalizations.Skip(6).Take(3).ToList() };
            var mls4 = new ComplexNavigationString { DefaultText = "MLS4", Globalizations = globalizations.Skip(9).ToList() };

            context.MultilingualStrings.AddRange(mls1, mls2, mls3, mls4);
            context.SaveChanges();

            var field1 = new ComplexNavigationField { Name = "Field1", Label = mls1, Placeholder = null };
            var field2 = new ComplexNavigationField { Name = "Field2", Label = mls3, Placeholder = mls4 };

            context.Fields.AddRange(field1, field2);
            context.SaveChanges();
        }
    }
}
