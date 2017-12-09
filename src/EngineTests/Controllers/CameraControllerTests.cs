using Microsoft.VisualStudio.TestTools.UnitTesting;
using Engine.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Utilities;

namespace Engine.Controllers.Tests
{
    [TestClass()]
    public class CameraControllerTests
    {
        [TestMethod()]
        public void SetPositionTest_WithValues()
        {
            CameraController.Instance.SetPosition(123, 456);
            Assert.AreEqual(123, CameraController.Instance.Position.X);
            Assert.AreEqual(456, CameraController.Instance.Position.Y);
        }

        [TestMethod()]
        public void SetPositionTest_WithVector()
        {
            var pos = new Vector2<int>(123, 456);
            CameraController.Instance.SetPosition(pos);
            Assert.AreEqual(123, CameraController.Instance.Position.X);
            Assert.AreEqual(456, CameraController.Instance.Position.Y);
        }

        [TestMethod()]
        public void ScreenToWorldPoint_WhenStationary()
        {
            Vector2<int> screen;
            Vector2<float> world;

            screen = new Vector2<int>(0, 0);
            world = CameraController.Instance.ScreenToWorldPoint(screen);
            Assert.AreEqual(0, world.X, "Tile X should be zero at screen zero.");
            Assert.AreEqual(0, world.Y, "Tile X should be zero at screen zero.");

            screen = new Vector2<int>(128, 64);
            world = CameraController.Instance.ScreenToWorldPoint(screen);
            Assert.AreEqual(2, world.X, "Tile X should be 2 at screen 128,64.");
            Assert.AreEqual(1, world.Y, "Tile X should be 1 at screen 128,64.");
        }

        [TestMethod()]
        public void ScreenToWorldPoint_WhenCameraMoved()
        {
            Vector2<int> screen;
            Vector2<float> world;

            // Move the camera a bit
            CameraController.Instance.Position = new Vector2<int>(256, 128);

            screen = new Vector2<int>(0, 0);
            world = CameraController.Instance.ScreenToWorldPoint(screen);
            Assert.AreEqual(4, world.X, "Tile X should be 4 at screen zero.");
            Assert.AreEqual(-2, world.Y, "Tile Y should be -2 at screen zero.");

            screen = new Vector2<int>(128, 64);
            world = CameraController.Instance.ScreenToWorldPoint(screen);
            Assert.AreEqual(6, world.X, "Tile X should be 6 at screen 128,64.");
            Assert.AreEqual(-1, world.Y, "Tile Y should be -1 at screen 128,64.");
        }

        [TestMethod()]
        public void UpdateTest()
        {
            Assert.Inconclusive();
        }

        [TestMethod()]
        public void RenderTest()
        {
            Assert.Inconclusive();
        }
    }
}