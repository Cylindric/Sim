using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sim.DataFormats
{
    public class CharacterDatafile
    {
        public string SpritesheetName;
        public int[] WalkDownFrames;
        public int[] WalkLeftFrames;
        public int[] WalkRightFrames;
        public int[] WalkUpFrames;

        public int[] IdleDownFrames;
        public int[] IdleLeftFrames;
        public int[] IdleRightFrames;
        public int[] IdleUpFrames;
    }
}
