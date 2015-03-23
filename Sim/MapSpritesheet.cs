namespace Sim
{
    class MapSpritesheet : SpriteController
    {
        public MapSpritesheet(GraphicsController graphics)
        {
            LoadBitmap("grass_and_rock.png", 32, graphics);
        }
    }
}
