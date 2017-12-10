using Microsoft.VisualStudio.TestTools.UnitTesting;
using Engine.Renderer.SDLRenderer;
using Engine.Models;
using Engine.Utilities;
using System.Linq;
using System.IO;

namespace EngineTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestSimpleTileDraw()
        {
            SDLWindow.Instance.Start(100, 100, 128, 128);
            SDLRenderer.Instance.Start();

            var spritesheet = new SpriteSheet();
            spritesheet.Load(Engine.Engine.Instance.Path("assets", "base", "tiles", "floor.xml"));

            var go = new GameObject
            {
                Name = "Test",
                Position = new WorldCoord(32, 32),

                SpriteSheet = spritesheet,
                Sprite = spritesheet._sprites.First().Value
            };
            go.SpriteSheet.SortingLayer = "Tiles";

            SDLWindow.Instance.Update();
            spritesheet.Render(go.Sprite, (int)go.Position.X, (int)go.Position.Y);
            SDLWindow.Instance.Present();

            // Thread.Sleep(5000);

            SDLWindow.Instance.Screenshot(SsPath("TestSimpleTileDraw.bmp"));
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            if (Directory.Exists(SsPath()))
            {
                DeleteDirectory(SsPath());
            }
            Directory.CreateDirectory(SsPath());
        }

        private static string SsPath(string filename = null)
        {
            var path = Engine.Engine.Instance.Path("tests", "screenshots");

            if (!string.IsNullOrEmpty(filename))
            {
                path = Path.Combine(path, filename);
            }

            return path;
        }

        private static void DeleteDirectory(string path)
        {
            string[] files = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(path, false);
        }
    }
}
