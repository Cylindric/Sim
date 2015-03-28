using OpenTK.Input;

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
