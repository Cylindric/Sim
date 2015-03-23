using OpenTK;

namespace Sim
{
    class Sprite
    {
        private readonly PeopleSpritesheet _spriteSheet;

        public Sprite(GraphicsController graphics)
        {
            _spriteSheet = new PeopleSpritesheet(graphics);
        }

        public void Render(Vector2 position, GraphicsController graphics)
        {
            _spriteSheet.Render(2, position, graphics);
        }
    }
}
