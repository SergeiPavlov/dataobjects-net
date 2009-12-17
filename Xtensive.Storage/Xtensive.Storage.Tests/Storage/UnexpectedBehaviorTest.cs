// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.11

using System;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.UnexpectedBehaviorTestModel;

namespace Xtensive.Storage.Tests.Storage.UnexpectedBehaviorTestModel
{
  [HierarchyRoot, KeyGenerator(KeyGeneratorKind.None)]
  public class UncreatableEntity : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    public UncreatableEntity(int id)
      : base(id)
    {
      throw new InvalidOperationException("Epic deferred success!");
    }
  }
}

namespace Xtensive.Storage.Tests.Storage
{
  public class UnexpectedBehaviorTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (UncreatableEntity).Assembly, typeof (UncreatableEntity).Namespace);
      return configuration;
    }

    [Test]
    public void ExceptionInCtorTest()
    {
      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        try {
          new UncreatableEntity(42);
        }
        catch {
        }
        var wtf = Query.SingleOrDefault<UncreatableEntity>(42);
        Assert.IsNull(wtf);
      }
    }
  }
}