
using EasyImGui;
using ExternalSharp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSharp.Cheat
{
    public class Globals
    {
        public static Overlay GOverlay = null;
        public static Memory Memory = new Memory();
        public static Config cfg = Utils.Config.Get();
        public static Cheat.SDK GameSDK = null;

    }
}
