﻿#if NET40
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Tests.Issues.IssueJira0314_IncorrectTransactionDisposingModel;

namespace Xtensive.Storage.Tests.Issues.IssueJira0314_IncorrectTransactionDisposingModel
{
  [HierarchyRoot]
  public sealed class Entity1 : Entity
  {
    [Field]
    [Key]
    public int Id { get; private set; }
  }

  [HierarchyRoot]
  public sealed class Entity2 : Entity
  {
    [Field]
    [Key]
    public int Id { get; private set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class IssueJira0314_IncorrectTransactionDisposing : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof (Entity1).Assembly, typeof (Entity1).Namespace);
      return config;
    }

    protected override void PopulateData()
    {
      using (Session.Open(Domain))
      using (Transaction.Open()) {
        new Entity1();
        new Entity2();
      }
    }

    [Test]
    public void Test()
    {
      int i = 0;
      var wait1 = new ManualResetEventSlim();
      var wait2 = new ManualResetEventSlim();
      Parallel.Invoke(
        () => {
          using (Session.Open(Domain))
          using (Transaction.Open(TransactionOpenMode.New, IsolationLevel.Serializable))
          using (Transaction.Open(TransactionOpenMode.New, IsolationLevel.Serializable)) {
            i++;
            Query.All<Entity1>().Lock(LockMode.Exclusive, LockBehavior.Wait).ToArray();
            wait1.Set();
            wait2.Wait();
            Query.All<Entity2>().Lock(LockMode.Exclusive, LockBehavior.Wait).ToArray();
          }
        },
        () => {
          using (Session.Open(Domain))
          using (Transaction.Open(TransactionOpenMode.New, IsolationLevel.Serializable))
          using (Transaction.Open(TransactionOpenMode.New, IsolationLevel.Serializable)) {
            i++;
            Query.All<Entity2>().Lock(LockMode.Exclusive, LockBehavior.Wait).ToArray();
            wait2.Set();
            wait1.Wait();
            Query.All<Entity1>().Lock(LockMode.Exclusive, LockBehavior.Wait).ToArray();
          }
        });
      Assert.That(i, Is.EqualTo(3));
    }
  }
}
#endif