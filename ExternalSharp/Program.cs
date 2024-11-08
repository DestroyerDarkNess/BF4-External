using DearImguiSharp;
using EasyImGui;
using ExternalSharp.Cheat;
using ExternalSharp.Utils;
using ProcessHacker.Common;
using ProcessHacker.Native;
using ProcessHacker.Native.Api;
using ProcessHacker.Native.Objects;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VNX;
using static SharpDX.Windows.RenderLoop;

namespace ExternalSharp
{

    public static class Program
    {

        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();
        public static bool FreeCMD = false;
        public static TokenElevationType ElevationType;

        [DllImport("user32.dll")]
        public static extern uint SetWindowDisplayAffinity(IntPtr hwnd, uint dwAffinity);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);

        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                using (var thandle = ProcessHandle.Current.GetToken())
                {
                    thandle.TrySetPrivilege("SeDebugPrivilege", SePrivilegeAttributes.Enabled);
                    thandle.TrySetPrivilege("SeIncreaseBasePriorityPrivilege", SePrivilegeAttributes.Enabled);
                    thandle.TrySetPrivilege("SeLoadDriverPrivilege", SePrivilegeAttributes.Enabled);
                    thandle.TrySetPrivilege("SeRestorePrivilege", SePrivilegeAttributes.Enabled);
                    thandle.TrySetPrivilege("SeShutdownPrivilege", SePrivilegeAttributes.Enabled);
                    thandle.TrySetPrivilege("SeTakeOwnershipPrivilege", SePrivilegeAttributes.Enabled);

                    if (OSVersion.HasUac)
                    {
                        try { ElevationType = thandle.GetElevationType(); }
                        catch { ElevationType = TokenElevationType.Full; }

                        if (ElevationType == TokenElevationType.Default &&
                            !(new WindowsPrincipal(WindowsIdentity.GetCurrent())).
                                IsInRole(WindowsBuiltInRole.Administrator))
                            ElevationType = TokenElevationType.Limited;
                        else if (ElevationType == TokenElevationType.Default)
                            ElevationType = TokenElevationType.Full;
                    }
                    else
                    {
                        ElevationType = TokenElevationType.Full;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log(ex);
            }

            Console.WriteLine("What you want do?");
            Console.WriteLine("1 - Load in Other process.");
            Console.WriteLine("2 - Load Normally.");
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);

            char R = Console.ReadKey().KeyChar;
            Console.WriteLine();

