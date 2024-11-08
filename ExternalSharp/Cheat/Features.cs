using DearImguiSharp;
using ExternalSharp.Utils;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ExternalSharp.Cheat
{
    public class Features
    {
        public void Misc()
        {
            // while (ExternalSharp.Cheat.Globals.cfg.Run) // Removed for more performance.
            long SyncBFSetting = ExternalSharp.Cheat.Globals.Memory.Read<long>((IntPtr)offset.SyncBFSetting);
            long Weapon = ExternalSharp.Cheat.Globals.Memory.Read<long>((IntPtr)offset.ClientWeapons);

            if (Weapon == 0 || SyncBFSetting == 0)
            {
                Thread.Sleep(10);
                return;
            } else
            {
                long weaponPtr = ExternalSharp.Cheat.Globals.Memory.Read<long>((IntPtr)(Weapon + 0x128));
                long GunSwayData = ExternalSharp.Cheat.Globals.Memory.Read<long>((IntPtr)(weaponPtr + 0x30)); // Sway
                long FiringFunctionData = ExternalSharp.Cheat.Globals.Memory.Read<long>((IntPtr)(weaponPtr + 0x10));
                ExternalSharp.Cheat.Globals.cfg.CurrentBPS = ExternalSharp.Cheat.Globals.Memory.Read<int>((IntPtr)(FiringFunctionData + 0xD8));

                // Recoil / Spread
                if (ExternalSharp.Cheat.Globals.cfg.SwayModify && GunSwayData != 0 && ExternalSharp.Cheat.Globals.Memory.Read<float>((IntPtr)(GunSwayData + 0x430)) != ExternalSharp.Cheat.Globals.cfg.ModVal)
                {
                    ExternalSharp.Cheat.Globals.Memory.Write<float>((IntPtr)(GunSwayData + 0x430), ExternalSharp.Cheat.Globals.cfg.ModVal);
                    ExternalSharp.Cheat.Globals.Memory.Write<float>((IntPtr)(GunSwayData + 0x438), ExternalSharp.Cheat.Globals.cfg.ModVal);
                    ExternalSharp.Cheat.Globals.Memory.Write<float>((IntPtr)(GunSwayData + 0x434), ExternalSharp.Cheat.Globals.cfg.ModVal);
                    ExternalSharp.Cheat.Globals.Memory.Write<float>((IntPtr)(GunSwayData + 0x43C), ExternalSharp.Cheat.Globals.cfg.ModVal);
                }
                else if (!ExternalSharp.Cheat.Globals.cfg.SwayModify && GunSwayData != 0 && ExternalSharp.Cheat.Globals.Memory.Read<float>((IntPtr)(GunSwayData + 0x430)) != 1f)
                {
                    ExternalSharp.Cheat.Globals.Memory.Write<float>((IntPtr)(GunSwayData + 0x430), 1f);
                    ExternalSharp.Cheat.Globals.Memory.Write<float>((IntPtr)(GunSwayData + 0x438), 1f);
                    ExternalSharp.Cheat.Globals.Memory.Write<float>((IntPtr)(GunSwayData + 0x434), 1f);
                    ExternalSharp.Cheat.Globals.Memory.Write<float>((IntPtr)(GunSwayData + 0x43C), 1f);
                }

                // UnlockAll
                if (ExternalSharp.Cheat.Globals.cfg.UnlockAll && SyncBFSetting != 0 && ExternalSharp.Cheat.Globals.Memory.Read<bool>((IntPtr)(SyncBFSetting + 0x54)) == false)
                    ExternalSharp.Cheat.Globals.Memory.Write<bool>((IntPtr)(SyncBFSetting + 0x54), true);
            }

          
        }

    }
}
