using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RendererTests.scenes
{
    interface IScene
    {
        void Render();
        void Update();
    }
}
