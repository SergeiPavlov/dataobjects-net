// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.08.13

using System.Reflection;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Core.Testing;
using System;

#region Models

namespace Xtensive.Storage.Tests.Model.VersionInfoTests.InvalidModel1
{
  [HierarchyRoot]
  public class Parent : Entity
  {
    [Key, Field]
    public int Id { get; private set; }
 
    [Field, Version]
    public string ParentVersionField { get; set; }
  }

  public class Child : Parent
  {
    [Field, Version]
    public string ChildVersionField { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Model.VersionInfoTests.InvalidModel2
{
  [HierarchyRoot]
  public class Parent : Entity
  {
    [Key, Field, Version]
    public int Id { get; private set; }
  }
}

namespace Xtensive.Storage.Tests.Model.VersionInfoTests.InvalidModel3
{
  [HierarchyRoot]
  public class Parent : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(LazyLoad = true), Version]
    public string ReferenceField { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Model.VersionInfoTests.ValidModel
{
  [HierarchyRoot]
  public class Parent : Entity
  {
    [Key, Field]
    public int Id { get; private set; }
 
    [Field, Version]
    public string ParentVersionField { get; set; }

    [Field]
    public string ParentNonVersionField { get; set; }
  }

  public class Child : Parent
  {
    [Field]
    public string ChildNonVersionField { get; set; }
  }

  [HierarchyRoot]
  public class Simple : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string NonLazyField1 { get; set; }

    [Field]
    public int NonLazyField2 { get; set; }

    [Field]
    public SimpleStructure StructureField { get; set; }

    [Field(LazyLoad = true)]
    public string LazyField { get; set; }

    [Field]
    public Simple ReferenceField { get; set; }

    [Field]
    public EntitySet<Simple> CollectionField { get; private set; }

    [Field]
    public byte[] ByteArrayField { get; set; }
  }

  public class SimpleStructure : Structure
  {
    [Field]
    public string NonLazyField { get; set; }

    [Field(LazyLoad = true)]
    public string LazyField { get; set; }

    [Field]
    public Simple ReferenceField { get; set; }
  }
}

#endregion

namespace Xtensive.Storage.Tests.Model
{
  using VersionInfoTests.ValidModel;

  [TestFixture]
  public class VersionInfoTest
  {
    public Domain BuildDomain(string @namespace)
    {
      var configuration = DomainConfigurationFactory.Create("mssql2005");
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(Assembly.GetExecutingAssembly(), @namespace);
      return Domain.Build(configuration);
    }

    [Test]
    public void RootOnlyVersionTest()
    {
      AssertEx.Throws<DomainBuilderException>(() => 
        BuildDomain("Xtensive.Storage.Tests.Model.VersionInfoTests.InvalidModel1"));
    }

    [Test]
    public void DenyKeyFieldsTest()
    {
      AssertEx.Throws<DomainBuilderException>(() => 
        BuildDomain("Xtensive.Storage.Tests.Model.VersionInfoTests.InvalidModel2"));
    }

    [Test]
    public void DenyLazyLoadFieldsTest()
    {
      AssertEx.Throws<DomainBuilderException>(() => 
        BuildDomain("Xtensive.Storage.Tests.Model.VersionInfoTests.InvalidModel3"));
    }

    [Test]
    public void VersionFieldsTest()
    {
      var domain = BuildDomain("Xtensive.Storage.Tests.Model.VersionInfoTests.ValidModel");
      var model = domain.Model;

      var parentType = model.Types[typeof (Parent)];
      Assert.IsTrue(parentType.GetVersionFields().Any(field => field==parentType.Fields["ParentVersionField"]));
      Assert.IsFalse(parentType.GetVersionFields().Any(field => field==parentType.Fields["ParentNonVersionField"]));
      
      var childType = model.Types[typeof (Child)];
      Assert.IsTrue(childType.GetVersionFields().Any(field => field==childType.Fields["ParentVersionField"]));
      Assert.IsFalse(childType.GetVersionFields().Any(field => field==childType.Fields["ParentNonVersionField"]));
      Assert.IsFalse(childType.GetVersionFields().Any(field => field==childType.Fields["ChildNonVersionField"]));
      
      var simpleType = model.Types[typeof (Simple)];
      Assert.IsTrue(simpleType.GetVersionColumns().Any(pair => pair.First.Field==simpleType.Fields["NonLazyField1"]));
      Assert.IsTrue(simpleType.GetVersionColumns().Any(pair => pair.First.Field==simpleType.Fields["NonLazyField2"]));
      Assert.IsTrue(simpleType.GetVersionColumns().Any(pair => pair.First.Field==simpleType.Fields["ReferenceField.Id"]));
      Assert.IsFalse(simpleType.GetVersionColumns().Any(pair => pair.First.Field==simpleType.Fields["Id"]));
      Assert.IsFalse(simpleType.GetVersionColumns().Any(pair => pair.First.Field==simpleType.Fields["TypeId"]));
      Assert.IsFalse(simpleType.GetVersionColumns().Any(pair => pair.First.Field==simpleType.Fields["LazyField"]));
      Assert.IsFalse(simpleType.GetVersionColumns().Any(pair => pair.First.Field==simpleType.Fields["CollectionField"]));
      Assert.IsFalse(simpleType.GetVersionColumns().Any(pair => pair.First.Field==simpleType.Fields["StructureField"]));
      Assert.IsTrue(simpleType.GetVersionColumns().Any(pair => pair.First.Field==simpleType.Fields["StructureField.NonLazyField"]));
      Assert.IsFalse(simpleType.GetVersionColumns().Any(pair => pair.First.Field==simpleType.Fields["StructureField.LazyField"]));
      Assert.IsTrue(simpleType.GetVersionColumns().Any(pair => pair.First.Field==simpleType.Fields["StructureField.ReferenceField.Id"]));
      Assert.IsFalse(simpleType.GetVersionColumns().Any(pair => pair.First.Field==simpleType.Fields["ByteArrayField"]));
    }
    
    [Test]
    public void SerializeVersionInfoTest()
    {
      var domain = BuildDomain("Xtensive.Storage.Tests.Model.VersionInfoTests.ValidModel");
      Key key;
      VersionInfo version;

      using (var session = Session.Open(domain)) {
        using (var transactionScope = Transaction.Open()) {
          var instance = new Simple();
          instance.NonLazyField1 = "Value";
          instance.NonLazyField2 = 123;
          instance.StructureField = new SimpleStructure {NonLazyField = "Value"};
          instance.ReferenceField = instance;
          version = instance.VersionInfo;
          transactionScope.Complete();
        }
      }

      Assert.IsFalse(version.IsVoid);
      var versionClone = (VersionInfo) LegacyBinarySerializer.Instance.Clone(version);
      Assert.IsFalse(versionClone.IsVoid);
      Assert.IsTrue(version.Equals(versionClone));
    }
  }
}