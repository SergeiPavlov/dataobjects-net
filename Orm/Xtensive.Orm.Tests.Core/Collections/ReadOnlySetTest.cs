// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using System;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;

namespace Xtensive.Orm.Tests.Core.Collections
{
  [TestFixture]
  public class ReadOnlySetTest
  {
    private ISet<string> set;
    private ReadOnlySet<string> readOnlySet;

    [OneTimeSetUp]
    public void Init()
    {
      set = new SetSlim<string>();
      readOnlySet = new ReadOnlySet<string>(set);
    }

    [Test]
    public void Test_Add()
    {
      Assert.Throws<NotSupportedException>(() => {
        int count = readOnlySet.Count;
        try {
          readOnlySet.Add("Element");
        }
        finally {
          Assert.AreEqual(count, readOnlySet.Count);
        }
      });
    }

    [Test]
    public void Test_Remove()
    {
      Assert.Throws<NotSupportedException>(() => {
        set.Add("Element");
        int count = readOnlySet.Count;
        try {
          readOnlySet.Remove("Element");
        }
        finally {
          Assert.AreEqual(count, readOnlySet.Count);
        }
      });

    }
    
    [Test]
    public void CopyToTest()
    {
      set.Clear();
      set.Add("Element");
      set.Add("Element");
      set.Add("Element 2");
      string[] array = new string[3];
      readOnlySet.CopyTo(array, 1);
      Assert.IsTrue(array[0]==null);
      if (array[1]=="Element")
        Assert.IsTrue(array[2]=="Element 2");
      else
        Assert.IsTrue(array[2]=="Element");
    }

    [Test]
    public void SerializationTest()
    {
      var deserialized = Cloner.Clone(readOnlySet);
      Assert.AreEqual(deserialized.Count, readOnlySet.Count);
      foreach (string s in readOnlySet)
        Assert.IsTrue(deserialized.Contains(s));
      foreach (string s in deserialized)
        Assert.IsTrue(readOnlySet.Contains(s));
    }
  }
}