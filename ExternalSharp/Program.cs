using DearImguiSharp;
using EasyImGui;
using ExternalSharp.Cheat;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExternalSharp
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Console.WriteLine("Hello Sir! .... Please WAIT...");
            Globals.Memory.OnMemReady += OnMemoryResut;
            Globals.Memory.Init();
        }


        public static void OnMemoryResut(object Mem_sender, bool Mem_Status)
        {
            Console.WriteLine("OnMemoryResut : " + Mem_Status);
            if (Mem_Status)
            {

                Globals.GameSDK = new Cheat.SDK(Globals.Memory);
                Render Renders = new Render();
                Features Features = new Features();
                Renders.UpdateFramerate();
                Renders.UpdateColors();
             

                Overlay OverlayWindow = new Overlay() { EnableDrag = false, ResizableBorders = false, NoActiveWindow = true, ShowInTaskbar = false};
                Globals.GOverlay = OverlayWindow;

                OverlayWindow.OnImGuiReady += (object sender, bool Status) =>
                {

                    if (Status)
                    {
                        Process GameProc = Process.GetProcessById(Globals.Memory.PID);

                        OverlayWindow.Imgui.ConfigContex += delegate {
                            ExternalSharp.Cheat.Globals.cfg.Run = true;

                            //DearImguiSharp.ImGui.StyleColorsLight(null);
                            var DarkTheme = new Utils.Style.DarkTheme();
                            DarkTheme.ApplyColors();

                            var style = ImGui.GetStyle();

                            ImVec4[] styleColors = style.Colors;
                            styleColors[(int)ImGuiCol.CheckMark].W = 1.0f;
                            styleColors[(int)ImGuiCol.CheckMark].X = 1.0f;
                            styleColors[(int)ImGuiCol.CheckMark].Y = 1.0f;
                            styleColors[(int)ImGuiCol.CheckMark].Z = 1.0f;

                            styleColors[(int)ImGuiCol.FrameBg].W = 0.0f;
                            styleColors[(int)ImGuiCol.FrameBg].X = 0.0f;
                            styleColors[(int)ImGuiCol.FrameBg].Y = 0.0f;
                            styleColors[(int)ImGuiCol.FrameBg].Z = 0.0f;

                            style.WindowRounding = 5.0f;
                            style.FrameRounding = 5.0f;
                            style.FrameBorderSize = 1.0f;

                            Thread AimThread = new Thread(() => { Features.AimBot(); });
                            AimThread.Priority = ThreadPriority.Highest;
                            AimThread.SetApartmentState(ApartmentState.MTA);
                            AimThread.Start();

                            //Removed for more performance. Check the render Thread.
                            //Thread MiscThread = new Thread(() => { Features.Misc(); });
                            //MiscThread.Priority = ThreadPriority.Highest;
                            //MiscThread.SetApartmentState(ApartmentState.MTA);
                            //MiscThread.Start();

                            Thread KeysThread = new Thread(() =>
                            {

                                while (ExternalSharp.Cheat.Globals.cfg.Run)
                                {

                                    if (Utils.Config.IsKeyDown(Keys.Insert)) { Globals.cfg.ShowMenu = !Globals.cfg.ShowMenu; }
                                    Thread.Sleep(100);
                                }

                            });
                            //KeysThread.Priority = ThreadPriority.Normal;
                            KeysThread.SetApartmentState(ApartmentState.STA);
                            KeysThread.Start();

                            //OverlayWindow.Imgui.IO.ConfigFlags |= (int)ImGuiConfigFlags.ViewportsEnable;


                            return true;
                        };

                       
                        OverlayWindow.Imgui.Render += delegate {

                            if (GameProc.HasExited )
                                Environment.Exit(0);

                            OverlayWindow.FitTo(GameProc.MainWindowHandle, true);
                            OverlayWindow.PlaceAbove(GameProc.MainWindowHandle);
                            OverlayWindow.Interactive(Globals.cfg.ShowMenu);
                          
                            Renders.RenderInfo();
                            Renders.RenderESP();
                            Features.Misc();

                            if (Globals.cfg.ShowMenu == true ) { Renders.RenderMenu(); }

                            return true;

                        };

                    }
                };

             
                Application.Run(OverlayWindow);

            } else
            {

                MessageBox.Show("Memory Failed");

            }


        }


    }
}
