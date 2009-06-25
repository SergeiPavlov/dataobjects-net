using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Parameters;
using Xtensive.Core.Testing;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Tests.Storage.Performance.CrudModel;

namespace Xtensive.Storage.Tests.Storage.Performance
{
  public abstract class DOCrudTestBase : AutoBuildTest
  {
    private bool warmup;
    private int instanceCount;
    private const int BaseCount = 10000;
    private const int InsertCount = BaseCount;

    protected abstract DomainConfiguration CreateConfiguration();

    protected sealed override DomainConfiguration BuildConfiguration()
    {
      var config = CreateConfiguration();
      config.Sessions.Add(new SessionConfiguration("Default"));
      config.Sessions.Default.CacheSize = BaseCount;
      config.Sessions.Default.CacheType = SessionCacheType.Infinite;
      config.Types.Register(typeof(Simplest).Assembly, typeof(Simplest).Namespace);
      return config;
    }

    [Test]
    public void RegularTest()
    {
      warmup = true;
      CombinedTest(10, 10);
      warmup = false;
      CombinedTest(BaseCount, InsertCount);
    }

    [Test]
    [Explicit]
    [Category("Profile")]
    public void ProfileTest()
    {
      warmup = true;
      CombinedTest(10, 10);
      warmup = false;
      InsertTest(BaseCount);
      //QueryTest(instanceCount / 5);
      CachedQueryTest(instanceCount / 5);
      MaterializeGetFieldTest(BaseCount);
      MaterializeCachedTest(BaseCount);
    }

    private void CombinedTest(int baseCount, int insertCount)
    {
      if (warmup)
        Log.Info("Warming up...");
      InsertTest(insertCount);
      MaterializeCachedTest(baseCount);
      MaterializeTest(baseCount);
      MaterializeGetFieldTest(baseCount);
      ExplicitMaterializeTest(baseCount);
      FetchTest(baseCount / 2);
      QueryTest(baseCount / 5);
      CachedQueryExpressionTest(baseCount / 5);
      CachedQueryTest(baseCount / 2);
      ProjectingCachedQueryTest(baseCount / 2);
      RseQueryTest(baseCount / 5);
      CachedRseQueryTest(baseCount / 5);
      RemoveTest();
    }

