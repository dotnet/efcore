// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Compiled;

namespace Microsoft.Data.Entity.FunctionalTests.Metadata
{
    public class KoolEntityBase
    {
        public int Id { get; set; }
        public string Foo { get; set; }
        public Guid Goo { get; set; }
    }

    public class KoolEntity1 : KoolEntityBase
    {
    }

    public class KoolEntity2 : KoolEntityBase
    {
    }

    public class KoolEntity3 : KoolEntityBase
    {
    }

    public class KoolEntity4 : KoolEntityBase
    {
    }

    public class KoolEntity5 : KoolEntityBase
    {
    }

    public class KoolEntity6 : KoolEntityBase
    {
    }

    public class KoolEntity7 : KoolEntityBase
    {
    }

    public class KoolEntity8 : KoolEntityBase
    {
    }

    public class KoolEntity9 : KoolEntityBase
    {
    }

    public class KoolEntity10 : KoolEntityBase
    {
    }

    public class KoolEntity11 : KoolEntityBase
    {
    }

    public class KoolEntity12 : KoolEntityBase
    {
    }

    public class KoolEntity13 : KoolEntityBase
    {
    }

    public class KoolEntity14 : KoolEntityBase
    {
    }

    public class KoolEntity15 : KoolEntityBase
    {
    }

    public class KoolEntity16 : KoolEntityBase
    {
    }

    public class KoolEntity17 : KoolEntityBase
    {
    }

    public class KoolEntity18 : KoolEntityBase
    {
    }

    public class KoolEntity19 : KoolEntityBase
    {
    }

    public class KoolEntity20 : KoolEntityBase
    {
    }

    public class KoolEntity21 : KoolEntityBase
    {
    }

    public class KoolEntity22 : KoolEntityBase
    {
    }

    public class KoolEntity23 : KoolEntityBase
    {
    }

    public class KoolEntity24 : KoolEntityBase
    {
    }

    public class KoolEntity25 : KoolEntityBase
    {
    }

    public class KoolEntity26 : KoolEntityBase
    {
    }

    public class KoolEntity27 : KoolEntityBase
    {
    }

    public class KoolEntity28 : KoolEntityBase
    {
    }

    public class KoolEntity29 : KoolEntityBase
    {
    }

    public class KoolEntity30 : KoolEntityBase
    {
    }

    public class KoolEntity31 : KoolEntityBase
    {
    }

    public class KoolEntity32 : KoolEntityBase
    {
    }

    public class KoolEntity33 : KoolEntityBase
    {
    }

    public class KoolEntity34 : KoolEntityBase
    {
    }

    public class KoolEntity35 : KoolEntityBase
    {
    }

    public class KoolEntity36 : KoolEntityBase
    {
    }

    public class KoolEntity37 : KoolEntityBase
    {
    }

    public class KoolEntity38 : KoolEntityBase
    {
    }

    public class KoolEntity39 : KoolEntityBase
    {
    }

    public class KoolEntity40 : KoolEntityBase
    {
    }

    public class KoolEntity41 : KoolEntityBase
    {
    }

    public class KoolEntity42 : KoolEntityBase
    {
    }

    public class KoolEntity43 : KoolEntityBase
    {
    }

    public class KoolEntity44 : KoolEntityBase
    {
    }

    public class KoolEntity45 : KoolEntityBase
    {
    }

    public class KoolEntity46 : KoolEntityBase
    {
    }

    public class KoolEntity47 : KoolEntityBase
    {
    }

    public class KoolEntity48 : KoolEntityBase
    {
    }

    public class KoolEntity49 : KoolEntityBase
    {
    }

    public class KoolEntity50 : KoolEntityBase
    {
    }

