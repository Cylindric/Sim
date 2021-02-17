using static Engine.Engine;

namespace Engine.Controllers
{
    public interface IController
    {
        void Start();
        void Update();
        void Render(LAYER layer = LAYER.DEFAULT);
    }
}
