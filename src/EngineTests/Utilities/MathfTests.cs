using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Engine.Utilities.Tests
{
    [TestClass()]
    public class MathfTests
    {
        [TestMethod()]
        public void WrapTest_Ints()
        {
            Assert.AreEqual(5, Mathf.Wrap(15, 0, 10), "input above range should wrap to beginning");
            Assert.AreEqual(5, Mathf.Wrap(105, 0, 10), "input far above range should wrap to beginning");
            Assert.AreEqual(6, Mathf.Wrap(1, 5, 10), "input below range should wrap to end");
        }

        [TestMethod()]
        public void WrapTest_Floats()
        {
            Assert.AreEqual(5f, Mathf.Wrap(15f, 0f, 10f)); // value is 5 more than the max, so result should be 5.
            Assert.AreEqual(6f, Mathf.Wrap(1f, 5f, 10f)); // value is 4 less than the min, so result should be 6.
        }

        [TestMethod()]
        public void LerpTest()
        {
            Assert.AreEqual(0f, Mathf.Lerp(0f, 1f, 0f));
            Assert.AreEqual(0.5f, Mathf.Lerp(0f, 1f, 0.5f));
            Assert.AreEqual(1f, Mathf.Lerp(0f, 1f, 1f));
        }
    }
}