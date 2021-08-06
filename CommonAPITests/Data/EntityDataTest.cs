using System;
using System.Collections.Generic;
using CommonAPI;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace CommonAPITests
{
    [TestFixture]
    public class EntityDataTest
    {
        public EntityData data;
        public EntityData data2;

        [SetUp]
        public void Setup()
        {
            data = new EntityData
            {
                customData = new Dictionary<string, object>()
            };
            data2 = new EntityData();
            EntityDataExtensions.propertySerializers.Clear();
        }

        [Test]
        public void TestSerializerWarnProperties()
        {
            False(data.HasProperty("Test"));

            Throws(typeof(ArgumentException), () =>
            {
                data.SetProperty("Test", 5);
            });

            EntityDataExtensions.DefineProperty("Test", new IntArrayPropertySerializer());

            Throws(typeof(ArgumentException), () =>
            {
                data.SetProperty("Test", 5);
            });

            EntityDataExtensions.DefineProperty("Test1", new IntPropertySerializer());

            DoesNotThrow(() =>
            {
                data.SetProperty("Test1", 5);
            });
        }
        
        [Test]
        public void TestProperties()
        {
            EntityDataExtensions.DefineProperty("Test", new IntPropertySerializer());
            EntityDataExtensions.DefineProperty("Hello", new IntPropertySerializer());

            False(data.HasProperty("Test"));
            
            data.SetProperty("Test", 5);
            data.SetProperty("Test", 7);

            True(data.HasProperty("Test"));

            AreEqual(7, data.GetProperty<int>("Test"));
            AreEqual(0, data.GetProperty<int>("Hello"));

            AreEqual(7, data.GetOrAddProperty<int>("Test"));
            AreEqual(0, data.GetOrAddProperty<int>("Hello"));
        }
        
        [Test]
        public void TestPropertiesWNullData()
        {
            data.customData = null;
            EntityDataExtensions.DefineProperty("Test", new IntPropertySerializer());
            EntityDataExtensions.DefineProperty("Hello", new IntPropertySerializer());
            
            False(data.HasProperty("Test"));
            
            data.customData = null;
            data.SetProperty("Test", 5);
            data.SetProperty("Test", 7);

            True(data.HasProperty("Test"));

            AreEqual(7, data.GetProperty<int>("Test"));
            data.customData = null;
            AreEqual(0, data.GetProperty<int>("Hello"));

            data.customData = null;
            AreEqual(0, data.GetOrAddProperty<int>("Test"));
            AreEqual(0, data.GetOrAddProperty<int>("Hello"));

            data.customData = null;
            DoesNotThrow(() =>
            {
                Util.GetSerializationSetup(w =>
                {
                    EntityDataExtensions.ExportData(ref data, w);
                }, r =>
                {
                    EntityDataExtensions.ImportData(ref data, r);
                });
            });
        }
        
        [Test]
        public void TestSerialization()
        {
            EntityDataExtensions.DefineProperty("Test", new IntArrayPropertySerializer());
            EntityDataExtensions.DefineProperty("Test", new IntPropertySerializer());
            EntityDataExtensions.DefineProperty("Test1", new IntArrayPropertySerializer());

            data.SetProperty("Test", 5);
            data.SetProperty("Test1", new[]{1,2,3});
            
            Util.GetSerializationSetup(w =>
            {
                EntityDataExtensions.ExportData(ref data, w);
            }, r =>
            {
                EntityDataExtensions.ImportData(ref data2, r);
            });

            AreEqual(5, data2.GetProperty<int>("Test"));
            AreEqual(new[]{1,2,3}, data2.GetProperty<int[]>("Test1"));
        }
    }
}