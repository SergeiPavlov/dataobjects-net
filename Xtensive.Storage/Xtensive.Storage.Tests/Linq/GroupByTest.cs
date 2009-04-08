// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.04

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class GroupByTest : NorthwindDOModelTest
  {
    [Test]
    public void EntityGroupTest()
    {
      var result = Query<Product>.All.GroupBy(p => p.Category);
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
      foreach (IGrouping<Category, Product> products in list)
        Assert.Greater(products.ToList().Count, 0);
    }

    [Test]
    public void EntityKeyGroupTest()
    {
      var result = Query<Product>.All.GroupBy(p => p.Category.Key);
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
      foreach (var grouping in list) {
        Assert.IsNotNull(grouping.Key);
        Assert.Greater(grouping.Count(), 0);
      }
    }

    [Test]
    public void EntityFieldGroupTest()
    {
      var result = Query<Product>.All.GroupBy(p => p.Category.CategoryName);
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
      foreach (var grouping in list) {
        Assert.IsNotNull(grouping.Key);
        Assert.Greater(grouping.Count(), 0);
      }
    }

    [Test]
    public void StructureGroupTest()
    {
      var result = Query<Customer>.All.GroupBy(p => p.Address);
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
      foreach (var grouping in list) {
        Assert.IsNotNull(grouping.Key);
        Assert.Greater(grouping.Count(), 0);
      }
    }


    [Test]
    public void StructureGroupWithNullFieldTest()
    {
      var customer = Query<Customer>.All.First();
      customer.Address.Country = null;
      Session.Current.Persist();
      StructureGroupTest();
    }

    [Test]
    public void AnonimousTypeGroupTest()
    {
      var result = Query<Customer>.All.GroupBy(c => new {c.Address.City, c.Address.Country});
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
      foreach (var grouping in list) {
        Assert.IsNotNull(grouping.Key);
        Assert.Greater(grouping.Count(), 0);
      }
    }

    [Test]
    public void AnonimousTypeEntityAndStructureGroupTest()
    {
      var result = Query<Employee>.All.GroupBy(e => new
                                                    {
//          e.Address.City,
                                                      e.Address,
                                                    });
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
      foreach (var grouping in list) {
        Assert.IsNotNull(grouping.Key);
        Assert.Greater(grouping.Count(), 0);
      }
    }

    [Test]
    public void AnonymousStructureGroupTest()
    {
      var result = Query<Customer>.All.GroupBy(p => new {p.Address});
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
      foreach (var grouping in list) {
        Assert.IsNotNull(grouping.Key);
        Assert.Greater(grouping.Count(), 0);
      }
    }

    [Test]
    public void AnonimousTypeEntityAndFieldTest()
    {
      var result = Query<Product>.All.GroupBy(product => new
                                                         {
                                                           product.Category,
                                                           product.Category.CategoryName,
                                                         });
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
      foreach (var grouping in list) {
        Assert.IsNotNull(grouping.Key);
        int count = 0;
        foreach (Product product in grouping) {
          count++;
        }
        Assert.Greater(count, 0);
      }
    }

    [Test]
    public void AnonimousTypeEntityGroupTest()
    {
      var result = Query<Product>.All.GroupBy(product => new
                                                         {
                                                           product.Category,
                                                         });
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
      foreach (var grouping in list) {
        Assert.IsNotNull(grouping.Key);
        int count = 0;
        foreach (Product product in grouping) {
          count++;
        }
        Assert.Greater(count, 0);
      }
    }


    [Test]
    public void DefaultTest()
    {
      var result = Query<Customer>.All.GroupBy(c => c.Address.City);
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
      foreach (var grouping in list) {
        Assert.IsNotNull(grouping.Key);
        Assert.Greater(grouping.Count(), 0);
      }
    }


    [Test]
    public void FilterGroupingByKeyTest()
    {
      var result = Query<Order>.All.GroupBy(o => o.ShippingAddress.City).Where(g => g.Key.StartsWith("L"));
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void FilterGroupingByAgregateTest()
    {
      var result = Query<Order>.All.GroupBy(o => o.ShippingAddress.City).Where(g => g.Count() > 1);
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }


    [Test]
    public void FilterGroupingByAgregateTest2()
    {
      var result = Query<Order>.All.GroupBy(o => o.ShippingAddress.City).Where(g => g.Sum(ord => ord.Freight) > 10);
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void GroupByWhereTest()
    {
      var result = Query<Order>.All.GroupBy(o => o.ShippingAddress.City).Where(g => g.Key.StartsWith("L") && g.Count() > 1);
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void GroupBySelectTest()
    {
      IQueryable<IGrouping<string, Order>> result = Query<Order>.All.GroupBy(o => o.ShipName).Select(g => g);
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }


    [Test]
    public void GroupBySelectWithAnonymousTest()
    {
      var result = Query<Order>.All.GroupBy(o => o.ShipName).Select(g => new { g });
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }


    [Test]
    public void GroupBySelectKeyTest()
    {
      IQueryable<string> result = Query<Order>.All.GroupBy(o => o.ShipName).Select(g => g.Key);
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void GroupBySelectKeyWithSelectCalculableColumnTest()
    {
      IQueryable<string> result = Query<Order>.All.GroupBy(o => o.ShipName).Select(g => g.Key + "aaa");
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }


    [Test]
    public void GroupBySelectKeyWithCalculableColumnTest()
    {
      IQueryable<IGrouping<string, Order>> result = Query<Order>.All.GroupBy(o => o.ShipName + "aaa");
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void GroupByWithSelectFirstTest()
    {
      var result = Query<Order>.All.GroupBy(o => o.Customer).Select(g => g.First());
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void GroupBySelectGroupingTest()
    {
      var result = Query<Order>.All.GroupBy(o => o.Customer).Select(g => g);
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void GroupBySelectManyTest()
    {
      var result = Query<Customer>.All.GroupBy(c => c.Address.City).SelectMany(g => g);
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void GroupBySelectManyTest2()
    {
      var result = Query<Order>.All.GroupBy(o => o.Customer).SelectMany(g => g);
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void GroupBySumTest()
    {
      var result = Query<Order>.All.GroupBy(o => o.Customer.Id).Select(g => g.Sum(o => o.Freight));
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void GroupByCountTest()
    {
      var result = Query<Order>.All.GroupBy(o => o.Customer).Select(g => new {Customer = g.Key, OrdersCount = g.Count()});
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void GroupByWithFilterTest()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer)
        .Select(g => new {Customer = g.Key, Orders = g.Where(order => order.OrderDate < DateTime.Now)});
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void GroupBySumMinMaxAvgTest()
    {
      var result = Query<Order>.All.GroupBy(o => o.Customer).Select(g =>
        new
        {
          Sum = g.Sum(o => o.Freight),
          Min = g.Min(o => o.Freight),
          Max = g.Max(o => o.Freight),
          Avg = g.Average(o => o.Freight)
        });
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void GroupByWithConstantResultSelectorTest()
    {
      var result = Query<Order>.All.GroupBy(o => o.Customer, (c, g) =>
        new
        {
          ConstString = "ConstString"
        });
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }


    [Test]
    public void GroupByWithAnonymousSelectTest()
    {
      IQueryable<IGrouping<Customer, Order>> groupings = Query<Order>.All.GroupBy(o => o.Customer);
      var result = groupings.Select(g => new { g });
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void GroupByWithEntityResultSelector2Test()
    {
      var result = Query<Order>.All.GroupBy(o => o.Customer, (c, g) =>
        new
        {
          Customer = c,
        });
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }


    [Test]
    public void GroupByWithEntityResultSelector3Test()
    {
      IQueryable<IEnumerable<Order>> result = Query<Order>.All.GroupBy(o => o.Customer, (c, g) => g);
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void BooleanTest()
    {
      var result = Query<Customer>.All
        .GroupBy(customer => customer.Orders.Average(o => o.Freight) >= 80);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithEntityResultSelector4Test()
    {
      IQueryable<int> result = Query<Order>.All.GroupBy(o => o.Freight, (c, g) => g.Count());
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }


    [Test]
    public void GroupByWithEntityResultSelector5Test()
    {
      var result = Query<Order>.All.GroupBy(o => o.Customer, (c, g) => new{ Count = g.Count(),Customer = c});
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }


    [Test]
    public void GroupByWithEntityResultSelector5BisTest()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Freight)
        .Select(g => new { Count = g.Count(), Freight = g.Key });
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void GroupByWithEntityResultSelector5Bis2Test()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer)
        .Select(g => new { Count = g.Count(), Customer = g.Key });
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void GroupByWithEntityResultSelector5Bis21Test()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer)
        .Select(g => new { Count = new {Count1 = g.Count(), Count2 = g.Count()}, Customer = g.Key} );
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }


    [Test]
    public void GroupByWithEntityResultSelector5Bis3Test()
    {
      var result = Query<Order>.All.GroupBy(o => new { o.OrderDate, o.Freight }).Select(g => new { Count = g.Count(), Customer = g.Key });
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void GroupByWithEntityResultSelector5Bis4Test()
    {
      var result = Query<Order>.All.Select(o => new{o.OrderDate, o.Freight}).Select(g => g.OrderDate);
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void GroupByWithResultSelectorTest2()
    {
      var result = Query<Order>.All.GroupBy(o => o.Customer, (c, g) =>
        new
        {
//          Customer = c,
          Sum = g.Sum(o => o.Freight),
          Min = g.Min(o => o.Freight),
          Max = g.Max(o => o.Freight),
          Avg = g.Average(o => o.Freight)
        });
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void GroupByWithElementSelectorSumTest()
    {
      IQueryable<decimal> result = Query<Order>.All.GroupBy(o => o.Customer, o => o.Freight).Select(g => g.Sum());
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void GroupByWithElementSelectorSumAnonymousTest()
    {
      var result = Query<Order>.All.GroupBy(o => o.Customer, o => o.Freight).Select(g => new {A = g.Sum(), B = g.Sum()});
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void GroupByWithElementSelectorTest()
    {
      IQueryable<IGrouping<Customer, decimal>> result = Query<Order>.All.GroupBy(o => o.Customer, o => o.Freight);
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void GroupByWithElementSelectorSumMaxTest()
    {
      var result = Query<Order>.All.GroupBy(o => o.Customer.Id, o => o.Freight).Select(g => new {Sum = g.Sum(), Max = g.Max()});
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void GroupByWithAnonymousElementTest()
    {
      var result = Query<Order>.All.GroupBy(o => o.Customer, o => new {o.Freight}).Select(g => g.Sum(x => x.Freight));
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void GroupByWithTwoPartKeyTest()
    {
      var result = Query<Order>.All.GroupBy(o => new {o.Customer.Id, o.OrderDate}).Select(g => g.Sum(o => o.Freight));
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void OrderByGroupByTest()
    {
      // NOTE: order-by is lost when group-by is applied (the sequence of groups is not ordered)
      var result = Query<Order>.All.OrderBy(o => o.OrderDate).GroupBy(o => o.Customer.Id).Select(g => g.Sum(o => o.Freight));
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void OrderByGroupBySelectManyTest()
    {
      // NOTE: order-by is preserved within grouped sub-collections
      var result = Query<Order>.All.OrderBy(o => o.OrderDate).GroupBy(o => o.Customer.Id).SelectMany(g => g);
      QueryDumper.Dump(result);
      Assert.Greater(result.ToList().Count, 0);
    }
  }
}