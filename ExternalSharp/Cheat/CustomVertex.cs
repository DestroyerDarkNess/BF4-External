using SharpDX;
using SharpDX.Direct3D9;


namespace Cheat
{
    
    // Define a custom vertex structure for DirectX
    public struct CustomVertex
    {
        public Vector4 Position;
        public int Color;  // ARGB color format

        public CustomVertex(Vector4 position, int color)
        {
            Position = position;
            Color = color;
        }

        public const VertexFormat Format = VertexFormat.PositionRhw | VertexFormat.Diffuse;
    }

}
