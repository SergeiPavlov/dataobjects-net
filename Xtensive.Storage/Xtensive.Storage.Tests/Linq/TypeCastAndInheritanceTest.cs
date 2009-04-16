// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.10

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  public class TypeCastAndInheritanceTest : NorthwindDOModelTest
  {
    [Test]
    public void InheritanceCountTest()
    {
      var productCount = Query<Product>.All.Count();
      var discontinuedProductCount = Query<DiscontinuedProduct>.All.Count();
      var activeProductCount = Query<ActiveProduct>.All.Count();
      Assert.IsTrue(productCount > 0);
      Assert.IsTrue(discontinuedProductCount > 0);
      Assert.IsTrue(activeProductCount > 0);
      Assert.AreEqual(productCount, discontinuedProductCount, activeProductCount);
    }

    [Test]
    public void IsSimpleTest()
    {
      var result = Query<Product>.All.Where(p => p is DiscontinuedProduct);
      QueryDumper.Dump(result);
    }

    [Test]
    public void IsSameTypeTest()
    {
#pragma warning disable 183
      var result = Query<Product>.All.Where(p => p is Product);
#pragma warning restore 183
      QueryDumper.Dump(result);
    }

    [Test]
    public void IsSubTypeTest()
    {
#pragma warning disable 183
      var result = Query<DiscontinuedProduct>.All.Where(p => p is Product);
#pragma warning restore 183
      QueryDumper.Dump(result);
    }

    [Test]
    public void IsIntermediateTest()
    {
        Query<Product>.All
          .Where(p => p is IntermediateProduct)
          .Select(product => (IntermediateProduct)product)
          .Count();
    }

    [Test]
    public void IsCountTest()
    {
      int productCount = Query<Product>.All.Count();
      int intermediateProductCount = Query<IntermediateProduct>.All.Count();
      int discontinuedProductCount = Query<DiscontinuedProduct>.All.Count();
      int activeProductCount = Query<ActiveProduct>.All.Count();

      Assert.Greater(productCount, 0);
      Assert.Greater(intermediateProductCount, 0);
      Assert.Greater(discontinuedProductCount, 0);
      Assert.Greater(activeProductCount, 0);

      Assert.AreEqual(
        productCount,
        intermediateProductCount);

      Assert.AreEqual(
        intermediateProductCount,
        Query<Product>.All
          .Where(p => p is IntermediateProduct)
          .Select(product => (IntermediateProduct)product)
          .Count());

      Assert.AreEqual(
        discontinuedProductCount,
        Query<Product>.All
          .Where(p => p is DiscontinuedProduct)
          .Select(product => (DiscontinuedProduct) product)
          .Count());

      Assert.AreEqual(
        activeProductCount,
        Query<Product>.All
          .Where(p => p is ActiveProduct)
          .Select(product => (ActiveProduct) product)
          .Count());

#pragma warning disable 183
      Assert.AreEqual(
        productCount,
        Query<Product>.All
          .Where(p => p is Product)
          .Count());
#pragma warning restore 183
    }

    [Test]
    public void OfTypeSimpleTest()
    {
      var result = Query<Product>.All.OfType<DiscontinuedProduct>();
      QueryDumper.Dump(result);
    }

    [Test]
    public void OfTypeSameTypeTest()
    {
      var result = Query<Product>.All.OfType<Product>();
      QueryDumper.Dump(result);
    }

    [Test]
    public void OfTypeSubTypeTest()
    {
      var result = Query<DiscontinuedProduct>.All.OfType<Product>();
      QueryDumper.Dump(result);
    }

    [Test]
    public void OfTypeIntermediateTest()
    {
      Query<Product>.All
        .OfType<IntermediateProduct>()
        .Count();
    }

    [Test]
    public void OfTypeCountTest()
    {
      int productCount = Query<Product>.All.Count();
      int intermediateProductCount = Query<IntermediateProduct>.All.Count();
      int discontinuedProductCount = Query<DiscontinuedProduct>.All.Count();
      int activeProductCount = Query<ActiveProduct>.All.Count();

      Assert.Greater(productCount, 0);
      Assert.Greater(intermediateProductCount, 0);
      Assert.Greater(discontinuedProductCount, 0);
      Assert.Greater(activeProductCount, 0);

      Assert.AreEqual(
        productCount,
        intermediateProductCount);

      Assert.AreEqual(
        intermediateProductCount,
        Query<Product>.All
          .OfType<IntermediateProduct>()
          .Count());

      Assert.AreEqual(
        discontinuedProductCount,
        Query<Product>.All
          .OfType<DiscontinuedProduct>()
          .Count());

      Assert.AreEqual(
        activeProductCount,
        Query<Product>.All
          .OfType<ActiveProduct>()
          .Count());

      Assert.AreEqual(
        productCount,
        Query<Product>.All
          .OfType<Product>()
          .Count());
    }

    [Test]
    public void CastSimpleTest()
    {
      var discontinuedProducts = Query<DiscontinuedProduct>.All.Cast<Product>();
      AssertEx.ThrowsNotSupportedException(()=>QueryDumper.Dump(discontinuedProducts));
    }


    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void CastCountTest()
    {
      var discontinuedProductCount1 = Query<DiscontinuedProduct>.All.Count();
      var discontinuedProductCount2 = Query<DiscontinuedProduct>.All.Cast<Product>().Where(product => product!=null).Count();
      Assert.AreEqual(discontinuedProductCount1, discontinuedProductCount2);

      var activeProductCount1 = Query<ActiveProduct>.All.Count();
      var activeProductCount2 = Query<ActiveProduct>.All.Cast<Product>().Where(product => product!=null).Count();
      Assert.AreEqual(activeProductCount1, activeProductCount2);

      var productCount1 = Query<Product>.All.Count();
      var productCount2 = Query<Product>.All.Cast<Product>().Count();
      Assert.AreEqual(productCount1, productCount2);
    }

    [Test]
    public void OfTypeGetFieldTest()
    {
      var result = Query<Product>.All
        .OfType<DiscontinuedProduct>()
        .Select(dp => dp.QuantityPerUnit);
      QueryDumper.Dump(result);
    }

    [Test]
    public void IsGetFieldTest()
    {
      var result = Query<Product>.All
        .Where(p => p is DiscontinuedProduct)
        .Select(x => (DiscontinuedProduct) x)
        .Select(dp => dp.QuantityPerUnit);
      QueryDumper.Dump(result);
    }
  }
}