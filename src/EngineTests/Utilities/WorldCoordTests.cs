using Microsoft.VisualStudio.TestTools.UnitTesting;
using Engine.Utilities;

namespace EngineTests.Utilities
{
    [TestClass]
    public class WorldCoordTests
    {
        [TestMethod]
        public void TestConstructor_Floats()
        {
            var w = new WorldCoord(1.234f, 5.678f);
            Assert.AreEqual(1.234f, w.X);
            Assert.AreEqual(5.678f, w.Y);
        }

        [TestMethod]
        public void TestConstructor_Ints()
        {
            var w = new WorldCoord(1, 2);
            Assert.AreEqual(1.0f, w.X);
            Assert.AreEqual(2.0f, w.Y);
        }

        [TestMethod]
        public void TestOperatorSubtract()
        {
            WorldCoord w1;
            WorldCoord w2;

            w1 = new WorldCoord(5, 7);
            w2 = new WorldCoord(4, 5);
            Assert.AreEqual(new WorldCoord(1, 2), w1 - w2);

            w1 = new WorldCoord(4, 5);
            w2 = new WorldCoord(5, 7);
            Assert.AreEqual(new WorldCoord(-1, -2), w1 - w2);

            w1 = new WorldCoord(5.0625f, 3.796875f);
            w2 = new WorldCoord(5.078125f, 3.8125f);
            Assert.AreEqual(WorldCoord.Zero, w1 - w1);

        }
    }
}
