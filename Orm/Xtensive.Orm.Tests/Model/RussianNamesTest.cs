// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2010.02.15

using NUnit.Framework;
using Xtensive.Orm.Tests.Model.RussianNamesTestModel;

namespace Xtensive.Orm.Tests.Model.RussianNamesTestModel
{
  [HierarchyRoot]
  public class Энимал : Entity
  {
    [Field, Key]
    public int Ид { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Model
{
  [TestFixture]
  public class RussianNamesTest
  {
    [Test]
    public void MainTest()
    {
      Require.ProviderIsNot(StorageProvider.MySql | StorageProvider.Firebird);
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof (Энимал).Assembly, typeof (Энимал).Namespace);
      var domain = Domain.Build(config);
      domain.Dispose();
    }
  }
}