    private void InsertTest(int insertCount)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Insert", insertCount)) {
          using (var ts = s.OpenTransaction()) {
            for (int i = 0; i < insertCount; i++)
              new Simplest(i, i);
            ts.Complete();
          }
        }
      }
      instanceCount = insertCount;
    }

    private void FetchTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        long sum = (long)count*(count-1)/2;
        using (var ts = s.OpenTransaction()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Fetch & GetField", count)) {
            for (int i = 0; i < count; i++) {
              var key = Key.Create<Simplest>((long) i % instanceCount);
              var o = key.Resolve<Simplest>();
              sum -= o.Id;
            }
            ts.Complete();
          }
        }
        if (count<=instanceCount)
          Assert.AreEqual(0, sum);
      }
    }

    private void ExplicitMaterializeTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        int i = 0;
        using (var ts = s.OpenTransaction()) {
          var rs = d.Model.Types[typeof (Simplest)].Indexes.PrimaryIndex.ToRecordSet();
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Explicit Materialize", count)) {
            while (i < count) {
              foreach (var tuple in rs) {
                var o = new SqlClientCrudModel.Simplest 
                  {
                    Id = tuple.GetValueOrDefault<long>(0), 
                    Value = tuple.GetValueOrDefault<long>(2)
                  };
                if (++i >= count)
                  break;
              }
            }
            ts.Complete();
          }
        }
      }
    }

    private void MaterializeCachedTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        long sum = 0;
        int i = 0;
        var entities = new List<Entity>(count/2);
        using (var ts = s.OpenTransaction()) {
          while (i < count) {
            foreach (var o in Query<Simplest>.All) {
              sum += o.Id;
              if (i%2 == 0)
                entities.Add(o);
              if (++i >= count)
                break;
            }
            using (warmup ? null : new Measurement("Materialize Cached", count/2)) {
              foreach (var entity in entities)
                entity.Key.Resolve();
            }
          }
          Assert.AreEqual((long) count*(count - 1)/2, sum);
        }
      }
    }

    private void MaterializeGetFieldTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        long sum = 0;
        int i = 0;
        using (var ts = s.OpenTransaction()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Materialize & GetField", count)) {
            while (i < count)
              foreach (var o in CachedQuery.Execute(() => Query<Simplest>.All)) {
                sum += o.Id;
                if (++i >= count)
                  break;
              }
            ts.Complete();
          }
        }
        Assert.AreEqual((long)count*(count-1)/2, sum);
      }
    }

    private void MaterializeTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        int i = 0;
        using (var ts = s.OpenTransaction()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Materialize", count)) {
            while (i < count)
              foreach (var o in CachedQuery.Execute(() => Query<Simplest>.All)) {
                if (++i >= count)
                  break;
              }
            ts.Complete();
          }
        }
      }
    }

    private void QueryTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        using (var ts = s.OpenTransaction()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Query", count)) {
            for (int i = 0; i < count; i++) {
              var id = i % instanceCount;
              var query = Query<Simplest>.All.Where(o => o.Id == id);
              foreach (var simplest in query) {
                // Doing nothing, just enumerate
              }
            }
            ts.Complete();
          }
        }
      }
    }

    private void CachedQueryExpressionTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        using (var ts = s.OpenTransaction()) {
          var id = 0;
          var query = Query<Simplest>.All.Where(o => o.Id == id);
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Cached Query Expression", count)) {
            for (int i = 0; i < count; i++) {
              id = i % instanceCount;
              foreach (var simplest in query) {
                // Doing nothing, just enumerate
              }
            }
            ts.Complete();
          }
        }
      }
    }

    private void CachedQueryTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        using (var ts = s.OpenTransaction()) {
          var id = 0;
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Cached Query", count)) {
            for (int i = 0; i < count; i++) {
              id = i % instanceCount;
              var query = CachedQuery.Execute(() => Query<Simplest>.All
                .Where(o => o.Id == id));
              foreach (var simplest in query) {
                // Doing nothing, just enumerate
              }
            }
            ts.Complete();
          }
        }
      }
    }

    private void ProjectingCachedQueryTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        using (var ts = s.OpenTransaction()) {
          var id = 0;
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("Projecting Cached Query", count)) {
            for (int i = 0; i < count; i++) {
              id = i % instanceCount;
              var query = CachedQuery.Execute(() => Query<Simplest>.All
                .Where(o => o.Id == id)
                .Select(o => new {o.Id, o.Value}));
              foreach (var simplest in query) {
                // Doing nothing, just enumerate
              }
            }
            ts.Complete();
          }
        }
      }
    }

    private void RseQueryTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        using (var ts = s.OpenTransaction()) {
          TestHelper.CollectGarbage();
          using (warmup ? null : new Measurement("RSE Query", count)) {
            for (int i = 0; i < count; i++) {
              var pKey = new Parameter<Tuple>();
              var rs = d.Model.Types[typeof (Simplest)].Indexes.PrimaryIndex.ToRecordSet();
              rs = rs.Seek(() => pKey.Value);
              using (new ParameterContext().Activate()) {
                pKey.Value = Tuple.Create(i % instanceCount);
                var es = rs.ToEntities<Simplest>();
                foreach (var o in es) {
                  // Doing nothing, just enumerate
                }
              }
            }
            ts.Complete();
          }
        }
      }
    }

    private void CachedRseQueryTest(int count)
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        using (var ts = s.OpenTransaction()) {
          TestHelper.CollectGarbage();
          var pKey = new Parameter<Tuple>();
          var rs = d.Model.Types[typeof (Simplest)].Indexes.PrimaryIndex.ToRecordSet();
          rs = rs.Seek(() => pKey.Value);
          using (new ParameterContext().Activate()) {
            using (warmup ? null : new Measurement("Cached RSE Query", count)) {
              for (int i = 0; i < count; i++) {
                pKey.Value = Tuple.Create(i % instanceCount);
                var es = rs.ToEntities<Simplest>();
                foreach (var o in es) {
                  // Doing nothing, just enumerate
                }
              }
            }
            ts.Complete();
          }
        }
      }
    }

    private void RemoveTest()
    {
      var d = Domain;
      using (var ss = d.OpenSession()) {
        var s = ss.Session;
        TestHelper.CollectGarbage();
        using (warmup ? null : new Measurement("Remove", instanceCount)) {
          using (var ts = s.OpenTransaction()) {
            var rs = d.Model.Types[typeof (Simplest)].Indexes.PrimaryIndex.ToRecordSet();
            var es = rs.ToEntities<Simplest>();
            foreach (var o in es)
              o.Remove();
            ts.Complete();
          }
        }
      }
    }
  }
}