            switch (R)
            {
                case '1':
                    LoadInto = true;
                    break;
                case '2':
                    LoadInto = false;
                    break;
                default:
                    Console.WriteLine("Incorrect option, option 2 is automatically selected.");
                    LoadInto = false;
                    break;
            }

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);



            Globals.Memory.OnMemReady += OnMemoryResut;
            Globals.Memory.Init();
        }

        public static bool LoadInto = false;
        public static void InjecInProc(RemoteControl Control, Process Process)
        {
            Control.LockEntryPoint();

            string CurrentAssembly = Assembly.GetExecutingAssembly().Location;

            int Ret = Control.CLRInvoke(CurrentAssembly, typeof(Program).FullName, "EntryPoint", Process.GetCurrentProcess().Id.ToString());

            Control.UnlockEntryPoint();

            Environment.Exit(0);
        }

        public static int EntryPoint(string Arg)
        {
            Process.GetProcessById(int.Parse(Arg))?.Kill();
            Globals.Memory.OnMemReady += OnMemoryResut;
            Globals.Memory.Init();
            return int.MaxValue;
        }


        public static void OnMemoryResut(object Mem_sender, bool Mem_Status)
        {
            Console.WriteLine("OnMemoryResut : " + Mem_Status);
            if (Mem_Status)
            {
                if (LoadInto) {
                    Process process; 
                    
                    VNX.RemoteControl Control = new VNX.RemoteControl("calc", out process);

                    bool Compatible = Control.IsCompatibleProcess();

                    Console.WriteLine(process.Is64Bits() ? "x64 Detected" : "x86 Detected");
                    Console.WriteLine($"The Game {(Compatible ? "is" : "isn't")} compatible with this build.");

                    Control.WaitInitialize();

                    if (Compatible)
                    {
                        InjecInProc(Control, process);
                        Globals.Memory.pHandle.Dispose();
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Impossible to Inject, Continuing externally.  Press any key to continue...");
                    }

                    Console.ReadKey();
                }

               
                Globals.GameSDK = new Cheat.SDK(Globals.Memory);
                Render Renders = new Render();
                Features Features = new Features();
              
                Overlay OverlayWindow = new Overlay() { EnableDrag = false, ShowInTaskbar = false};
                OverlayWindow.PresentParams = Renders.presentParams;
                //OverlayWindow.Load += (object sender, EventArgs e) => {
                //    var D3DDevice = new Device(new Direct3D(), 0, SharpDX.Direct3D9.DeviceType.Hardware, OverlayWindow.Handle, CreateFlags.MixedVertexProcessing, Renders.presentParams);
                //    OverlayWindow.D3DDevice = D3DDevice;
                //};

                OverlayWindow.Text = "OBS Studio v4.3.5";
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
                            //OverlayWindow.MakeOverlayChild(OverlayWindow.Handle, GameProc.MainWindowHandle);

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
                            //Removed for more performance. Check the render Thread.
                            //Thread MiscThread = new Thread(() => { Features.Misc(); });
                            //MiscThread.Priority = ThreadPriority.AboveNormal;
                            //MiscThread.SetApartmentState(ApartmentState.MTA);
                            //MiscThread.Start();

                            OverlayWindow.Imgui.IO.ConfigFlags |= (int)ImGuiConfigFlags.ViewportsEnable;
                            Globals.GOverlay.Opacity = ((double)Math.Round(Globals.cfg.Opacity) / 100);

                            SharpDX.Direct3D9.Device Device3D = Globals.GOverlay.D3DDevice;


                            Renders.MSAA.Add("None");

                            for (int i = 1;  i < 17;)
                            {
                                MultisampleType MultiSample = (MultisampleType)i;
                                bool IsCompatible =   Device3D.Direct3D.CheckDeviceMultisampleType(0, SharpDX.Direct3D9.DeviceType.Hardware, Renders.presentParams.BackBufferFormat, true, MultiSample);
                                
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

                        bool PlaceOnTopGame = false;


                        OverlayWindow.Imgui.Render += delegate {

                            if (GameProc.HasExited)
                                Environment.Exit(0);

                            //Globals.Memory.RemoteNop("user32.dll", "GetWindowDisplayAffinity");
                            if (FreeCMD == false)
                            {
                                FreeCMD = true;
                                SetWindowDisplayAffinity(OverlayWindow.Handle, 0x00000011);
                                FreeConsole();
                            }


                            if (PlaceOnTopGame == false )
                            {
                                //PlaceOnTopGame = true;
                                OverlayWindow.FitTo(GameProc.MainWindowHandle, true);
                                OverlayWindow.PlaceAbove(GameProc.MainWindowHandle);
                                OverlayWindow.Interactive(Globals.cfg.ShowMenu);
                            }

                            if (Globals.cfg.FpsLimit != 0 && (int)ImGui.GetIO().Framerate > Globals.cfg.FpsLimit) {
                                Renders.UpdateFramerate(Globals.cfg.FpsLimit.ToString());
                                Renders.UpdatePriority();
                                Renders.UpdateColors();
                            }

                            Renders.RenderESP();
                            Renders.RenderInfo();
                            Renders.RenderSpectatorList();
                            Features.Misc();

                            if (Utils.Config.IsKeyDown(Keys.Insert)) { Globals.cfg.ShowMenu = !Globals.cfg.ShowMenu; }

                            if (Globals.cfg.ShowMenu == true ) {

                                if (Globals.cfg.BlurOnGUI == true && CurrentTheme == Themer.Themes.None)
                                {
                                    CurrentTheme = Themer.Themes.AeroGlass;
                                    Theme.Apply(CurrentTheme);
                                }

                                Globals.GOverlay.Opacity = (double)1; Renders.RenderMenu();

                                //if (GetParent(OverlayWindow.Handle) == GameProc.MainWindowHandle) {
                                //    OverlayWindow.MakeOverlayChild(OverlayWindow.Handle, IntPtr.Zero);
                                //}

                            }
                            else {

                                if (CurrentTheme != Themer.Themes.None)
                                {
                                    CurrentTheme = Themer.Themes.None;
                                    Theme.Apply(CurrentTheme);
                                }

                                Globals.GOverlay.Opacity = ((double)Math.Round(Globals.cfg.Opacity) / 100);

                                //if (GetParent(OverlayWindow.Handle) == IntPtr.Zero)
                                //{
                                //    OverlayWindow.MakeOverlayChild(OverlayWindow.Handle, GameProc.MainWindowHandle);
                                //}

                            }


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
