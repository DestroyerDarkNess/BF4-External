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

        public static bool AimBot(Vector2 TargetPosition)
        {
            switch (ExternalSharp.Cheat.Globals.cfg.AimKeyType)
            {
                case 0: // and
                    if (Config.IsKeyDown(ExternalSharp.Cheat.Globals.cfg.AimKey0))
                        break;
                    else if (!Config.IsKeyDown(ExternalSharp.Cheat.Globals.cfg.AimKey0) || !Config.IsKeyDown(ExternalSharp.Cheat.Globals.cfg.AimKey1))
                        return false;
                    else if (!Config.IsKeyDown(ExternalSharp.Cheat.Globals.cfg.AimKey0))
                        return false;
                    break;
                case 1: // or
                    if (Config.IsKeyDown(ExternalSharp.Cheat.Globals.cfg.AimKey0))
                        break;
                    else if (Config.IsKeyDown(ExternalSharp.Cheat.Globals.cfg.AimKey0) || Config.IsKeyDown(ExternalSharp.Cheat.Globals.cfg.AimKey1))
                        break;
                    break;
            }

            Vector2 ScreenMiddle = new Vector2((float)ExternalSharp.Cheat.Globals.GOverlay.Right / 2f, (float)ExternalSharp.Cheat.Globals.GOverlay.Bottom / 2f);

            if (TargetPosition != new Vector2(0f, 0f))
            {
                int cX = (int)((ScreenMiddle.X - TargetPosition.X) / ExternalSharp.Cheat.Globals.cfg.Smooth);
                int cY = (int)((ScreenMiddle.Y - TargetPosition.Y) / ExternalSharp.Cheat.Globals.cfg.Smooth);

                Utils.WinAPI.MoveMouse( -cX, -cY);
            }

            return true;
        }

        public void AimBotOld()
        {
            while (ExternalSharp.Cheat.Globals.cfg.Run)
            {

                /*----| SomeChecks |--------------------------------------------------------------------------------*/
                if (!ExternalSharp.Cheat.Globals.cfg.AimBot)
                {
                    Thread.Sleep(100);
                    continue;
                }
                else if (!Config.IsKeyDown(ExternalSharp.Cheat.Globals.cfg.AimKey0) && !Config.IsKeyDown(ExternalSharp.Cheat.Globals.cfg.AimKey1))
                {
                    Thread.Sleep(10);
                    continue;
                }

                bool check = false;

                switch (ExternalSharp.Cheat.Globals.cfg.AimKeyType)
                {
                    case 0: // and
                        if (ExternalSharp.Cheat.Globals.cfg.AimKey1 != 0)
                            if (!Config.IsKeyDown(ExternalSharp.Cheat.Globals.cfg.AimKey0) || !Config.IsKeyDown(ExternalSharp.Cheat.Globals.cfg.AimKey1))
                                continue;
                            else
                                if (!Config.IsKeyDown(ExternalSharp.Cheat.Globals.cfg.AimKey0))
                                continue;

                        check = true;
                        break;
                    case 1: // or
                        if (ExternalSharp.Cheat.Globals.cfg.AimKey1 != 0)
                            if (Config.IsKeyDown(ExternalSharp.Cheat.Globals.cfg.AimKey0) || Config.IsKeyDown(ExternalSharp.Cheat.Globals.cfg.AimKey1))
                                check = true;
                            else
                                if (!Config.IsKeyDown(ExternalSharp.Cheat.Globals.cfg.AimKey0))
                                continue;

                        check = true;
                        break;
                    default:
                        break;
                }

                if (!check)
                    continue;
                /*--------------------------------------------------------------------------------------------------*/
                // Overlay Fails checks
                uint CurrentPid = Utils.WinAPI.ForegroundProcessID();
                if (CurrentPid != ExternalSharp.Cheat.Globals.Memory.PID)
                {
                    if (CurrentPid != ExternalSharp.Cheat.Globals.Memory.OverlayPID)
                    {
                        continue;
                    }
                }


                int aimBone = Utils.WinAPI.AimBoneHead;
                switch (ExternalSharp.Cheat.Globals.cfg.AimTargetBone)
                {
                    case 0:
                        aimBone = Utils.WinAPI.AimBoneHead;
                        break;
                    case 1:
                        aimBone = Utils.WinAPI.AimBoneChest;
                        break;
                    default:
                        break;
                }

                float fov = 0f;
                float minFov = 0f;
                float minDistance = 0f;
                Vector2 screenMiddle = new Vector2(ExternalSharp.Cheat.Globals.GOverlay.Right / 2f, ExternalSharp.Cheat.Globals.GOverlay.Bottom / 2f);

                // TargetPosition
                Vector2 targetPosition = new Vector2();
                Player player = new Player(ExternalSharp.Cheat.Globals.Memory);
                Player local = new Player(ExternalSharp.Cheat.Globals.Memory);

                // Context
                long clientGameContext = ExternalSharp.Cheat.Globals.Memory.Read<long>((IntPtr)offset.ClientgameContext);
                long playerManager = ExternalSharp.Cheat.Globals.Memory.Read<long>((IntPtr)(clientGameContext + offset.PlayerManager));
                long playerEntity = ExternalSharp.Cheat.Globals.Memory.Read<long>((IntPtr)(playerManager + offset.ClientPlayer));

                // LocalPlayer
                local.ClientPlayer = ExternalSharp.Cheat.Globals.Memory.Read<long>((IntPtr)(playerManager + offset.LocalPlayer));
                local.Update();

                // ESP Loop
                for (int i = 0; i < 64; i++)
                {
                    // LocalPlayer Check
                    if (local.IsDead())
                        break;

                    // Update Player
                    player.ClientPlayer = ExternalSharp.Cheat.Globals.Memory.Read<long>((IntPtr)(playerEntity + (i * 0x08)));
                    player.Update();


                    // Spectaror Warning
                    //if (ExternalSharp.Cheat.Globals.cfg.CheckPlayerIsSpectator && player.IsSpectator())
                    //    continue;

                    // Invalid Player
                    if (player.ClientPlayer == 0)
                        continue;
                    else if (player.ClientPlayer == local.ClientPlayer)
                        continue;
                    else if (player.IsDead())
                        continue;
                    else if (!ExternalSharp.Cheat.Globals.cfg.AimAtTeam && player.Team == local.Team)
                        continue;
                    else if (ExternalSharp.Cheat.Globals.cfg.VisCheck == true && !player.IsVisible())
                        continue;
                    else if (ExternalSharp.Cheat.Globals.cfg.VehAim == false && player.InVehicle())
                        continue;

                    if (ExternalSharp.Cheat.Globals.cfg.ShowMenu == true)
                        continue;

                    if (ExternalSharp.Cheat.Globals.cfg.VehAim && player.InVehicle())
                    {

                        float distance = ExternalSharp.Cheat.Globals.GameSDK.GetDistance(local.Position, player.Position);


                        Vector2 VehicleScreen = new Vector2();
                        if (!ExternalSharp.Cheat.Globals.GameSDK.WorldToScreen(player.Position, out VehicleScreen))
                            continue;

                        if (VehicleScreen != new Vector2(0f, 0f))
                        {
                            Vector4 pEntityMax = player.VehicleAABB.Max;
                            Vector4 pEntityMin = player.VehicleAABB.Min;
                            Vector3 Top = player.Position + new Vector3(pEntityMax.X, pEntityMax.Y, pEntityMax.Z);
                            Vector3 Btm = player.Position + new Vector3(pEntityMin.X, pEntityMin.Y, pEntityMin.Z);

                            Vector2 BoxTop, BoxBtm;
                            if (!ExternalSharp.Cheat.Globals.GameSDK.WorldToScreen(Top, out BoxTop) || !ExternalSharp.Cheat.Globals.GameSDK.WorldToScreen(Btm, out BoxBtm))
                                continue;
                            else if (BoxTop == new Vector2(0f, 0f) || BoxBtm == new Vector2(0f, 0f))
                                continue;

                            float BoxMiddle = VehicleScreen.X;
                            float Height = BoxBtm.Y - BoxTop.Y;
                            float Width = Height / 4f;

                         //   DrawBox((int)(VehicleScreen.X - Width), (int)VehicleScreen.Y, (int)(Height), (int)(-Height), VehColor);


                            // Fov check
                            fov = Math.Abs((screenMiddle - VehicleScreen).Length());

                            if (fov < ExternalSharp.Cheat.Globals.cfg.AimFov)
                            {
                                if (minFov == 0f || minFov > fov)
                                {
                                    minFov = fov;
                                    targetPosition = new Vector2((VehicleScreen.X - Width), (int)(VehicleScreen.Y -( Height /2)));
                                }
                            }
                        }

                        continue;
                    }
                    else 
                    {
                        // GetDistance
                        float distance = ExternalSharp.Cheat.Globals.GameSDK.GetDistance(local.Position, player.Position);

                        // CheckDistance
                        if (ExternalSharp.Cheat.Globals.cfg.Aim_MaxDistance < distance)
                            continue;

                        // GetBone Position
                        Vector2 screenPosition = new Vector2(0f, 0f);

                        // ToDo : Prediction

                        if (!ExternalSharp.Cheat.Globals.GameSDK.WorldToScreen(player.GetBone(aimBone), out screenPosition))
                            continue;

                        // Fov check
                        fov = Math.Abs((screenMiddle - screenPosition).Length());

                        if (fov < ExternalSharp.Cheat.Globals.cfg.AimFov)
                        {
                            switch (ExternalSharp.Cheat.Globals.cfg.AimType)
                            {
                                case 0:
                                    if (minFov == 0f || minFov > fov)
                                    {
                                        minFov = fov;
                                        targetPosition = screenPosition;
                                    }
                                    break;
                                case 1:
                                    if (minDistance == 0f || minDistance > distance)
                                    {
                                        minDistance = distance;
                                        targetPosition = screenPosition;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                   
                }

                // AimBotŽÀs?
                if (targetPosition != new Vector2(0f, 0f))
                {

                    int deltaX = (int)((screenMiddle.X - targetPosition.X) / ExternalSharp.Cheat.Globals.cfg.Smooth);
                    int deltaY = (int)((screenMiddle.Y - targetPosition.Y) / ExternalSharp.Cheat.Globals.cfg.Smooth);

                    Utils.WinAPI.MoveMouse(-deltaX, -deltaY);
                } 
            }
        }

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
