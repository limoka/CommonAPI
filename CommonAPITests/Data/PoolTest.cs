using System;
using System.IO;
using CommonAPI;
using NUnit.Framework;
using NUnit.Framework.Internal;
using static NUnit.Framework.Assert;

namespace CommonAPITests
{
    public class PoolItem : IPoolable
    {

        public int id;
        public int data;
        public bool test;

        public bool updated;
        
        public void Free()
        {
            id = 0;
            data = 123;
            test = true;
            updated = false;
        }

        public void Export(BinaryWriter w)
        {
            w.Write(id);
            w.Write(data);
            w.Write(test);
        }

        public void Import(BinaryReader r)
        {
            id = r.ReadInt32();
            data = r.ReadInt32();
            test = r.ReadBoolean();
        }

        public int GetId()
        {
            return id;
        }

        public void SetId(int id)
        {
            this.id = id;
        }
    }

    public class TestPool : Pool<PoolItem>
    {
        protected override void InitPoolItem(PoolItem item, object[] data)
        {
            item.data = (int)((Randomizer) data[0]).NextUInt();
        }
        
        protected override Action<PoolItem> InitUpdate()
        {
            return poolable =>
            {
                poolable.updated = true;
            };
        }
    }
    
    public class PoolTest
    {
        public TestPool pool;
        public TestPool pool2;
        public Randomizer randomizer;
        
        [SetUp]
        public void Setup()
        {
            pool = new TestPool();
            pool.Init(8);

            pool2 = new TestPool();
            pool2.Init(8);
            
            randomizer = new Randomizer();
        }

        [TearDown]
        public void End()
        {
            pool.Free();
            pool = new TestPool();
        }

        [Test]
        public void TestAddingElements()
        {
            pool.AddPoolItem(new object[] {randomizer});
            pool.AddPoolItem(new object[] {randomizer});
            pool.AddPoolItem(new object[] {randomizer});
            pool.AddPoolItem(new object[] {randomizer});
            pool.AddPoolItem(new object[] {randomizer});
            pool.AddPoolItem(new object[] {randomizer});
            pool.AddPoolItem(new object[] {randomizer});
            pool.AddPoolItem(new object[] {randomizer});
            pool.AddPoolItem(new object[] {randomizer});

            AreEqual(10, pool.poolCursor);
        }
        
        [Test]
        public void TestRemovingElements()
        {
            pool.AddPoolItem(new object[] {randomizer});
            int id = pool.AddPoolItem(new object[] {randomizer});
            pool.AddPoolItem(new object[] {randomizer});
            
            pool.RemovePoolItem( id);

            AreEqual(0, pool[id].id);
        }
        
        [Test]
        public void TestAddingAfterRemoval()
        {
            pool.AddPoolItem(new object[] {randomizer});
            int id = pool.AddPoolItem(new object[] {randomizer});
            pool.AddPoolItem(new object[] {randomizer});
            
            pool.RemovePoolItem( id);
            
            int newId = pool.AddPoolItem(new object[] {randomizer});

            AreEqual(newId, id);
        }
        
        [Test]
        public void TestUpdatingElements()
        {
            pool.AddPoolItem(new object[] {randomizer});
            int id = pool.AddPoolItem(new object[] {randomizer});
            pool.AddPoolItem(new object[] {randomizer});
            pool.AddPoolItem(new object[] {randomizer});
            
            pool.RemovePoolItem( id);

            pool.UpdatePool();

            foreach (PoolItem item in pool.pool)
            {
                if (item != null && item.id != 0)
                {
                    True(item.updated);
                }
            }
        }

        [Test]
        public void TestSavingData()
        {
            pool.AddPoolItem(new object[] {randomizer});
            int id = pool.AddPoolItem(new object[] {randomizer});
            pool.AddPoolItem(new object[] {randomizer});
            pool.AddPoolItem(new object[] {randomizer});
            int id2 = pool.AddPoolItem(new object[] {randomizer});
            pool.RemovePoolItem( id);
            pool.pool[id2] = null;
            
            pool.UpdatePool();
            
            Util.GetSerializationSetup(w =>
            {
                pool.Export(w);
            }, r =>
            {
                pool2.Import(r);
            });
            
            for (int i = 1; i < pool.poolCursor; i++)
            {
                PoolItem item = pool[i];
                PoolItem item2 = pool2[i];
                if (item == null)
                {
                    IsNull(item2);
                    continue;
                }
                
                AreEqual(item.id, item2.id);
                AreEqual(item.data, item2.data);
                AreEqual(item.test, item2.test);
            }
        }
    }
}