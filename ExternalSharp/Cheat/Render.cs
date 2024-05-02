using DearImguiSharp;
using GameOverlay.Windows;
using SharpDX.Direct2D1;
using SharpDX.Direct3D9;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace ExternalSharp.Cheat
{
    public class Render
    {

        // Colors
        private System.Drawing.Color FOV_User = System.Drawing.Color.Gray; //new ImColor() { Value = new ImVec4() { W = 1f, X = 1f, Y = 1f, Z = 1f } };
        private System.Drawing.Color ESP_Normal = System.Drawing.Color.Red; //new ImColor() { Value = new ImVec4() { W = 1f, X = 0f, Y = 0f, Z = 1f } }; 
        private System.Drawing.Color ESP_Visible = System.Drawing.Color.Lime;// new ImColor() { Value = new ImVec4() { W = 0f, X = 1f, Y = 0f, Z = 1f } }; 
        private System.Drawing.Color ESP_Team = System.Drawing.Color.DarkGray;// new ImColor() { Value = new ImVec4() { W = 0f, X = 0.75f, Y = 1f, Z = 1f } }; 
        private System.Drawing.Color ESP_Filled = System.Drawing.Color.FromArgb(255,34,34,34); //new ImColor() { Value = new ImVec4() { W = 0f, X = 0f, Y = 0f, Z = 0.3f } }; 
        private System.Drawing.Color ESP_Skeleton = System.Drawing.Color.Red; //new ImColor() { Value = new ImVec4() { W = 1f, X = 1f, Y = 1f, Z = 0.9f } }; 
        private System.Drawing.Color Crosshair_Color = System.Drawing.Color.Aqua;
        private System.Drawing.Color ESP_VehTeam = System.Drawing.Color.DarkGray;
        private System.Drawing.Color ESP_VehNormal = System.Drawing.Color.DarkRed;

        public List<string> NameList = new List<string>();

        public void DrawLine(ImVec2 a, ImVec2 b, RawColorBGRA color, float width)
        {
            //ImGui.ImDrawListAddLine(ImGui.GetWindowDrawList(), a, b, 1, width);

            if (Globals.GOverlay.LineSurfaces.Count != 0)
            {
                Globals.GOverlay.LineSurfaces[0].Draw(new[] {
            new RawVector2(a.X, a.Y), 
            new RawVector2(b.X, b.Y), 
            }, color);

            }

            //ImGui.GetWindowDrawList().AddLine(a, b, color, width);
        }

        void Text(ImVec2 pos, System.Drawing.Color color, string text_begin, string text_end, float wrap_width, ImVec4 cpu_fine_clip_rect)
        {
          
            if (string.IsNullOrEmpty(text_begin))
                return;
          
            if (Globals.GOverlay.FontSurfaces.Count != 0) { Globals.GOverlay.FontSurfaces[0].DrawText(null, text_begin, (int)pos.X, (int)pos.Y, Utils.Extensions.ToSharpDXColor(color)); }


            //ImGui.ImDrawListAddTextFontPtr(ImGui.GetWindowDrawList(), ImGui.GetFont(), ImGui.GetFontSize(), pos, 1 , text_begin, text_end, wrap_width, cpu_fine_clip_rect);
            //  ImGui.GetWindowDrawList().AddText(ImGui.GetFont(), ImGui.GetFontSize(), pos, color, text_begin, text_end, wrap_width, cpu_fine_clip_rect);
        }

        

        void String(ImVec2 pos, System.Drawing.Color color, string text)
        {
            Text(pos, color, text, text + text.Length, 200, new ImVec4());
        }

        public void RectFilled(float x0, float y0, float x1, float y1, RawColorBGRA color, float rounding, int rounding_corners_flags)
        {
            //ImGui.ImDrawListAddRectFilled(ImGui.GetBackgroundDrawListNil(), new ImVec2() { X = 0, Y = 0 }, new ImVec2() { X = 1, Y = 1}, 0, rounding, rounding_corners_flags);
            //   ImGui.GetWindowDrawList().AddRectFilled(new ImVec2(x0, y0), new ImVec2(x1, y1), color, rounding, rounding_corners_flags);
            DrawBox((int)x0, (int)y0, (int)x1, (int)y1, color);
        }

        void DrawFilledBox(int x, int y, int w, int h, RawColorBGRA borderColor, RawColorBGRA fillColor)
        {
            // Dibujar los bordes de la caja
            DrawBox(x, y, w, h, borderColor);

            // Rellenar el área interior de la caja
            for (int xPos = x + 1; xPos < x + w; xPos++)
            {
                DrawLine(new ImVec2() { X = xPos, Y = y -1 }, new ImVec2() { X = xPos, Y = y + h +1 }, fillColor, 1.0f);
            }
        }

        public void HealthBar(float x, float y, float w, float h, int value, int v_max, bool Background)
        {
            if (v_max <= 0)
                return;

            if (value <= 0)
                return;

            //v_max ---- 100%
            //Value ----  ?
        
            int ValuePorcent = ((value * 100) / v_max);

            //h --------- 100%
            //? -------- Value%

            int ValueY_H = ((ValuePorcent * (int)h) / 100);

            System.Drawing.Color BarColor = System.Drawing.Color.Lime;

            // BaseBar
            if (Background)
            {
                DrawFilledBox((int)(x - 3), (int)y, (int)(w + 3), (int)(h), Utils.Extensions.ToSharpDXColor(System.Drawing.Color.FromArgb(80, System.Drawing.Color.DarkGray)), Utils.Extensions.ToSharpDXColor(System.Drawing.Color.DarkGray));
            }
            
            DrawFilledBox((int)(x - 3), (int)y, (int)(w + 3), ValueY_H, Utils.Extensions.ToSharpDXColor(System.Drawing.Color.FromArgb(80, System.Drawing.Color.DarkGray)), Utils.Extensions.ToSharpDXColor(BarColor));
        }


        public void DrawCircle(int x, int y, int radius, RawColorBGRA color, int segments = 360)
        {
            if (Globals.GOverlay.LineSurfaces.Count == 0)
            {
                return;
            }

            float angleStep = (float)(2 * Math.PI) / segments;

            List<RawVector2> circlePoints = new List<RawVector2>();

            for (int i = 0; i <= segments; i++)
            {
                float angle = i * angleStep;
                float px = x + radius * (float)Math.Cos(angle);
                float py = y + radius * (float)Math.Sin(angle);

                circlePoints.Add(new RawVector2(px, py));
            }

            for (int i = 0; i < circlePoints.Count - 1; i++)
            {
                Globals.GOverlay.LineSurfaces[0].Draw(new[]
                {
            circlePoints[i],
            circlePoints[i + 1],
        }, color);
            }
        }



        public void DrawCircleFilled(int x, int y, int radius, RawColorBGRA color, float thickness = 1.0f)
        {
            if (Globals.GOverlay.LineSurfaces.Count != 0)
            {
                // Calculate circle points using Bresenham's circle algorithm
                List<RawVector2> circlePoints = GetCirclePoints(x, y, radius);

                // Draw lines using existing Draw functionality (assuming Draw is available)
                foreach (var pointPair in SplitCirclePoints(circlePoints))
                {
                    Draw(pointPair[0], pointPair[1], color, thickness);
                }
            }
        }

        private List<RawVector2> GetCirclePoints(int xCenter, int yCenter, int radius)
        {
            List<RawVector2> circlePoints = new List<RawVector2>();
            int x = 0;
            int y = radius;
            int decisionOver2 = 1 - radius;

            while (y >= x)
            {
                circlePoints.Add(new RawVector2(xCenter + x, yCenter + y));
                circlePoints.Add(new RawVector2(xCenter - x, yCenter + y));
                circlePoints.Add(new RawVector2(xCenter + x, yCenter - y));
                circlePoints.Add(new RawVector2(xCenter - x, yCenter - y));
                circlePoints.Add(new RawVector2(xCenter + y, yCenter + x));
                circlePoints.Add(new RawVector2(xCenter - y, yCenter + x));
                circlePoints.Add(new RawVector2(xCenter + y, yCenter - x));
                circlePoints.Add(new RawVector2(xCenter - y, yCenter - x));

                x++;
                if (decisionOver2 <= 0)
                {
                    decisionOver2 += 2 * x + 1;
                }
                else
                {
                    y--;
                    decisionOver2 += 2 * (x - y) + 1;
                }
            }

            return circlePoints;
        }

        private IEnumerable<RawVector2[]> SplitCirclePoints(List<RawVector2> points)
        {
            for (int i = 0; i < points.Count; i += 2)
            {
                yield return new RawVector2[] { points[i], points[(i + 1) % points.Count] };
            }
        }

        private void Draw(RawVector2 Raw1, RawVector2 Raw2, RawColorBGRA Color, float thickness)
        {
            if (Globals.GOverlay.LineSurfaces.Count != 0)
            {
                Globals.GOverlay.LineSurfaces[0].Draw(new[] {
            Raw1,
            Raw2,
                }, Color);
            }
        }

        public void DrawTriangle(ImVec2 a, ImVec2 b, ImVec2 c, RawColorBGRA color)
        {
            if (Globals.GOverlay.LineSurfaces.Count == 0)
            {
                return;
            }

            Globals.GOverlay.LineSurfaces[0].Draw(new[]
            {
        new RawVector2(a.X, a.Y),
        new RawVector2(b.X, b.Y),
    }, color);

            Globals.GOverlay.LineSurfaces[0].Draw(new[]
            {
        new RawVector2(b.X, b.Y),
        new RawVector2(c.X, c.Y),
    }, color);

            Globals.GOverlay.LineSurfaces[0].Draw(new[]
            {
        new RawVector2(c.X, c.Y),
        new RawVector2(a.X, a.Y),
    }, color);
        }


        void DrawBox(int x, int y, int w, int h, RawColorBGRA color)
        {
            //DrawLine(new ImVec2() {X= x,Y= y }, new ImVec2() { X= x + w,Y= y }, color, 1.0f);
            //DrawLine(new ImVec2() {X= x, Y= y }, new ImVec2() {X= x,Y= y + h }, color, 1.0f);
            //DrawLine(new ImVec2() {X= x + w,Y= y }, new ImVec2() {X= x + w,Y= y + h }, color, 1.0f);
            //DrawLine(new ImVec2() {X= x,Y= y + h }, new ImVec2() {X= x + w,Y= y + h }, color, 1.0f);


            if (Globals.GOverlay.LineSurfaces.Count != 0)
            {
                Globals.GOverlay.LineSurfaces[0].Draw(new[] {
            new RawVector2(x, y),
            new RawVector2(x + w,  y),
            }, color);

                Globals.GOverlay.LineSurfaces[0].Draw(new[] {
            new RawVector2(x, y),
            new RawVector2(x ,  y + h),
            }, color);


                Globals.GOverlay.LineSurfaces[0].Draw(new[] {
            new RawVector2(x + w, y),
            new RawVector2(x + w , y + h ),
            }, color);

                Globals.GOverlay.LineSurfaces[0].Draw(new[] {
            new RawVector2(x , y + h),
            new RawVector2(x + w , y + h),
            }, color);

            }

        }


        public void RenderInfo()
        {
            //FPS
            String(new ImVec2() { X = 1, Y = 1 }, System.Drawing.Color.White, (int)ImGui.GetIO().Framerate + " FPS");


            // AimFov
            if (ExternalSharp.Cheat.Globals.cfg.AimBot && ExternalSharp.Cheat.Globals.cfg.DrawFov )
            {
                RawColorBGRA fovcol = Utils.Extensions.ToSharpDXColor(FOV_User);

                if (!Globals.cfg.ShowMenu )
                {
                    DrawCircle((int)ExternalSharp.Cheat.Globals.GOverlay.Right / 2, (int)ExternalSharp.Cheat.Globals.GOverlay.Bottom / 2, (int)(ExternalSharp.Cheat.Globals.cfg.AimFov + 1f), fovcol);

                }
                else
                {
                    DrawCircleFilled((int)ExternalSharp.Cheat.Globals.GOverlay.Right / 2, (int)ExternalSharp.Cheat.Globals.GOverlay.Bottom / 2, (int)(ExternalSharp.Cheat.Globals.cfg.AimFov + 1f), fovcol);

                }

                //if (ExternalSharp.Cheat.Globals.cfg.FovFilled)
                //ImGui.GetBackgroundDrawList().AddCircleFilled(new ImVec2((float)cfg.GameRect.right / 2f, (float)cfg.GameRect.bottom / 2f), cfg.AimFov, new ImColor(fovcol.Value.x, fovcol.Value.y, fovcol.Value.z, 0.1f), NULL);
            }

            // Crosshair
            if (ExternalSharp.Cheat.Globals.cfg.Crosshair)
            {
                switch (ExternalSharp.Cheat.Globals.cfg.CrosshairType)
                {
                    case 0:
                        DrawLine(new ExternalSharp.Utils.ImVec2(((float)ExternalSharp.Cheat.Globals.GOverlay.Right / 2f + 4) , ((float)ExternalSharp.Cheat.Globals.GOverlay.Bottom / 2f)), new ExternalSharp.Utils.ImVec2(((float)ExternalSharp.Cheat.Globals.GOverlay.Right / 2f + 10), ((float)ExternalSharp.Cheat.Globals.GOverlay.Bottom / 2f)), Utils.Extensions.ToSharpDXColor(Crosshair_Color), 1);
                        DrawLine(new ExternalSharp.Utils.ImVec2(((float)ExternalSharp.Cheat.Globals.GOverlay.Right / 2f - 4) , ((float)ExternalSharp.Cheat.Globals.GOverlay.Bottom / 2f)), new ExternalSharp.Utils.ImVec2(((float)ExternalSharp.Cheat.Globals.GOverlay.Right / 2f - 10) , ((float)ExternalSharp.Cheat.Globals.GOverlay.Bottom / 2f)), Utils.Extensions.ToSharpDXColor(Crosshair_Color), 1);
                        DrawLine(new ExternalSharp.Utils.ImVec2(((float)ExternalSharp.Cheat.Globals.GOverlay.Right / 2f), ((float)ExternalSharp.Cheat.Globals.GOverlay.Bottom / 2f + 4)), new ExternalSharp.Utils.ImVec2(((float)ExternalSharp.Cheat.Globals.GOverlay.Right / 2f), ((float)ExternalSharp.Cheat.Globals.GOverlay.Bottom / 2f + 10)), Utils.Extensions.ToSharpDXColor(Crosshair_Color), 1);
                        DrawLine(new ExternalSharp.Utils.ImVec2(((float)ExternalSharp.Cheat.Globals.GOverlay.Right / 2f), ((float)ExternalSharp.Cheat.Globals.GOverlay.Bottom / 2f - 4)), new ExternalSharp.Utils.ImVec2(((float)ExternalSharp.Cheat.Globals.GOverlay.Right / 2f), ((float)ExternalSharp.Cheat.Globals.GOverlay.Bottom / 2f - 10)), Utils.Extensions.ToSharpDXColor(Crosshair_Color), 1);
                        break;
                    case 1:
                        //ImGui.GetBackgroundDrawList().AddCircleFilled(new ImVec2((float)cfg.GameRect.right / 2f, (float)cfg.GameRect.bottom / 2f), 3, new ImColor(0f, 0f, 0f, 1f), NULL);
                        //ImGui.GetBackgroundDrawList().AddCircleFilled(new ImVec2((float)cfg.GameRect.right / 2f, (float)cfg.GameRect.bottom / 2f), 2, new ImColor(1f, 1f, 1f, 1f), NULL);
                        break;
                    default:
                        break;
                }
            }

            if (NameList.Count != 0)
            {
                string spcw = "[ Spectator Found! ]";
                ImVec2 spcwSize = new ImVec2();
                ImGui.CalcTextSize(spcwSize, spcw, null, false, -1.0f);
                String(new Utils.ImVec2((Globals.GOverlay.Width / 2) - (spcwSize.X / 2), 0f), System.Drawing.Color.Red, spcw);
                //   ImGui.Text(new Utils.ImVec2(Globals.GOverlay.Right / 2f - (spcwSize.X / 2f), 0f), new Utils.ImColor(1f, 0f, 0f, 1f), spcw);
            }

            
        }


        public void RenderESP()
        {

            Player pEntity = new Player(ExternalSharp.Cheat.Globals.Memory);
            Player pLocal = new Player(ExternalSharp.Cheat.Globals.Memory);
           
            // Context
            long ClientGameContext = ExternalSharp.Cheat.Globals.Memory.Read<long>((IntPtr)offset.ClientgameContext);
            long PlayerManager = ExternalSharp.Cheat.Globals.Memory.Read<long>((IntPtr)(ClientGameContext + offset.PlayerManager));
            long PlayerEntity = ExternalSharp.Cheat.Globals.Memory.Read<long>((IntPtr)(PlayerManager + offset.ClientPlayer));

            // LocalPlayer
            pLocal.ClientPlayer = ExternalSharp.Cheat.Globals.Memory.Read<long>((IntPtr)(PlayerManager + offset.LocalPlayer));
            pLocal.Update();
            ExternalSharp.Cheat.Globals.GameSDK.UpdateW2SData();

            NameList.Clear();

            // ESP Loop
            for (int i = 0; i < 72; i++)
            {
                
                // LocalPlayer Check
                if (pLocal.IsDead() && !pLocal.InVehicle())
                    break;

                // Update Player
                pEntity.ClientPlayer = ExternalSharp.Cheat.Globals.Memory.Read<long>((IntPtr)(PlayerEntity + (i * 0x08)));
                pEntity.Update();

                // Spectaror Warning
                bool IsSpec = pEntity.IsSpectator();

                if (ExternalSharp.Cheat.Globals.cfg.CheckSpectator == true && IsSpec)
                {
                    NameList.Add(pEntity.Name);
                    continue;
                }
                  

                // Invalid Player
                if (pEntity.ClientPlayer == 0)
                {
                    continue;
                }
                else if (pEntity.ClientPlayer == pLocal.ClientPlayer) {
                    continue;
                }
                else if (pEntity.ClientVehicle == pLocal.ClientVehicle && pLocal.ClientVehicle != 0 && pEntity.ClientVehicle != 0)
                {
                    continue;
                }
                  

                // GetDistance
                float distance = ExternalSharp.Cheat.Globals.GameSDK.GetDistance(pLocal.Position, pEntity.Position);
              
                // Check ESP MaxDistance
                if (ExternalSharp.Cheat.Globals.cfg.ESP_MaxDistance < distance)
                    continue;

                 // Vehicle ESP
                if (ExternalSharp.Cheat.Globals.cfg.VehicleESP && pEntity.InVehicle())
                {

                    if (!ExternalSharp.Cheat.Globals.cfg.TeamESP && pEntity.Team == pLocal.Team)
                        continue;

                  RawColorBGRA VehColor =  pEntity.Team == pLocal.Team ? Utils.Extensions.ToSharpDXColor(ESP_VehTeam) : Utils.Extensions.ToSharpDXColor(ESP_VehNormal);

                    //Matrix Object Bug :(
                  //  DrawAABB(pEntity.VehicleAABB, pEntity.VehicleTranfsorm, SharpDX.Color.Red);

                    if (ExternalSharp.Cheat.Globals.cfg.ESP_Distance)
                    {
                        // float to Text

                        string VehHealth = Math.Round(pEntity.HealthBase.m_VehicleHealth).ToString();

                        string text = ((int)distance).ToString() + "m - HP: " + VehHealth;
                        ImVec2 textSize = new ImVec2();
                        ImGui.CalcTextSize(textSize, text, null, false, -1.0f);

                        Vector2 VehicleScreen = new Vector2();
                        if (!ExternalSharp.Cheat.Globals.GameSDK.WorldToScreen(pEntity.Position, out VehicleScreen))
                            continue;

                        if (VehicleScreen != new Vector2(0f, 0f))
                        {
                           
                            Vector4 pEntityMax = pEntity.VehicleAABB.Max;
                            Vector4 pEntityMin = pEntity.VehicleAABB.Min;
                            Vector3 Top = pEntity.Position + new Vector3(pEntityMax.X, pEntityMax.Y, pEntityMax.Z);
                            Vector3 Btm = pEntity.Position + new Vector3(pEntityMin.X, pEntityMin.Y, pEntityMin.Z);

                            Vector2 BoxTop, BoxBtm;
                            if (!ExternalSharp.Cheat.Globals.GameSDK.WorldToScreen(Top, out BoxTop) || !ExternalSharp.Cheat.Globals.GameSDK.WorldToScreen(Btm, out BoxBtm))
                                continue;
                            else if (BoxTop == new Vector2(0f, 0f) || BoxBtm == new Vector2(0f, 0f))
                                continue;

                            float BoxMiddle = VehicleScreen.X;
                            float Height = BoxBtm.Y - BoxTop.Y;
                            float Width = Height / 4f;

                            DrawBox((int)(VehicleScreen.X - Width), (int)VehicleScreen.Y, (int)(Height), (int)(-Height), VehColor);

                            String(new ImVec2() { X = VehicleScreen.X - (textSize.X / 2f), Y = VehicleScreen.Y }, System.Drawing.Color.White, text);
                        }
                    }

                    continue;
                }
                else if (pEntity.InVehicle())
                {
                    continue;
                }


                // Some check
                if (pEntity.IsDead() || pEntity.InVehicle())
                    continue;
                else if (!ExternalSharp.Cheat.Globals.cfg.TeamESP && pEntity.Team == pLocal.Team)
                    continue;
                
                // WorldToScreen
                Vector2 ScreenPosition = new Vector2(0f, 0f);
                if (!ExternalSharp.Cheat.Globals.GameSDK.WorldToScreen(pEntity.Position,out ScreenPosition))
                    continue;


                // W2S Check
                if (ScreenPosition != new Vector2(0f, 0f))
                {
                    // Set ESP color
                    RawColorBGRA color = pEntity.IsVisible() ? Utils.Extensions.ToSharpDXColor(ESP_Visible) : Utils.Extensions.ToSharpDXColor(ESP_Normal);


                    // Teammte
                    if (ExternalSharp.Cheat.Globals.cfg.TeamESP && pEntity.Team == pLocal.Team)
                        color = Utils.Extensions.ToSharpDXColor(ESP_Team);


                    //  Get some size.

                    Vector4 pEntityMax = pEntity.GetAABB().Max;
                    Vector4 pEntityMin = pEntity.GetAABB().Min;
                    Vector3 Top = pEntity.Position + new Vector3(pEntityMax.X, pEntityMax.Y, pEntityMax.Z);
                    Vector3 Btm = pEntity.Position + new Vector3(pEntityMin.X, pEntityMin.Y, pEntityMin.Z);

                    Vector2 BoxTop, BoxBtm;
                    if (!ExternalSharp.Cheat.Globals.GameSDK.WorldToScreen(Top, out BoxTop) || !ExternalSharp.Cheat.Globals.GameSDK.WorldToScreen(Btm, out BoxBtm))
                        continue;
                    else if (BoxTop == new Vector2(0f, 0f) || BoxBtm == new Vector2(0f, 0f))
                        continue;

                    float BoxMiddle = ScreenPosition.X;
                    float Height = BoxBtm.Y - BoxTop.Y;
                    float Width = Height / 4f;


                    // Box
                    if (ExternalSharp.Cheat.Globals.cfg.ESP_Box)
                    {
                        //string DebugText = "X: " + ((int)(ScreenPosition.X - Width)).ToString() + Environment.NewLine + " Y: " + ((int)ScreenPosition.Y).ToString() + Environment.NewLine + Environment.NewLine + "Size: " + Environment.NewLine + "Width : " + (int)(Height / 2f) + Environment.NewLine + "Height : " + (int)-Height;
                        //String(new ImVec2() { X = (int)(ScreenPosition.X - Width), Y = (int)ScreenPosition.Y }, new ImColor() { Value = new ImVec4 { W = 1f, X = 1f, Y = 1f, Z = 1f } }, DebugText);
                    
                        // Filled
                        if (ExternalSharp.Cheat.Globals.cfg.ESP_BoxFilled)
                        {
                            DrawFilledBox((int)(ScreenPosition.X - Width), (int)ScreenPosition.Y, (int)(Height / 2f), (int)-Height, color, Utils.Extensions.ToSharpDXColor(ESP_Filled));
                        } 
                        else
                        {
                            DrawBox((int)(ScreenPosition.X - Width), (int)ScreenPosition.Y, (int)(Height / 2f), (int)-Height, color);
                        }

                    }

                    // Line
                    if (ExternalSharp.Cheat.Globals.cfg.ESP_Line)
                        DrawLine(new ImVec2() { X = ExternalSharp.Cheat.Globals.GOverlay.Right / 2, Y = ExternalSharp.Cheat.Globals.GOverlay.Bottom }, new ImVec2() { X = ScreenPosition.X, Y = ScreenPosition.Y }, color, 1);

                    // Bone ESP
                    if (ExternalSharp.Cheat.Globals.cfg.ESP_Skeleton)
                    {
                        int[,] aSkeleton = new int[12, 2]
                        {
                    { 104, 142 },{ 142, 9 },{ 9, 11 },{ 11, 15 },
                    { 142,109},{109,111 },{111, 115},{ 142, 5 },
                    { 5,  188},{ 5, 197},{ 188, 184},{ 197, 198},
                        };

                        for (int j = 0; j < 12; ++j)
                        {
                            Vector3 Bone1 = pEntity.GetBone(aSkeleton[j, 0]);
                            Vector3 Bone2 = pEntity.GetBone(aSkeleton[j, 1]);

                            if (Bone1 == new Vector3(0f, 0f, 0f) || Bone2 == new Vector3(0f, 0f, 0f))
                                break;

                            Vector2 Out1, Out2;
                            if (ExternalSharp.Cheat.Globals.GameSDK.WorldToScreen(Bone1, out Out1) && ExternalSharp.Cheat.Globals.GameSDK.WorldToScreen(Bone2, out Out2))
                            {
                                if (Out1.X > ExternalSharp.Cheat.Globals.GOverlay.Right || Out1.X < ExternalSharp.Cheat.Globals.GOverlay.Left)
                                    break;
                                else if (Out1 == new Vector2(0f, 0f) || Out2 == new Vector2(0f, 0f))
                                    continue;

                                DrawLine(new ImVec2() { X = Out1.X, Y = Out1.Y }, new ImVec2() { X = Out2.X, Y= Out2.Y }, ExternalSharp.Cheat.Globals.cfg.ESP_SkeletonColor == 0 ? color : Utils.Extensions.ToSharpDXColor(ESP_Skeleton), 1);
                            }
                        }
                    }

                    // Health Bar
                    if (ExternalSharp.Cheat.Globals.cfg.ESP_HealthBar)
                        HealthBar(ScreenPosition.X - Width - 5, ScreenPosition.Y + 1, 2f, -Height - 1, (int)pEntity.HealthBase.m_Health, (int)pEntity.HealthBase.m_MaxHealth, true);

                    // Distance
                    if (ExternalSharp.Cheat.Globals.cfg.ESP_Distance)
                    {
                        // float to Text
                        string text = ((int)distance).ToString() + "m";

                        ImVec2 textSize = new ImVec2();
                        ImGui.CalcTextSize(textSize, text, null, false, -1.0f);

                        String(new ImVec2() { X = ScreenPosition.X - (textSize.X / 2f), Y = ScreenPosition.Y }, System.Drawing.Color.White, text);
                    }

                    // Name
                    if (ExternalSharp.Cheat.Globals.cfg.ESP_Name)
                    {
                        string pName = pEntity.Name;
                       // ExternalSharp.Cheat.Globals.Memory.ReadString((IntPtr)(pEntity.ClientPlayer + offset.PlayerName), out pName, 128);

                        //pName = CleanString(pName);
                      
                        ImVec2 textSize = new ImVec2();
                        ImGui.CalcTextSize(textSize, pName, null, false, -1.0f);

                        string invalidCharacters = "&♂%Èß@╬à?4B☺^Ü>Ðß@╬à?4B☺^Øß@╬?´4B☺0Ã?╬??♣☻♦?t¢½!X¼^ÎÄ?╬??♣`¼^Î►Å?╬??♦ ╬h¼^Îÿÿÿÿªªªª↕à€♀♣♦♥♠♦♥♥♦☻☺☻♦♦♣♣♣♥☻☻☻☻☺♥☻♥☺☻☻☻☻☺☼☼þø☼ø♥øøøø☼ø☼ø☼ø☼ø▼øø☼ø☺ø☼ø♥øø☼ø▼ø☺ø☼øø♥ü♥☺ø♥ø▼øøø☼ø☼øü♥øøø☺øð♥ø♥þø☼ø♥ðþ☺ü☼ø☼ø☺ø☼øþ♥ø☼ø☼ðøø▼ø♥øø♥ø☼ø☼øø";
                       
                        if (!IsStringCorrupted(pName, invalidCharacters))
                        {
                          
                            String(new ImVec2() { X = BoxTop.X - (textSize.X / 2f), Y = (BoxTop.Y - textSize.Y) - 2f }, System.Drawing.Color.White, pName);

                        }

                    }

                    
                }

            }

        }

        public void RenderSpectatorList()
        {
        
            if (Globals.cfg.SpectList && NameList.Count != 0)
            {
                //ImVec2 ListSize = new Utils.ImVec2(150, NameList.Count * 20);
                //ImGui.SetNextWindowPos(new Utils.ImVec2((Globals.GOverlay.Width / 2) - (ListSize.X / 2 ), 20), 0, new Utils.ImVec2(0, 0));
                //ImGui.SetNextWindowSize(ListSize, 0);

                int nameCount = NameList.Count;
                float textHeight = ImGui.GetTextLineHeightWithSpacing();
                ImVec2 ListSize = new Utils.ImVec2(150, (textHeight * nameCount) + 15);

                // Posicionar y dimensionar la ventana
                ImGui.SetNextWindowPos(new Utils.ImVec2((Globals.GOverlay.Width / 2) - (ListSize.X / 2) - 20, 20), 0, new Utils.ImVec2(0, 0));
                ImGui.SetNextWindowSize(ListSize, 0);

                ImGui.Begin("SpectatorList", ref Globals.cfg.SpectList, (int)ImGuiWindowFlags.NoScrollbar | (int)ImGuiWindowFlags.NoCollapse | (int)ImGuiWindowFlags.NoFocusOnAppearing | (int)ImGuiWindowFlags.NoTitleBar | (int)ImGuiWindowFlags.NoResize);

                ImVec2 contentRegionAvail = new ImVec2();
                ImGui.GetContentRegionAvail(contentRegionAvail);

                foreach (var name in NameList)
                {
                    ImVec2 textSize = new ImVec2();
                    ImGui.CalcTextSize(textSize, name, null, false, 0f);
                    ImGui.SetCursorPosX((contentRegionAvail.X - textSize.X) / 2);

                    ImGui.Text(name);
                }

                ImGui.End();
            }

        }

        public  string CleanString(string s)
        {
            return string.Join("_", s.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }

        public bool InGame()
        {
            uint CurrentPid = Utils.WinAPI.ForegroundProcessID();
            if (CurrentPid == ExternalSharp.Cheat.Globals.Memory.PID) { return true; } else {
                if (CurrentPid != ExternalSharp.Cheat.Globals.Memory.OverlayPID)
                {
                    return false;
                } else
                {
                    return true;
                }
            } 
        }

        public bool IsStringCorrupted(string inputString, string invalidCharacters)
        {
            // Check if any of the invalid characters are present in the input string
            foreach (char invalidChar in invalidCharacters)
            {
                if (inputString.Contains(invalidChar))
                {
                    return true; // String is corrupted
                }
            }

            // If no invalid characters are found, the string is not corrupted
            return false;
        }

        public Vector3 Multiply(Vector3 vector, SharpDX.Matrix mat)
        {
            return new Vector3(
                mat.M11 * vector.X + mat.M21 * vector.Y + mat.M31 * vector.Z,
                mat.M12 * vector.X + mat.M22 * vector.Y + mat.M32 * vector.Z,
                mat.M13 * vector.X + mat.M23 * vector.Y + mat.M33 * vector.Z
            );
        }

        public void DrawAABB(AxisAlignedBox aabb, SharpDX.Matrix transform, RawColorBGRA color)
        {
            Vector3 m_Position = new Vector3(transform.M41, transform.M42, transform.M43);
            Vector3 fld = Multiply(new Vector3(aabb.Min.X, aabb.Min.Y, aabb.Min.Z), transform) + m_Position;
            Vector3 brt = Multiply(new Vector3(aabb.Max.X, aabb.Max.Y, aabb.Max.Z), transform) + m_Position;
            Vector3 bld = Multiply(new Vector3(aabb.Min.X, aabb.Min.Y, aabb.Max.Z), transform) + m_Position;
            Vector3 frt = Multiply(new Vector3(aabb.Max.X, aabb.Max.Y, aabb.Min.Z), transform) + m_Position;
            Vector3 frd = Multiply(new Vector3(aabb.Max.X, aabb.Min.Y, aabb.Min.Z), transform) + m_Position;
            Vector3 brb = Multiply(new Vector3(aabb.Max.X, aabb.Min.Y, aabb.Max.Z), transform) + m_Position;
            Vector3 blt = Multiply(new Vector3(aabb.Min.X, aabb.Max.Y, aabb.Max.Z), transform) + m_Position;
            Vector3 flt = Multiply(new Vector3(aabb.Min.X, aabb.Max.Y, aabb.Min.Z), transform) + m_Position;
            
            if (!ExternalSharp.Cheat.Globals.GameSDK.WorldToScreen(fld, out fld) || !ExternalSharp.Cheat.Globals.GameSDK.WorldToScreen(brt, out brt)
                || !ExternalSharp.Cheat.Globals.GameSDK.WorldToScreen(bld, out bld) || !ExternalSharp.Cheat.Globals.GameSDK.WorldToScreen(frt, out frt)
                || !ExternalSharp.Cheat.Globals.GameSDK.WorldToScreen(frd, out frd) || !ExternalSharp.Cheat.Globals.GameSDK.WorldToScreen(brb, out brb)
                || !ExternalSharp.Cheat.Globals.GameSDK.WorldToScreen(blt, out blt) || !ExternalSharp.Cheat.Globals.GameSDK.WorldToScreen(flt, out flt))
                return;



            DrawLine(new Utils.ImVec2(fld.X, fld.Y), new Utils.ImVec2(flt.X, flt.Y), color, 1f);
            DrawLine(new Utils.ImVec2(flt.X, flt.Y), new Utils.ImVec2(frt.X, frt.Y), color, 1f);
            DrawLine(new Utils.ImVec2(frt.X, frt.Y), new Utils.ImVec2(frd.X, frd.Y), color, 1f);
            DrawLine(new Utils.ImVec2(frd.X, frd.Y), new Utils.ImVec2(fld.X, fld.Y), color, 1f);
            DrawLine(new Utils.ImVec2(bld.X, bld.Y), new Utils.ImVec2(blt.X, blt.Y), color, 1f);
            DrawLine(new Utils.ImVec2(blt.X, blt.Y), new Utils.ImVec2(brt.X, brt.Y), color, 1f);
            DrawLine(new Utils.ImVec2(brt.X, brt.Y), new Utils.ImVec2(brb.X, brb.Y), color, 1f);
            DrawLine(new Utils.ImVec2(brb.X, brb.Y), new Utils.ImVec2(bld.X, bld.Y), color, 1f);
            DrawLine(new Utils.ImVec2(fld.X, fld.Y), new Utils.ImVec2(bld.X, bld.Y), color, 1f);
            DrawLine(new Utils.ImVec2(frd.X, frd.Y), new Utils.ImVec2(brb.X, brb.Y), color, 1f);
            DrawLine(new Utils.ImVec2(flt.X, flt.Y), new Utils.ImVec2(blt.X, blt.Y), color, 1f);
            DrawLine(new Utils.ImVec2(frt.X, frt.Y), new Utils.ImVec2(brt.X, brt.Y), color, 1f);
        }

       
        string[] FramerateList = { "<30", "=<40", ">45", "Unlocked" };

        // Menu String
        string[] BoxList = { "2D Box", "2D Corner Box" };
        string[] BoneList = { "Head", "Spine" };
        string[] bAimTupeText = { "Crosshair", "Distance" };
        string[] SkeletonColorModeList = { "ESP", "User" };
        string[] CrosshairList = { "Cross", "Circle" };
        string[] AimKeyTypeList = { "and", "or" };

        public float[] ESP_NormalF = { 0f, 0f, 0f, 0f };
        public float[] ESP_VisibleF = { 0f, 0f, 0f, 0f };
        public float[] ESP_TeamF = { 0f, 0f, 0f, 0f };
        public float[] ESP_FilledF = { 0f, 0f, 0f, 0f };
        public float[] ESP_SkeletonF = { 0f, 0f, 0f, 0f };

        public void UpdateColors()
        {
            ESP_NormalF = new float[] { ESP_Normal.R, ESP_Normal.G, ESP_Normal.B, ESP_Normal.A };
            ESP_VisibleF = new float[] { ESP_Visible.R, ESP_Visible.G, ESP_Visible.B, ESP_Visible.A };
            ESP_TeamF = new float[] { ESP_Team.R, ESP_Team.G, ESP_Team.B, ESP_Team.A };
            ESP_FilledF = new float[] { ESP_Filled.R, ESP_Filled.G, ESP_Filled.B, ESP_Filled.A };
            ESP_SkeletonF = new float[] { ESP_Skeleton.R, ESP_Skeleton.G, ESP_Skeleton.B, ESP_Skeleton.A };
        }

        public List<string> MSAA = new List<string>();

        public void RenderMenu()
        {
            bool AimbotOpen = true;
            bool VisualOpen = true;
            bool MiscOpen = true;
            bool DeveloperOpen = true;

            ImGui.SetNextWindowSize(new Utils.ImVec2(850, 500), 0);
            ImGui.Begin("ProjectLocker - Battlefield4 [ EXTERNAL ] | C# Port By Destroyer", ref Globals.cfg.ShowMenu, (int)ImGuiWindowFlags.NoResize | (int)ImGuiWindowFlags.NoCollapse);

            //---// Clild 0 //-----------------------------------//
            ImVec2 BaseChildRegion = new ImVec2();
                ImGui.GetContentRegionAvail(BaseChildRegion);
                BaseChildRegion.X = 150f;

            ImGui.BeginChildStr("##BaseChild", BaseChildRegion, false, 0);


            ImGui.Spacing();


            ImVec2 SomeChildRegion = new ImVec2();
            ImGui.GetContentRegionAvail(SomeChildRegion);
            ImGui.BeginChildStr("##SomeChild", SomeChildRegion, true, 0);

            ImGui.SetCursorPosY(SomeChildRegion.Y - 130f);


            // Exit
            ImGui.Separator();
            ImGui.Spacing();
            ImVec2 ExitRegion = new ImVec2();
            ImGui.GetContentRegionAvail(ExitRegion);
            ExitRegion.Y = 30f;
            if (ImGui.Button("Exit", ExitRegion))
                Process.GetCurrentProcess().Kill();

            ImGui.EndChild();
            ImGui.EndChild();
            //---// Clild 0 //-----------------------------------//

            ImGui.SameLine(0,2f);

            //---// Clild 1 //-----------------------------------//
            ImVec2 BaseChild2Region = new ImVec2();
            ImGui.GetContentRegionAvail(BaseChild2Region);
            ImGui.BeginChildStr("BaseChild##2", BaseChild2Region, false, 0);

            ImGuiStyle style = ImGui.GetStyle();
            ImVec2 FramePadding = style.FramePadding;
            style.FramePadding = new Utils.ImVec2(40, 8);

            if (ImGui.BeginTabBar("##ContextTabBar", 0))
            {
                style.FramePadding = new Utils.ImVec2(40, 8);
             
                if (ImGui.BeginTabItem("   AimBot   ", ref AimbotOpen, (int)ImGuiWindowFlags.NoTitleBar))
                {
                    /*---------------*/
                    style.FramePadding = FramePadding;
                    ImGui.Spacing();
                    ImVec2 LeftAimBaseRegion = new ImVec2();
                    ImGui.GetContentRegionAvail(LeftAimBaseRegion);
                    LeftAimBaseRegion.X = LeftAimBaseRegion.X / 2f - 8f;
                    ImGui.BeginChildStr("##LeftAimBase", LeftAimBaseRegion, false, 0);

                    ImGui.Text("  AimBot");
                    ImGui.Separator();
                    ImGui.Spacing();
                    ImGui.Checkbox("Check Player IsSpectator", ref ExternalSharp.Cheat.Globals.cfg.CheckPlayerIsSpectator);
                    ImGui.Checkbox("Vehicle Aim", ref ExternalSharp.Cheat.Globals.cfg.VehAim);
                    ImGui.Checkbox("AimBot", ref Globals.cfg.AimBot);
                    ImGui.Checkbox("Aim at Team", ref Globals.cfg.AimAtTeam);

                    ImGui.NewLine();
                    ImGui.Spacing();

                    ImGui.Text("  AimBot Config");
                    ImGui.Separator();
                    ImGui.Spacing();
                    ImGui.Checkbox("Visibility Check", ref Globals.cfg.VisCheck);
                    ImGui.ComboStr_arr("AimBone", ref Globals.cfg.AimTargetBone, BoneList, BoneList.Count(), BoneList.Count());

                    ImGui.NewLine();
                    ImGui.Spacing();

                    ImGui.Text("  FOV");
                    ImGui.Separator();
                    ImGui.Spacing();
                    ImGui.Checkbox("Draw FOV", ref Globals.cfg.DrawFov);
                    ImGui.Checkbox("Rainbow FOV", ref Globals.cfg.RainbowFov);
                    ImGui.Checkbox("Fov Filled", ref Globals.cfg.FovFilled);
                    ImGui.SliderFloat("FOV", ref Globals.cfg.AimFov, 25f, 650f, "", 0);
                    //ImGui.ColorEdit4("FOV Color", ref  FOV_User.Value.x, 0);

                    ImGui.EndChild();
                    /*---------------*/
                    ImGui.SameLine(0,5);
                    /*---------------*/
                    ImVec2 RightAimBaseRegion = new ImVec2();
                    ImGui.GetContentRegionAvail(RightAimBaseRegion);
                    ImGui.BeginChildStr("##RightAimBase", RightAimBaseRegion, false, 0);

                    ImGui.Text("  AimBot Setting");
                    ImGui.Separator();
                    ImGui.Spacing();
                    ImGui.SliderInt("Smooth", ref Globals.cfg.Smooth, 10, 100, "", 0);
                    ImGui.SliderFloat("Distance", ref Globals.cfg.Aim_MaxDistance, 15f, 600f, "", 0);
                    ImGui.ComboStr_arr("AimType", ref Globals.cfg.AimType, bAimTupeText, bAimTupeText.Count(), bAimTupeText.Count());

                    ImGui.NewLine();
                    ImGui.Spacing();

                    ImGui.Text("  KeyBind");
                    ImGui.Separator();
                    ImGui.Spacing();


                    ImGui.EndChild();
                    /*---------------*/

                    ImGui.EndTabItem();
                }

                style.FramePadding = new Utils.ImVec2(40, 8);
                if (ImGui.BeginTabItem("   Visual   ", ref VisualOpen, 0))
                {
                    /*---------------*/
                    style.FramePadding = FramePadding;
                    ImGui.Spacing();
                    ImVec2 LeftVisualBaseRegion = new ImVec2();
                    ImGui.GetContentRegionAvail(LeftVisualBaseRegion);
                    LeftVisualBaseRegion.X = LeftVisualBaseRegion.X / 2f - 8f;
                    ImGui.BeginChildStr("##LeftVisualBase", LeftVisualBaseRegion, false, 0);

                    ImGui.Text("   Overlay");
                    ImGui.Separator();
                    ImGui.Spacing();
                    ImGui.SliderFloat("Opacity " + string.Format("({0})", Math.Round(Globals.cfg.Opacity).ToString()), ref Globals.cfg.Opacity, 10f, 100f, "", 0);


                    Debug.WriteLine(Globals.GOverlay.Opacity.ToString());

                    ImGui.NewLine();
                    ImGui.Spacing();


                    ImGui.Text("  Visual");
                    ImGui.Separator();
                    ImGui.Spacing();

                    ImGui.Checkbox("Player ESP", ref Globals.cfg.PlayerESP);
                    ImGui.Checkbox("Team ESP", ref Globals.cfg.TeamESP);
                    ImGui.Checkbox("Vehicle ESP", ref Globals.cfg.VehicleESP);

                    ImGui.NewLine();
                    ImGui.Spacing();

                    ImGui.Text("  ESP Options");
                    ImGui.Separator();
                    ImGui.Spacing();

                    ImGui.Checkbox("Check Spectator", ref ExternalSharp.Cheat.Globals.cfg.CheckSpectator);
                    ImGui.Checkbox("Box", ref Globals.cfg.ESP_Box);
                    ImGui.Checkbox("BoxFilled", ref Globals.cfg.ESP_BoxFilled);
                    ImGui.Checkbox("Line", ref Globals.cfg.ESP_Line);
                    ImGui.Checkbox("Distance", ref Globals.cfg.ESP_Distance);
                    ImGui.Checkbox("Name", ref Globals.cfg.ESP_Name);
                    ImGui.Checkbox("HealthBar", ref Globals.cfg.ESP_HealthBar);

                    ImGui.EndChild();
                    /*---------------*/
                    ImGui.SameLine(0, 5);
                    /*---------------*/
                    ImVec2 ContentRegionAvail = new ImVec2();
                    ImGui.GetContentRegionAvail(ContentRegionAvail);
                    ImGui.BeginChildStr("##RightVisualBase", ContentRegionAvail, false, 0);

                    ImGui.Text("  ESP Setting");
                    ImGui.Separator();
                    ImGui.Spacing();

                    ImGui.SliderFloat("Distance", ref Globals.cfg.ESP_MaxDistance, 25f, 2000f, "", 0);
                    ImGui.ComboStr_arr("Box Style", ref Globals.cfg.ESP_BoxType, BoxList, BoxList.Count(), BoxList.Count());

                    ImGui.NewLine();
                    ImGui.Spacing();

                    ImGui.Text("  ESP Colors");
                    ImGui.Separator();
                    ImGui.Spacing();

                    ImGui.ColorEdit4("Normal",  ESP_NormalF , 0);
                    ESP_Normal  = System.Drawing.Color.FromArgb(255, (int)ESP_NormalF[0], (int)ESP_NormalF[1], (int)ESP_NormalF[2]);
                    ImGui.ColorEdit4("Visible",  ESP_VisibleF, 0); 
                    ESP_Visible = System.Drawing.Color.FromArgb(255, (int)ESP_VisibleF[0], (int)ESP_VisibleF[1], (int)ESP_VisibleF[2]);
                    ImGui.ColorEdit4("Team",  ESP_TeamF, 0); 
                    ESP_Team = System.Drawing.Color.FromArgb(255, (int)ESP_TeamF[0], (int)ESP_TeamF[1], (int)ESP_TeamF[2]);
                    ImGui.ColorEdit4("BoxFilled",  ESP_FilledF, 0);
                    ESP_Filled = System.Drawing.Color.FromArgb(255, (int)ESP_FilledF[0], (int)ESP_FilledF[1], (int)ESP_FilledF[2]);
                    ImGui.ColorEdit4("Skeleton",  ESP_SkeletonF , 0);
                    ESP_Skeleton = System.Drawing.Color.FromArgb(255, (int)ESP_SkeletonF[0], (int)ESP_SkeletonF[1], (int)ESP_SkeletonF[2]);

                    ImGui.ComboStr_arr("Skeleton Color", ref Globals.cfg.ESP_SkeletonColor, SkeletonColorModeList, SkeletonColorModeList.Count(), SkeletonColorModeList.Count());

                    ImGui.EndChild();
                    /*---------------*/

                    ImGui.EndTabItem();
                }

                style.FramePadding = new Utils.ImVec2(40, 8);
                if (ImGui.BeginTabItem("    Misc    ", ref MiscOpen, 0))
                {
                    /*---------------*/
                    style.FramePadding = FramePadding;
                    ImGui.Spacing();
                    ImVec2 LeftMiscBaseAvail = new ImVec2();
                    ImGui.GetContentRegionAvail(LeftMiscBaseAvail);
                    LeftMiscBaseAvail.X = LeftMiscBaseAvail.X / 2f - 12f;
                    ImGui.BeginChildStr("##LeftMiscBase", LeftMiscBaseAvail, false, 0);

                    ImGui.Text("  System");
                    ImGui.Separator();
                    ImGui.Spacing();
                    ImGui.Checkbox("StreamProof", ref Globals.cfg.StreamProof);
                    ImGui.Checkbox("SpectatorList", ref Globals.cfg.SpectList);
                    ImGui.Checkbox("Crosshair", ref Globals.cfg.Crosshair);
                    ImGui.ComboStr_arr("Type", ref Globals.cfg.CrosshairType, CrosshairList, CrosshairList.Count(), CrosshairList.Count());
                    ImGui.SliderInt("RainbowRate", ref Globals.cfg.RainbowRate, 1, 200, "", 0);
                   
                    ImGui.NewLine();
                    ImGui.Spacing();

                    ImGui.Checkbox("Blur On Gui", ref Globals.cfg.BlurOnGUI);

                    ImGui.Text("  Overlay Framerate");
                    ImGui.Separator();
                    ImGui.Spacing();
                    ImGui.ComboStr_arr("oFramerate", ref Globals.cfg.FramerateType, FramerateList, FramerateList.Count(), FramerateList.Count());
                    ImVec2 FramerateRegion = new ImVec2();
                    ImGui.GetContentRegionAvail(FramerateRegion);
                    FramerateRegion.Y = 30f;
                    if (ImGui.Button("Apply", FramerateRegion))
                        new Thread(() => { this.UpdateFramerate(); }).Start();


                    ImGui.EndChild();
                    /*---------------*/
                    ImGui.SameLine(0,5);
                    /*---------------*/
                    ImVec2 RightMiscBaseAvail = new ImVec2();
                    ImGui.GetContentRegionAvail(RightMiscBaseAvail);
                    ImGui.BeginChildStr("##RightMiscBase", RightMiscBaseAvail, false,0);

                    ImGui.Text("   SwayModify");
                    ImGui.Separator();
                    ImGui.Spacing();
                    ImGui.Checkbox("SwayModify", ref Globals.cfg.SwayModify);
                    ImGui.SliderFloat("Recoil/Spread", ref Globals.cfg.ModVal, 0f, 1f, "", 0);

                    ImGui.NewLine();
                    ImGui.Spacing();

                    ImGui.Text("   DamageHack");
                    ImGui.Separator();
                    ImGui.Spacing();
                    ImGui.SliderInt("BulletPerShell", ref Globals.cfg.ModBPS, 1, 30, "", 0);
                    ImGui.Text("CurrentBPS : " + Globals.cfg.CurrentBPS);
                    ImVec2 ApplyRegion = new ImVec2();
                    ImGui.GetContentRegionAvail(ApplyRegion);
                    ApplyRegion.Y = 30f;
                    if (ImGui.Button("Apply", ApplyRegion))
                        new Thread(() => { this.SetBPS(Globals.cfg.ModBPS); }).Start();


                    ImGui.NewLine();
                    ImGui.Spacing();

                    ImGui.Text("  Overlay MSAA");
                    ImGui.Separator();
                    ImGui.Spacing();
                    ImGui.ComboStr_arr("MSAA (x" + presentParams.MultiSampleQuality  + ")", ref Globals.cfg.MSAA_Level, MSAA.ToArray(), MSAA.Count(), MSAA.Count());
                    ImVec2 MSAARegion = new ImVec2();
                    ImGui.GetContentRegionAvail(MSAARegion);
                    MSAARegion.Y = 30f;

                    if (RuntimeMSAA == false)
                    {
                        if (ImGui.Button("ApplyEx", MSAARegion))
                        {
                            this.UpdateMSAA(Globals.cfg.MSAA_Level);
                        }
                    }
                     



                    ImGui.EndChild();
                    /*---------------*/

                    ImGui.EndTabItem();
                }

                style.FramePadding = new Utils.ImVec2(40, 8);
                if (ImGui.BeginTabItem("Developer", ref DeveloperOpen, 0))
                {
                    /*---------------*/
                    style.FramePadding = FramePadding;
                    ImGui.Spacing();

                    ImVec2 LeftDevBaseAvail = new ImVec2();
                    ImGui.GetContentRegionAvail(LeftDevBaseAvail);
                    LeftDevBaseAvail.X = LeftDevBaseAvail.X / 2f - 8f;
                    ImGui.BeginChildStr("##LeftDevBase", LeftDevBaseAvail, false, 0);

                    ImGui.Text("  Process");
                    ImGui.Separator();
                    ImGui.Spacing();
                    ImGui.Text("PID : " + Globals.Memory.PID);

                    ImGui.NewLine();
                    ImGui.Spacing();

                    ImGui.Text(" Developers");
                    ImGui.Separator();
                    ImGui.Spacing();
                    ImGui.Text("C++ ProjectLocker Dev");
                    ImGui.Separator();
                    ImGui.Spacing();
                    ImGui.Text("wallhax_ena");
                    ImGui.Separator();
                    ImGui.Spacing();
                    ImGui.Text("C# ProjectLocker Port");
                    ImGui.Separator();
                    ImGui.Spacing();
                    ImGui.Text("Destroyer");

                    ImGui.EndChild();
                    /*---------------*/
                    ImGui.SameLine(0,5);
                    /*---------------*/

                    ImVec2 RightDevBaseAvail = new ImVec2();
                    ImGui.GetContentRegionAvail(RightDevBaseAvail);
                    ImGui.BeginChildStr("##RightDevBase", RightDevBaseAvail, false, 0);



                    ImGui.EndChild();
                    /*---------------*/

                    ImGui.EndTabItem();
                }

                style.FramePadding = FramePadding;
                ImGui.EndTabBar();
            }

            ImGui.EndChild();
            //---// Clild 1 //-----------------------------------//

            ImGui.End();
        }

        public SharpDX.Direct3D9.PresentParameters presentParams = new SharpDX.Direct3D9.PresentParameters()
        {
            Windowed = true,
            SwapEffect = SharpDX.Direct3D9.SwapEffect.Discard,
            BackBufferFormat = SharpDX.Direct3D9.Format.A8R8G8B8,
            MultiSampleType = SharpDX.Direct3D9.MultisampleType.None,
            MultiSampleQuality = 0
        };

        bool RuntimeMSAA = false;
        public void UpdateMSAA(int Level)
        {
            try
            {
                if (!RuntimeMSAA) {
                    RuntimeMSAA = true;

                    if (Level == 0)
                    {
                        presentParams.MultiSampleQuality = 0;
                        presentParams.MultiSampleType = (MultisampleType)0;
                        Globals.GOverlay.Reset();
                    }
                    else
                    {
                        string Content = Regex.Match(MSAA[Level], @"\d+").Value;
                        int MSAA_Level = int.Parse(Content);
                        presentParams.MultiSampleQuality = MSAA_Level;
                        presentParams.MultiSampleType = (MultisampleType)MSAA_Level;
                        Globals.GOverlay.Reset();
                        Console.WriteLine("Current MSAA Level : x" + Content);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("MSAA " + MSAA[Level] + " Error: " + ex.Message);
            }
            new Thread(() => { System.Threading.Thread.Sleep(3000); RuntimeMSAA = false; }).Start();
           
        }

        public void UpdateFramerate()
        {
            try
            {
                if (Globals.cfg.FramerateType == 0)
                {
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
                }
                else if (Globals.cfg.FramerateType == 1)
                {
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
                }
                else if (Globals.cfg.FramerateType == 2)
                {
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
                }
                else if (Globals.cfg.FramerateType == 3)
                {
                    Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Framerate Error: " + ex.Message);
            }
           
        }

        public bool SetBPS(int value)
        {
            long Weapon = ExternalSharp.Cheat.Globals.Memory.Read<long>((IntPtr)offset.ClientWeapons);

            if (Weapon == 0)
                return false;

            long weaponPtr = ExternalSharp.Cheat.Globals.Memory.Read<long>((IntPtr)(Weapon + 0x128));
            long FiringFunctionData = ExternalSharp.Cheat.Globals.Memory.Read<long>((IntPtr)(weaponPtr + 0x10));

            if (FiringFunctionData == 0)
                return false;

            ExternalSharp.Cheat.Globals.Memory.Write<int>((IntPtr)(FiringFunctionData + 0xD8), value);

            return true;
        }

    }
}
