using DearImguiSharp;
using RenderSpy.ImGui.Core;
using System.Drawing;

namespace ExternalSharp.Utils
{
   
    namespace Style
    {
        public static class StyleExtensions {
            public static DearImguiSharp.ImVec4 ToImVec4(this System.Drawing.Color color)
            {
                return new DearImguiSharp.ImVec4()
                {
                    W = color.A / (float)255.0F,
                    X = color.R / (float)255.0F,
                    Y = color.G / (float)255.0F,
                    Z = color.B / (float)255.0F
                };
            }
        }

        public class DarkTheme
        {
            // https://www.unknowncheats.me/forum/c-and-c-/189635-imgui-style-settings.html
          

            public int Alpha { get; set; } = 255;

            public void ApplyColors()
            {
                var Style = DearImguiSharp.ImGui.GetStyle();
                var colors = new FinalizedList<DearImguiSharp.ImVec4[], DearImguiSharp.ImVec4>(Style.Colors);


                colors.Instance[(int)ImGuiCol.Text] = Color.FromArgb(Alpha, 204, 204, 211).ToImVec4();
                colors.Instance[(int)ImGuiCol.TextDisabled] = Color.FromArgb(Alpha, 61, 59, 74).ToImVec4();
                colors.Instance[(int)ImGuiCol.WindowBg] = Color.FromArgb(Alpha, 15, 13, 18).ToImVec4();
                colors.Instance[(int)ImGuiCol.ChildBg] = Color.FromArgb(Alpha, 18, 18, 23).ToImVec4();
                colors.Instance[(int)ImGuiCol.PopupBg] = Color.FromArgb(Alpha, 18, 18, 23).ToImVec4();
                colors.Instance[(int)ImGuiCol.Border] = Color.FromArgb(Alpha, 204, 204, 211).ToImVec4();
                colors.Instance[(int)ImGuiCol.BorderShadow] = Color.FromArgb(Alpha, 235, 232, 224).ToImVec4();
                colors.Instance[(int)ImGuiCol.FrameBg] = Color.FromArgb(Alpha, 26, 23, 31).ToImVec4();
                colors.Instance[(int)ImGuiCol.FrameBgHovered] = Color.FromArgb(Alpha, 61, 59, 74).ToImVec4();
                colors.Instance[(int)ImGuiCol.FrameBgActive] = Color.FromArgb(Alpha, 143, 143, 148).ToImVec4();
                colors.Instance[(int)ImGuiCol.TitleBg] = Color.FromArgb(Alpha, 26, 23, 31).ToImVec4();
                colors.Instance[(int)ImGuiCol.TitleBgCollapsed] = Color.FromArgb(Alpha, 255, 250, 242).ToImVec4();
                colors.Instance[(int)ImGuiCol.TitleBgActive] = Color.FromArgb(Alpha, 18, 18, 23).ToImVec4();
                colors.Instance[(int)ImGuiCol.MenuBarBg] = Color.FromArgb(Alpha, 26, 23, 31).ToImVec4();
                colors.Instance[(int)ImGuiCol.ScrollbarBg] = Color.FromArgb(Alpha, 26, 23, 31).ToImVec4();
                colors.Instance[(int)ImGuiCol.ScrollbarGrab] = Color.FromArgb(Alpha, 204, 204, 211).ToImVec4();
                colors.Instance[(int)ImGuiCol.ScrollbarGrabHovered] = Color.FromArgb(Alpha, 143, 143, 148).ToImVec4();
                colors.Instance[(int)ImGuiCol.ScrollbarGrabActive] = Color.FromArgb(Alpha, 15, 13, 18).ToImVec4();
                // Dim ComboBg = Color.FromArgb(Alpha,49, 46, 54, 255).ToImVec4()

                colors.Instance[(int)ImGuiCol.CheckMark] = Color.FromArgb(Alpha, 204, 204, 211).ToImVec4();
                colors.Instance[(int)ImGuiCol.SliderGrab] = Color.FromArgb(Alpha, 204, 204, 211).ToImVec4();
                colors.Instance[(int)ImGuiCol.SliderGrabActive] = Color.FromArgb(Alpha, 15, 13, 18).ToImVec4();
                colors.Instance[(int)ImGuiCol.Button] = Color.FromArgb(Alpha, 26, 23, 31).ToImVec4();
                colors.Instance[(int)ImGuiCol.ButtonHovered] = Color.FromArgb(Alpha, 61, 59, 74).ToImVec4();
                colors.Instance[(int)ImGuiCol.ButtonActive] = Color.FromArgb(Alpha, 143, 143, 148).ToImVec4();
                colors.Instance[(int)ImGuiCol.Header] = Color.FromArgb(Alpha, 26, 23, 31).ToImVec4();
                colors.Instance[(int)ImGuiCol.HeaderHovered] = Color.FromArgb(Alpha, 143, 143, 148).ToImVec4();
                colors.Instance[(int)ImGuiCol.HeaderActive] = Color.FromArgb(Alpha, 15, 13, 18).ToImVec4();
                // Dim Column = Color.FromArgb(Alpha,143, 143, 148, 255).ToImVec4()
                // Dim ColumnHovered = Color.FromArgb(Alpha,61, 59, 74, 255).ToImVec4()
                // Dim ColumnActive = Color.FromArgb(Alpha,143, 143, 148, 255).ToImVec4()
                colors.Instance[(int)ImGuiCol.ResizeGrip] = Color.FromArgb(Alpha, 0, 0, 0).ToImVec4();
                colors.Instance[(int)ImGuiCol.ResizeGripHovered] = Color.FromArgb(Alpha, 143, 143, 148).ToImVec4();
                colors.Instance[(int)ImGuiCol.ResizeGripActive] = Color.FromArgb(Alpha, 15, 13, 18).ToImVec4();
                // Dim CloseButton = Color.FromArgb(Alpha,102, 100, 97, 41).ToImVec4()
                // Dim CloseButtonHovered = Color.FromArgb(Alpha,102, 100, 97, 99).ToImVec4()
                // Dim CloseButtonActive = Color.FromArgb(Alpha,102, 100, 97, 255).ToImVec4()
                colors.Instance[(int)ImGuiCol.PlotLines] = Color.FromArgb(Alpha, 102, 100, 97).ToImVec4();
                colors.Instance[(int)ImGuiCol.PlotLinesHovered] = Color.FromArgb(Alpha, 64, 255, 0).ToImVec4();
                colors.Instance[(int)ImGuiCol.PlotHistogram] = Color.FromArgb(Alpha, 102, 100, 97).ToImVec4();
                colors.Instance[(int)ImGuiCol.PlotHistogramHovered] = Color.FromArgb(Alpha, 64, 255, 0).ToImVec4();
                colors.Instance[(int)ImGuiCol.TextSelectedBg] = Color.FromArgb(Alpha, 64, 255, 0).ToImVec4();
                colors.Instance[(int)ImGuiCol.ModalWindowDimBg] = Color.FromArgb(Alpha, 255, 250, 242).ToImVec4();


                Style.Colors = colors;
            }
        }
    }

}