    // Proposed generated code below
// ReSharper disable InconsistentNaming
    public class _OneTwoThreeContextModel : CompiledModel, IModel
    {
        protected override IEntityType[] LoadEntityTypes()
        {
            return new IEntityType[]
                {
                    new _KoolEntity1EntityType(),
                    new _KoolEntity10EntityType(),
                    new _KoolEntity11EntityType(),
                    new _KoolEntity12EntityType(),
                    new _KoolEntity13EntityType(),
                    new _KoolEntity14EntityType(),
                    new _KoolEntity15EntityType(),
                    new _KoolEntity16EntityType(),
                    new _KoolEntity17EntityType(),
                    new _KoolEntity18EntityType(),
                    new _KoolEntity19EntityType(),
                    new _KoolEntity2EntityType(),
                    new _KoolEntity20EntityType(),
                    new _KoolEntity21EntityType(),
                    new _KoolEntity22EntityType(),
                    new _KoolEntity23EntityType(),
                    new _KoolEntity24EntityType(),
                    new _KoolEntity25EntityType(),
                    new _KoolEntity26EntityType(),
                    new _KoolEntity27EntityType(),
                    new _KoolEntity28EntityType(),
                    new _KoolEntity29EntityType(),
                    new _KoolEntity3EntityType(),
                    new _KoolEntity30EntityType(),
                    new _KoolEntity31EntityType(),
                    new _KoolEntity32EntityType(),
                    new _KoolEntity33EntityType(),
                    new _KoolEntity34EntityType(),
                    new _KoolEntity35EntityType(),
                    new _KoolEntity36EntityType(),
                    new _KoolEntity37EntityType(),
                    new _KoolEntity38EntityType(),
                    new _KoolEntity39EntityType(),
                    new _KoolEntity4EntityType(),
                    new _KoolEntity40EntityType(),
                    new _KoolEntity41EntityType(),
                    new _KoolEntity42EntityType(),
                    new _KoolEntity43EntityType(),
                    new _KoolEntity44EntityType(),
                    new _KoolEntity45EntityType(),
                    new _KoolEntity46EntityType(),
                    new _KoolEntity47EntityType(),
                    new _KoolEntity48EntityType(),
                    new _KoolEntity49EntityType(),
                    new _KoolEntity5EntityType(),
                    new _KoolEntity50EntityType(),
                    new _KoolEntity6EntityType(),
                    new _KoolEntity7EntityType(),
                    new _KoolEntity8EntityType(),
                    new _KoolEntity9EntityType()
                };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "ModelAnnotation1", "ModelAnnotation2" },
                new[] { "ModelValue1", "ModelValue2" }).ToArray();
        }
    }

    public class _KoolEntity1EntityType : CompiledEntityType<KoolEntity1>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity1"; }
        }

        public string StorageName
        {
            get { return "KoolEntity1Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity2EntityType : CompiledEntityType<KoolEntity2>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity2"; }
        }

        public string StorageName
        {
            get { return "KoolEntity2Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity3EntityType
        : CompiledEntityType<KoolEntity3>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity3"; }
        }

        public string StorageName
        {
            get { return "KoolEntity3Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity4EntityType
        : CompiledEntityType<KoolEntity4>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity4"; }
        }

        public string StorageName
        {
            get { return "KoolEntity4Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity5EntityType
        : CompiledEntityType<KoolEntity5>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity5"; }
        }

        public string StorageName
        {
            get { return "KoolEntity5Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity6EntityType
        : CompiledEntityType<KoolEntity6>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity6"; }
        }

        public string StorageName
        {
            get { return "KoolEntity6Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity7EntityType
        : CompiledEntityType<KoolEntity7>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity7"; }
        }

        public string StorageName
        {
            get { return "KoolEntity7Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity8EntityType
        : CompiledEntityType<KoolEntity8>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity8"; }
        }

        public string StorageName
        {
            get { return "KoolEntity8Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity9EntityType
        : CompiledEntityType<KoolEntity9>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity9"; }
        }

        public string StorageName
        {
            get { return "KoolEntity9Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity10EntityType
        : CompiledEntityType<KoolEntity10>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity10"; }
        }

        public string StorageName
        {
            get { return "KoolEntity10Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity11EntityType
        : CompiledEntityType<KoolEntity11>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity11"; }
        }

        public string StorageName
        {
            get { return "KoolEntity11Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity12EntityType
        : CompiledEntityType<KoolEntity12>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity12"; }
        }

        public string StorageName
        {
            get { return "KoolEntity12Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity13EntityType
        : CompiledEntityType<KoolEntity13>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity13"; }
        }

        public string StorageName
        {
            get { return "KoolEntity13Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity14EntityType
        : CompiledEntityType<KoolEntity14>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity14"; }
        }

        public string StorageName
        {
            get { return "KoolEntity14Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity15EntityType
        : CompiledEntityType<KoolEntity15>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity15"; }
        }

        public string StorageName
        {
            get { return "KoolEntity15Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity16EntityType
        : CompiledEntityType<KoolEntity16>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity16"; }
        }

        public string StorageName
        {
            get { return "KoolEntity16Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity17EntityType
        : CompiledEntityType<KoolEntity17>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity17"; }
        }

        public string StorageName
        {
            get { return "KoolEntity17Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity18EntityType
        : CompiledEntityType<KoolEntity18>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity18"; }
        }

        public string StorageName
        {
            get { return "KoolEntity18Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity19EntityType
        : CompiledEntityType<KoolEntity19>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity19"; }
        }

        public string StorageName
        {
            get { return "KoolEntity19Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity20EntityType
        : CompiledEntityType<KoolEntity20>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity20"; }
        }

        public string StorageName
        {
            get { return "KoolEntity20Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity21EntityType
        : CompiledEntityType<KoolEntity21>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity21"; }
        }

        public string StorageName
        {
            get { return "KoolEntity21Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity22EntityType
        : CompiledEntityType<KoolEntity22>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity22"; }
        }

        public string StorageName
        {
            get { return "KoolEntity22Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity23EntityType
        : CompiledEntityType<KoolEntity23>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity23"; }
        }

        public string StorageName
        {
            get { return "KoolEntity23Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity24EntityType
        : CompiledEntityType<KoolEntity24>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity24"; }
        }

        public string StorageName
        {
            get { return "KoolEntity24Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity25EntityType
        : CompiledEntityType<KoolEntity25>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity25"; }
        }

        public string StorageName
        {
            get { return "KoolEntity25Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity26EntityType
        : CompiledEntityType<KoolEntity26>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity26"; }
        }

        public string StorageName
        {
            get { return "KoolEntity26Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity27EntityType
        : CompiledEntityType<KoolEntity27>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity27"; }
        }

        public string StorageName
        {
            get { return "KoolEntity27Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity28EntityType
        : CompiledEntityType<KoolEntity28>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity28"; }
        }

        public string StorageName
        {
            get { return "KoolEntity28Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity29EntityType
        : CompiledEntityType<KoolEntity29>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity29"; }
        }

        public string StorageName
        {
            get { return "KoolEntity29Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity30EntityType
        : CompiledEntityType<KoolEntity30>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity30"; }
        }

        public string StorageName
        {
            get { return "KoolEntity30Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity31EntityType
        : CompiledEntityType<KoolEntity31>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity31"; }
        }

        public string StorageName
        {
            get { return "KoolEntity31Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity32EntityType
        : CompiledEntityType<KoolEntity32>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity32"; }
        }

        public string StorageName
        {
            get { return "KoolEntity32Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity33EntityType
        : CompiledEntityType<KoolEntity33>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity33"; }
        }

        public string StorageName
        {
            get { return "KoolEntity33Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity34EntityType
        : CompiledEntityType<KoolEntity34>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity34"; }
        }

        public string StorageName
        {
            get { return "KoolEntity34Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity35EntityType
        : CompiledEntityType<KoolEntity35>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity35"; }
        }

        public string StorageName
        {
            get { return "KoolEntity35Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity36EntityType
        : CompiledEntityType<KoolEntity36>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity36"; }
        }

        public string StorageName
        {
            get { return "KoolEntity36Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity37EntityType
        : CompiledEntityType<KoolEntity37>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity37"; }
        }

        public string StorageName
        {
            get { return "KoolEntity37Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity38EntityType
        : CompiledEntityType<KoolEntity38>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity38"; }
        }

        public string StorageName
        {
            get { return "KoolEntity38Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity39EntityType
        : CompiledEntityType<KoolEntity39>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity39"; }
        }

        public string StorageName
        {
            get { return "KoolEntity39Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity40EntityType
        : CompiledEntityType<KoolEntity40>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity40"; }
        }

        public string StorageName
        {
            get { return "KoolEntity40Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity41EntityType
        : CompiledEntityType<KoolEntity41>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity41"; }
        }

        public string StorageName
        {
            get { return "KoolEntity41Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity42EntityType
        : CompiledEntityType<KoolEntity42>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity42"; }
        }

        public string StorageName
        {
            get { return "KoolEntity42Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity43EntityType
        : CompiledEntityType<KoolEntity43>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity43"; }
        }

        public string StorageName
        {
            get { return "KoolEntity43Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity44EntityType
        : CompiledEntityType<KoolEntity44>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity44"; }
        }

        public string StorageName
        {
            get { return "KoolEntity44Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity45EntityType
        : CompiledEntityType<KoolEntity45>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity45"; }
        }

        public string StorageName
        {
            get { return "KoolEntity45Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity46EntityType
        : CompiledEntityType<KoolEntity46>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity46"; }
        }

        public string StorageName
        {
            get { return "KoolEntity46Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity47EntityType
        : CompiledEntityType<KoolEntity47>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity47"; }
        }

        public string StorageName
        {
            get { return "KoolEntity47Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity48EntityType
        : CompiledEntityType<KoolEntity48>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity48"; }
        }

        public string StorageName
        {
            get { return "KoolEntity48Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity49EntityType
        : CompiledEntityType<KoolEntity49>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity49"; }
        }

        public string StorageName
        {
            get { return "KoolEntity49Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity50EntityType
        : CompiledEntityType<KoolEntity50>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity50"; }
        }

        public string StorageName
        {
            get { return "KoolEntity50Table"; }
        }

        protected override int[] LoadKey()
        {
            return new[] { 2 };
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), new _KoolEntityIdProperty() };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    // The code below is shared by all entity types above to avoid having to have lots of hand-written
    // proposed code checked in. This sharing should not significantly impact functionality or managed
    // heap memory usage, except that there will be more than the usual amount of string interning
    // due to the similarity of all entity types

    public class _KoolEntityIdProperty : CompiledProperty<KoolEntityBase, int>, IProperty
    {
        public string Name
        {
            get { return "Id"; }
        }

        public string StorageName
        {
            get { return "MyKey"; }
        }

        public void SetValue(object instance, object value)
        {
            ((KoolEntityBase)instance).Id = (int)value;
        }

        public object GetValue(object instance)
        {
            return ((KoolEntityBase)instance).Id;
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "IdAnnotation1", "IdAnnotation2" },
                new[] { "IdValue1", "IdValue2" }).ToArray();
        }
    }

    public class _KoolEntityFooProperty : CompiledProperty<KoolEntityBase, string>, IProperty
    {
        public string Name
        {
            get { return "Foo"; }
        }

        public string StorageName
        {
            get { return "Foo"; }
        }

        public void SetValue(object instance, object value)
        {
            ((KoolEntityBase)instance).Foo = (string)value;
        }

        public object GetValue(object instance)
        {
            return ((KoolEntityBase)instance).Foo;
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "FooAnnotation1", "FooAnnotation2" },
                new[] { "FooValue1", "FooValue2" }).ToArray();
        }
    }

    public class _KoolEntityGooProperty : CompiledPropertyNoAnnotations<KoolEntityBase, Guid>, IProperty
    {
        public string Name
        {
            get { return "Goo"; }
        }

        public string StorageName
        {
            get { return "Goo"; }
        }

        public void SetValue(object instance, object value)
        {
            ((KoolEntityBase)instance).Goo = (Guid)value;
        }

        public object GetValue(object instance)
        {
            return ((KoolEntityBase)instance).Goo;
        }
    }

// ReSharper restore InconsistentNaming
}
