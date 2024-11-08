using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExternalSharp.Utils
{

    public class Config
    {
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(Keys vKey);

        public static bool IsKeyDown(Keys VK)
        {
            return (GetAsyncKeyState(VK) & 0x8000) != 0;
        }

        // Status
        public bool Run;
        public bool ShowMenu;
        public bool SpectList;
        public int CurrentBPS;

        // AimBot
        public bool VehAim;
        public bool CheckPlayerIsSpectator;
        public bool AimBot;
        public bool AimAtTeam;
        public bool VisCheck;
        public bool NoSway;
        public int AimTargetBone;
        public bool DrawFov;
        public bool RainbowFov;
        public bool FovFilled;
        public float AimFov;
        public int Smooth;
        public float Aim_MaxDistance;
        public int AimType;
        public int AimKeyType;

        // Visual
        public bool CheckSpectator;
        public bool PlayerESP;
        public bool TeamESP;
        public bool VehicleESP;
        public bool ESP_Box;
        public bool ESP_BoxFilled;
        public bool ESP_Line;
        public bool ESP_Distance;
        public bool ESP_AimAssist;
        public bool ESP_Name;
        public bool ESP_HealthBar;
        public bool ESP_Skeleton;
        public float ESP_MaxDistance;
        public int ESP_BoxType;
        public int ESP_SkeletonColor;

        // System
        public bool StreamProof;
        public bool Crosshair;
        public int CrosshairType;
        public int RainbowRate;

        // Overlay
        public float Opacity;
        public bool Framerate;
        public int FpsLimit;
        public int PriorityType;
        public int MSAA_Level;

        // Misc
        public bool UnlockAll;
        public bool SwayModify;
        public bool BlurOnGUI;
        public float ModVal;
        public bool DamageHack;
        public int ModBPS;

        // Key
        public Keys MenuKey = Keys.Insert;
        public Keys AimKey0 = Keys.RButton;
        public Keys AimKey1 = Keys.LButton;

        // KeyBinder
        public bool KeyBinding;
        public int BindingID;


        public static Config Get()
        {
            Config config = new Config();

            // Set default values
            config.Run = false;
            config.ShowMenu = false;
            config.SpectList = true;
            config.CurrentBPS = 0;

            config.CheckPlayerIsSpectator = false;
            config.VehAim = true;
            config.AimBot = true;
            config.AimAtTeam = false;
            config.VisCheck = false;
            config.NoSway = true;
            config.AimTargetBone = 0;
            config.DrawFov = true;
            config.RainbowFov = true;
            config.FovFilled = true;
            config.AimFov = 50f;
            config.Smooth = 1;
            config.Aim_MaxDistance = 150f;
            config.AimType = 0;
            config.AimKeyType = 1;

            config.CheckSpectator = false;
            config.PlayerESP = true;
            config.TeamESP = false;
            config.VehicleESP = true;
            config.ESP_Box = true;
            config.ESP_BoxFilled = false;
            config.ESP_Line = true;
            config.ESP_Distance = true;
            config.ESP_AimAssist = false;
            config.ESP_Name = true;
            config.ESP_HealthBar = true;
            config.ESP_Skeleton = false;
            config.ESP_MaxDistance = 200f;
            config.ESP_BoxType = 0;
            config.ESP_SkeletonColor = 0;

            config.StreamProof = false;
            config.Crosshair = true;
            config.CrosshairType = 0;
            config.RainbowRate = 25;

            config.Opacity = 57f;
            config.FpsLimit = 30;
            config.PriorityType = 1;
            config.MSAA_Level = 0;

            config.UnlockAll = false;
            config.SwayModify = false;
            config.ModVal = 0.95f;
            config.DamageHack = false;
            config.ModBPS = 1;

            config.KeyBinding = false;
            config.BindingID = 0;

            return config;
        }

    }

    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public struct POINT
    {
        public int X;
        public int Y;
    }

    public static class VK
    {
        public const int INSERT = 0x2D;
        public const int RBUTTON = 0x02;
        public const int LBUTTON = 0x01;
    }
}
