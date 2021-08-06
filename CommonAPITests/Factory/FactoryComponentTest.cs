using CommonAPI;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace CommonAPITests.Factory
{
    [TestFixture]
    public class FactoryComponentTest
    {
        public FactoryComponent component;
        
        [SetUp]
        public void Setup()
        {
            component = new FactoryComponent();
        }

        [Test]
        public void Test()
        {
            component.SetId(4);
            AreEqual(4, component.GetId());

            component.entityId = 2;
            component.pcId = 8;

            component.Free();
            AreEqual(0, component.GetId());
            AreEqual(0, component.entityId);
            AreEqual(0, component.pcId);
            
            component.SetId(4);
            component.entityId = 2;
            component.pcId = 8;
            
            Util.GetSerializationSetup(w =>
            {
                component.Export(w);
            }, r =>
            {
                component.Free();
                component.Import(r);
            });
            
            AreEqual(4, component.GetId());
            AreEqual(2, component.entityId);
            AreEqual(8, component.pcId);
        }
        
    }
}