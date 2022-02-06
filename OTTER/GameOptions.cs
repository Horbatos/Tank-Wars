using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OTTER
{
    /// <summary>
    /// Game options
    /// </summary>
    static class GameOptions
    {
        public static int SpriteHeight = 100;
        public static int SpriteWidth = 100;
        public static int Speed = 5;

        public static int LeftEdge = 0;
        public static int RightEdge = 1255;
        public static int UpEdge = 0;
        public static int DownEdge = 675;
        public static int MinDistance=250;//minimalna dozvoljena udaljenost neprijatelja od igraca

        //koordinate na kojima se skrivaju spriteovi kad se ne koriste
        public static int HideX = -1000;
        public static int HideY = -1000;

    }
}
