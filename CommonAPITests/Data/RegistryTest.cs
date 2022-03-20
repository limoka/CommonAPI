using System;
using System.Collections.Generic;
using System.IO;
using CommonAPI;
using NUnit.Framework;
using static NUnit.Framework.Assert;
// ReSharper disable InconsistentNaming

namespace CommonAPITests
{
    public class TestObject : IPoolable
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

        public virtual void Export(BinaryWriter w)
        {
            w.Write(id);
            w.Write(data);
            w.Write(test);
        }

        public virtual void Import(BinaryReader r)
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

    public class TestObject2 : TestObject
    {
        public int moreData;
    }
    
    public class BadObject : TestObject
    {
        public override void Import(BinaryReader r)
        {
            throw new Exception("Boo, I'm broken!");
        }
    }
    
    public class TestPool2 : Pool<TestObject>
    {
        public int typeId;
        private TypeRegistry<TestObject, TestPool2> registry;

        public TestPool2(int id, TypeRegistry<TestObject, TestPool2> registry)
        {
            typeId = id;
            this.registry = registry;
        }
        
        protected override TestObject GetNewInstance()
        {
            return registry.GetNew(typeId);
        }
    }
    
    [TestFixture]
    public class RegistryTest
    {
        public TypeRegistry<TestObject, TestPool2> registry;
        public TypeRegistry<TestObject, TestPool2> registry2;
        
        public Registry registry3;

        public List<TestPool2> objects;
        public List<TestPool2> objects2;


        [SetUp]
        public void SetUp()
        {
            registry = new TypeRegistry<TestObject, TestPool2>();
            registry2 = new TypeRegistry<TestObject, TestPool2>();
            registry3 = new Registry();
            objects = new List<TestPool2>();
            objects2 = new List<TestPool2>();
            
            CommonLogger.SetLogger(new FakeLogger());
        }

        [Test]
        public void TestRegistration()
        {
            registry.Register("Test:ID", typeof(TestObject));
            registry.Register("Test:ID1", typeof(TestObject2));
            
            int id2 = registry3.Register("this:test");
            registry3.Register("hello:world");
            int id3 = registry3.Register("hello:world");

            int id = registry.GetUniqueId("Test:ID");
            object obj = registry.GetNew(id);
            
            IsNotNull(obj);
            AreEqual(typeof(TestObject), obj.GetType());
            
            id = registry.GetUniqueId("Test:ID1");
            obj = registry.GetNew(id);
            
            IsNotNull(obj);
            AreEqual(typeof(TestObject2), obj.GetType());

            Throws<ArgumentException>(() =>
            {
                id = registry.GetUniqueId("Test:Hello");
            });

            id = registry3.GetUniqueId("hello:world");
            AreNotEqual(0, id);
            AreEqual(id3, id);
            
            id = registry3.GetUniqueId("this:test");
            AreEqual(id2, id);
        }
        
        [Test]
        public void TestMigration()
        {
            registry.Register("Test:ID", typeof(TestObject));
            registry.Register("Test:Bad", typeof(BadObject));
            registry.Register("Test:ID1", typeof(TestObject2));
            registry.Register("Test:ID2", typeof(TestObject));
            registry.Register("Test:ID3", typeof(TestObject2));
            
            registry2.Register("Test:Bad", typeof(BadObject));
            registry2.Register("Test:ID", typeof(TestObject));
            registry2.Register("Test:ID1", typeof(TestObject2));
            registry2.Register("Test:ID3", typeof(TestObject2));
            
            
            objects.Capacity = registry.data.Count + 1;
            objects.Add(null);
            for (int i = 1; i < registry.data.Count; i++)
            {
                TestPool2 pool = new TestPool2(i, registry);
                objects.Add(pool);
                
                pool.Init(8);
            }
            
            objects2.Capacity = registry2.data.Count + 1;
            objects2.Add(null);
            for (int i = 1; i < registry2.data.Count; i++)
            {
                TestPool2 pool = new TestPool2(i, registry2);
                objects2.Add(pool);
                
                pool.Init(8);
            }
            
            int id = registry.GetUniqueId("Test:ID");
            int id1 = registry.GetUniqueId("Test:ID1");
            int id2 = registry.GetUniqueId("Test:ID2");
            int id3 = registry.GetUniqueId("Test:ID3");
            
            int badId = registry.GetUniqueId("Test:Bad");

            objects[id].AddPoolItem(new object[0]);
            objects[id1].AddPoolItem(new object[0]);
            objects[id2].AddPoolItem(new object[0]);
            objects[id3].AddPoolItem(new object[0]);
            objects[badId].AddPoolItem(new object[0]);
            
            Util.GetSerializationSetup(w =>
            {
                registry.Export(w);
                registry.ExportContainer(objects, w);
            }, r =>
            {
                registry2.Import(r);
                registry2.ImportAndMigrate(objects2, r);
            });

            int id_2 = registry2.GetUniqueId("Test:ID");
            int id21 = registry2.GetUniqueId("Test:ID1");
            int id23 = registry2.GetUniqueId("Test:ID3");
            
            AreEqual(registry2.migrationMap[id], id_2);
            AreEqual(registry2.migrationMap[id1], id21);
            AreEqual(registry2.migrationMap[id3], id23);
            
            AreEqual(2, objects2[id_2].poolCursor);
            AreEqual(2, objects2[id21].poolCursor);
            AreEqual(2, objects2[id23].poolCursor);
        }

    }
}