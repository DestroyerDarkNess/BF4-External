using DearImguiSharp;
using EasyImGui;
using ExternalSharp.Cheat;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace ExternalSharp
{

    public static class Program
    {

        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();
        public static bool FreeCMD = false;

        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
          
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

                Overlay OverlayWindow = new Overlay() { EnableDrag = false, ResizableBorders = false, NoActiveWindow = true, ShowInTaskbar = false, AutoPresentParams = false};
                OverlayWindow.presentParams = Renders.presentParams;
                Globals.GOverlay = OverlayWindow;

                var Theme = new Themer.ThemerApplier(Globals.GOverlay.Handle);
                Themer.Themes CurrentTheme = Themer.Themes.None;

                OverlayWindow.OnImGuiReady += (object sender, bool Status) =>
                {

                    if (Status)
                    {
                        Process GameProc = Process.GetProcessById(Globals.Memory.PID);

                        OverlayWindow.Imgui.ConfigContex += delegate {
                            ExternalSharp.Cheat.Globals.cfg.Run = true;
                            OverlayWindow.Text = GameProc.MainWindowTitle;
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

                            //Check Features.cs
                            Thread AimThread = new Thread(() => { Features.AimBotOld(); });
                            AimThread.Priority = ThreadPriority.AboveNormal;
                            AimThread.SetApartmentState(ApartmentState.MTA);
                            AimThread.Start();

                            //Removed for more performance. Check the render Thread.
                            //Thread MiscThread = new Thread(() => { Features.Misc(); });
                            //MiscThread.Priority = ThreadPriority.AboveNormal;
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
                            Globals.GOverlay.Opacity = ((double)Math.Round(Globals.cfg.Opacity) / 100);

                            SharpDX.Direct3D9.Device Device3D = Globals.GOverlay.D3DDevice;


                            Renders.MSAA.Add("None");

                            for (int i = 1;  i < 17;)
                            {
                                MultisampleType MultiSample = (MultisampleType)i;
                                bool IsCompatible =   Device3D.Direct3D.CheckDeviceMultisampleType(0, DeviceType.Hardware, Renders.presentParams.BackBufferFormat, true, MultiSample);
                                
                                if (IsCompatible)
                                {
                                    Renders.MSAA.Add("x" + i);
                                }

                                i++;
                            }

             
                           

                            //Test 

                            //Test NameList
                            //Renders.NameList.AddRange(new []{ "Destroyer", "Mario Peña", "Megatron", "MASAWA", "lOOSDO", "[RPD]OverKill", "[RPD]Dissamble"});

                            //Test UserMode Overlay Detector | Incomplete, do not use.
                            //Thread MiscThread = new Thread(() => { Test.OverlayDetector.Scan(); });
                            //MiscThread.Priority = ThreadPriority.AboveNormal;
                            //MiscThread.SetApartmentState(ApartmentState.MTA);
                            //MiscThread.Start();

                            return true;
                        };

                       
                        OverlayWindow.Imgui.Render += delegate {

                            if (GameProc.HasExited )
                                Environment.Exit(0);

                            OverlayWindow.FitTo(GameProc.MainWindowHandle, true);
                            OverlayWindow.PlaceAbove(GameProc.MainWindowHandle);
                            OverlayWindow.Interactive(Globals.cfg.ShowMenu);
                          
                            Renders.RenderESP();
                            Renders.RenderInfo();
                            Renders.RenderSpectatorList();
                            Features.Misc();

                            if (Globals.cfg.ShowMenu == true ) {
                            
                                if (Globals.cfg.BlurOnGUI == true && CurrentTheme == Themer.Themes.None)
                                {
                                    CurrentTheme = Themer.Themes.AeroGlass;
                                    Theme.Apply(CurrentTheme);
                                }

                                Globals.GOverlay.Opacity = (double)1; Renders.RenderMenu(); 
                            } else {

                                if (CurrentTheme != Themer.Themes.None)
                                {
                                    CurrentTheme = Themer.Themes.None;
                                    Theme.Apply(CurrentTheme);
                                }

                                Globals.GOverlay.Opacity = ((double)Math.Round(Globals.cfg.Opacity) / 100); 
                            }

                            if(FreeCMD == false) { FreeCMD = true; FreeConsole(); }

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
