// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata.Compiled;

namespace Microsoft.Data.Entity.Metadata
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

    public class _KoolEntity1EntityType : CompiledEntity<KoolEntity1>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity1"; }
        }

        public string StorageName
        {
            get { return "KoolEntity1Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity1, int>(((KoolEntity1)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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

    public class _KoolEntity2EntityType : CompiledEntity<KoolEntity2>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity2"; }
        }

        public string StorageName
        {
            get { return "KoolEntity2Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity2, int>(((KoolEntity2)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity3>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity3"; }
        }

        public string StorageName
        {
            get { return "KoolEntity3Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity3, int>(((KoolEntity3)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity4>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity4"; }
        }

        public string StorageName
        {
            get { return "KoolEntity4Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity4, int>(((KoolEntity4)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity5>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity5"; }
        }

        public string StorageName
        {
            get { return "KoolEntity5Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity5, int>(((KoolEntity5)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity6>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity6"; }
        }

        public string StorageName
        {
            get { return "KoolEntity6Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity6, int>(((KoolEntity6)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity7>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity7"; }
        }

        public string StorageName
        {
            get { return "KoolEntity7Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity7, int>(((KoolEntity7)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity8>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity8"; }
        }

        public string StorageName
        {
            get { return "KoolEntity8Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity8, int>(((KoolEntity8)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity9>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity9"; }
        }

        public string StorageName
        {
            get { return "KoolEntity9Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity9, int>(((KoolEntity9)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity10>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity10"; }
        }

        public string StorageName
        {
            get { return "KoolEntity10Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity10, int>(((KoolEntity10)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity11>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity11"; }
        }

        public string StorageName
        {
            get { return "KoolEntity11Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity11, int>(((KoolEntity11)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity12>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity12"; }
        }

        public string StorageName
        {
            get { return "KoolEntity12Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity12, int>(((KoolEntity12)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity13>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity13"; }
        }

        public string StorageName
        {
            get { return "KoolEntity13Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity13, int>(((KoolEntity13)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity14>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity14"; }
        }

        public string StorageName
        {
            get { return "KoolEntity14Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity14, int>(((KoolEntity14)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity15>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity15"; }
        }

        public string StorageName
        {
            get { return "KoolEntity15Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity15, int>(((KoolEntity15)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity16>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity16"; }
        }

        public string StorageName
        {
            get { return "KoolEntity16Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity16, int>(((KoolEntity16)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity17>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity17"; }
        }

        public string StorageName
        {
            get { return "KoolEntity17Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity17, int>(((KoolEntity17)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity18>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity18"; }
        }

        public string StorageName
        {
            get { return "KoolEntity18Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity18, int>(((KoolEntity18)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity19>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity19"; }
        }

        public string StorageName
        {
            get { return "KoolEntity19Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity19, int>(((KoolEntity19)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity20>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity20"; }
        }

        public string StorageName
        {
            get { return "KoolEntity20Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity20, int>(((KoolEntity20)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity21>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity21"; }
        }

        public string StorageName
        {
            get { return "KoolEntity21Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity21, int>(((KoolEntity21)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity22>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity22"; }
        }

        public string StorageName
        {
            get { return "KoolEntity22Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity22, int>(((KoolEntity22)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity23>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity23"; }
        }

        public string StorageName
        {
            get { return "KoolEntity23Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity23, int>(((KoolEntity23)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity24>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity24"; }
        }

        public string StorageName
        {
            get { return "KoolEntity24Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity24, int>(((KoolEntity24)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity25>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity25"; }
        }

        public string StorageName
        {
            get { return "KoolEntity25Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity25, int>(((KoolEntity25)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity26>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity26"; }
        }

        public string StorageName
        {
            get { return "KoolEntity26Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity26, int>(((KoolEntity26)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity27>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity27"; }
        }

        public string StorageName
        {
            get { return "KoolEntity27Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity27, int>(((KoolEntity27)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity28>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity28"; }
        }

        public string StorageName
        {
            get { return "KoolEntity28Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity28, int>(((KoolEntity28)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity29>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity29"; }
        }

        public string StorageName
        {
            get { return "KoolEntity29Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity29, int>(((KoolEntity29)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity30>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity30"; }
        }

        public string StorageName
        {
            get { return "KoolEntity30Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity30, int>(((KoolEntity30)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity31>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity31"; }
        }

        public string StorageName
        {
            get { return "KoolEntity31Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity31, int>(((KoolEntity31)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity32>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity32"; }
        }

        public string StorageName
        {
            get { return "KoolEntity32Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity32, int>(((KoolEntity32)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity33>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity33"; }
        }

        public string StorageName
        {
            get { return "KoolEntity33Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity33, int>(((KoolEntity33)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity34>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity34"; }
        }

        public string StorageName
        {
            get { return "KoolEntity34Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity34, int>(((KoolEntity34)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity35>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity35"; }
        }

        public string StorageName
        {
            get { return "KoolEntity35Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity35, int>(((KoolEntity35)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity36>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity36"; }
        }

        public string StorageName
        {
            get { return "KoolEntity36Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity36, int>(((KoolEntity36)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity37>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity37"; }
        }

        public string StorageName
        {
            get { return "KoolEntity37Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity37, int>(((KoolEntity37)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity38>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity38"; }
        }

        public string StorageName
        {
            get { return "KoolEntity38Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity38, int>(((KoolEntity38)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity39>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity39"; }
        }

        public string StorageName
        {
            get { return "KoolEntity39Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity39, int>(((KoolEntity39)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity40>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity40"; }
        }

        public string StorageName
        {
            get { return "KoolEntity40Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity40, int>(((KoolEntity40)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity41>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity41"; }
        }

        public string StorageName
        {
            get { return "KoolEntity41Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity41, int>(((KoolEntity41)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity42>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity42"; }
        }

        public string StorageName
        {
            get { return "KoolEntity42Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity42, int>(((KoolEntity42)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity43>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity43"; }
        }

        public string StorageName
        {
            get { return "KoolEntity43Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity43, int>(((KoolEntity43)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity44>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity44"; }
        }

        public string StorageName
        {
            get { return "KoolEntity44Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity44, int>(((KoolEntity44)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity45>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity45"; }
        }

        public string StorageName
        {
            get { return "KoolEntity45Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity45, int>(((KoolEntity45)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity46>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity46"; }
        }

        public string StorageName
        {
            get { return "KoolEntity46Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity46, int>(((KoolEntity46)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity47>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity47"; }
        }

        public string StorageName
        {
            get { return "KoolEntity47Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity47, int>(((KoolEntity47)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity48>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity48"; }
        }

        public string StorageName
        {
            get { return "KoolEntity48Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity48, int>(((KoolEntity48)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity49>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity49"; }
        }

        public string StorageName
        {
            get { return "KoolEntity49Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity49, int>(((KoolEntity49)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
        : CompiledEntity<KoolEntity50>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity50"; }
        }

        public string StorageName
        {
            get { return "KoolEntity50Table"; }
        }

        public EntityKey CreateEntityKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity50, int>(((KoolEntity50)entity).Id);
        }

        protected override string[] LoadKeys()
        {
            return new[] { "Id" };
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
