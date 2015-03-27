using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sim
{
    public static class Input
    {
        public static KeyboardState GetState
        {
            get
            {
                return OpenTK.Input.Keyboard.GetState();
            }
        }
    }
}
