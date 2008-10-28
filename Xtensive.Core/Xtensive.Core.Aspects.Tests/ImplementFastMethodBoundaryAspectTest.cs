// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.26

using System;
using NUnit.Framework;
using Xtensive.Core.Aspects.Helpers;

namespace Xtensive.Core.Aspects.Tests
{
  [TestFixture]
  public class ImplementFastMethodBoundaryAspectTest
  {
    [AttributeUsage(AttributeTargets.Method |AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    [Serializable]
    internal class LogMethodAspect : ImplementFastMethodBoundaryAspect
    {
      public override object OnEntry(object instance)
      {
        Log.Info("OnEntry called.");
        return "OnEntry";
      }

      public override void OnExit(object instance, object onEntryResult)
      {
        Log.Info("OnExit called.");
        Log.Info(string.Format("OnEntry result: {0}", onEntryResult));
      }

      public override void OnSuccess(object instance, object onEntryResult)
      {
        Log.Info("OnSuccess called.");
      }

      public override bool OnError(object instance, Exception e)
      {
        Log.Error(e);
        return false;
      }
    }

    class BaseClass<T>
    {
      public virtual string AspectedMethod(T value)
      {
        throw new NotImplementedException();
      }

      public BaseClass()
      {
        Log.Info("Base ctor is called.");
      }
    }

    class TestClass : BaseClass<int>
    {
      [LogMethodAspect]
      public override string AspectedMethod(int value)
      {
        return value.ToString();
      }

      [LogMethodAspect]
      public string Method(int value)
      {
        return value.ToString();
      }

      [LogMethodAspect]
      public string Method(string value)
      {
        return value;
      }

      [LogMethodAspect]
      public string MethodGeneric<T>(T value)
      {
        return value.ToString();
      }

      [LogMethodAspect]
      public string MethodGeneric<T>(T value, bool isTrue)
      {
        return value.ToString();
      }

      public string NotAspectedMethod(int value)
      {
        try {
          return value.ToString();
        }
        catch(Exception e) {
          throw;
        }
        finally {
          
        }
      }

      [LogMethodAspect]
      public TestClass(int value)
      {
        Log.Info(string.Format("Passed value: {0}", value));
      }
    }
  
      
    [Test]
    public void Test()
    {
      TestClass testClass = new TestClass(512);
      var result = testClass.AspectedMethod(123321);
      testClass.Method(20);
      testClass.Method("20");
      testClass.MethodGeneric(20);
      testClass.MethodGeneric("20", true);
      Assert.AreEqual("123321", result);
    }
  }
}