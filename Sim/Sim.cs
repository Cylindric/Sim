using System;


namespace Sim
{
    public class Sim
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (var game = new SimController())
                game.Run(60f, 0f);
        }
    }
}