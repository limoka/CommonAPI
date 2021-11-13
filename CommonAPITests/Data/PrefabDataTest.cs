using System.Collections.Generic;
using CommonAPI;
using CommonAPI.Systems;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace CommonAPITests
{
    public class TestComponent : FactoryComponent
    {
        
    }
    
    [TestFixture]
    public class PrefabDataTest
    {
        public ComponentDesc component;
        public PrefabDesc desc;

        [SetUp]
        public void Setup()
        {
            component = new ComponentDesc();
            desc = new PrefabDesc
            {
                customData = new Dictionary<string, object>()
            };
        }

        [Test]
        public void TestComponentDesc()
        {
            component.componentId = "TEST:ID1";

            ComponentSystem.componentRegistry.Register("TEST:ID1", typeof(TestComponent));

            int id = ComponentSystem.componentRegistry.GetUniqueId("TEST:ID1");
            
            component.ApplyProperties(desc);
            int id2 = desc.GetProperty<int>(ComponentDesc.FIELD_NAME);

            AreEqual(id, id2);
        }
        
        [Test]
        public void TestPrefabProperties()
        {
            False(desc.HasProperty("Test"));
            desc.SetProperty("Test", 5);
            desc.SetProperty("Test", 7);
            
            True(desc.HasProperty("Test"));
            
            AreEqual(7, desc.GetProperty<int>("Test"));
            AreEqual(0, desc.GetProperty<int>("Hello"));

            AreEqual(7, desc.GetOrAddProperty<int>("Test"));
            AreEqual(0, desc.GetOrAddProperty<int>("Hello"));
        }
    }
}