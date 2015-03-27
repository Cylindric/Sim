namespace Sim
{
    public abstract class GameObject
    {
        protected GraphicsController Graphics;

        protected GameObject(GraphicsController graphics)
        {
            Graphics = graphics;
        }

        public abstract void Update(float timeDelta);
        public abstract void Render();
    }
}
