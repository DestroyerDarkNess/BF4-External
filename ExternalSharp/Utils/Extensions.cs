using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSharp.Utils
{
    public static class Extensions
    {
        public static RawColorBGRA ToSharpDXColor(System.Drawing.Color drawingColor)
        {
            return new RawColorBGRA(drawingColor.B , drawingColor.G , drawingColor.R , drawingColor.A );
        }
    }

    public class ImColor : DearImguiSharp.ImColor
    {
        public ImColor(float W, float X, float Y, float Z) { 
        this.Value = new DearImguiSharp.ImVec4() { X = X, Y = Y, W = W , Z = Z };
        }
    }

    public class ImVec2 : DearImguiSharp.ImVec2
    {
        public ImVec2(float X, float Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }


}
