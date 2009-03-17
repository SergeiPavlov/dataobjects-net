// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.02

using NUnit.Framework;

namespace Xtensive.Sql.Dom.Tests.MsSql
{
  [TestFixture]
  public class DateTimeIntervalTest : Tests.DateTimeIntervalTest
  {
    protected override string Url { get { return TestUrl.MsSql2005; } }

    [Test]
    public new void DateTimeSubtractIntervalTest()
    {
      Assert.Ignore("MSSQL DateTime precision issue");
    }
  }
}
