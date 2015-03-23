namespace Sim
{
    class PeopleSpritesheet:SpriteController
    {
        public PeopleSpritesheet(GraphicsController graphics)
        {
            LoadBitmap("people.png", 32, graphics);
        }
    }
}
