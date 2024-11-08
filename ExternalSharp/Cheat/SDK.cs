using ProcessHacker.Native.Api;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ExternalSharp.Cheat
{
    public class SDK
    {
        private Utils.Memory Memory;
        public SDK(Utils.Memory mem)
        {
            Memory = mem;
        }

        // Propiedades de solo lectura para evitar modificaciones externas
        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }
        public SharpDX. Matrix ViewProjection { get; private set; }

        /// <summary>
        /// Actualiza los datos necesarios para la conversión de coordenadas del mundo a pantalla.
        /// </summary>
        /// <returns>True si la actualización fue exitosa; de lo contrario, false.</returns>
        public bool UpdateW2SData()
        {
            // Leer el puntero a GameRenderer
            IntPtr gameRendererPtr = (IntPtr)Memory.Read<long>((IntPtr)offset.GameRenderer);
            if (gameRendererPtr == IntPtr.Zero)
                return false;

            // Leer el puntero a RenderView desde GameRenderer + 0x60
            IntPtr renderViewPtr = IntPtr.Add(gameRendererPtr, 0x60);
            long renderView = Memory.Read<long>(renderViewPtr);
            if (renderView == 0)
                return false;

            // Leer el puntero a DXRenderer
            IntPtr dxRendererPtr = (IntPtr)Memory.Read<ulong>((IntPtr)offset.DxRenderer);
            if (dxRendererPtr == IntPtr.Zero)
                return false;

            // Leer el puntero a m_pScreen desde DXRenderer + 0x38
            IntPtr screenDataPtr = IntPtr.Add(dxRendererPtr, 0x38);
            long m_pScreen = Memory.Read<long>(screenDataPtr);
            if (m_pScreen == 0)
                return false;

            // Leer la matriz ViewProjection desde RenderView + 0x420
            IntPtr viewProjectionPtr = IntPtr.Add((IntPtr)renderView, 0x420);
            ViewProjection = Memory.Read< SharpDX. Matrix>(viewProjectionPtr);

            // Leer las dimensiones de la pantalla desde m_pScreen + 0x58 y m_pScreen + 0x5C
            ScreenWidth = Memory.Read<int>(IntPtr.Add((IntPtr)m_pScreen, 0x58));
            ScreenHeight = Memory.Read<int>(IntPtr.Add((IntPtr)m_pScreen, 0x5C));

            return true;
        }


        public float GetDistance(Vector3 value1, Vector3 value2)
        {
            float num = value1.X - value2.X;
            float num2 = value1.Y - value2.Y;
            float num3 = value1.Z - value2.Z;

            return (float)Math.Sqrt(num * num + num2 * num2 + num3 * num3);
        }

        public bool WorldToScreen(Vector3 worldPosEx, out Vector2 screenPos)
        {
            // Convertir System.Numerics.Vector3 a SharpDX.Vector3
            SharpDX.Vector3 worldPos = new SharpDX.Vector3(worldPosEx.X, worldPosEx.Y, worldPosEx.Z);

            // Leer datos necesarios una vez
            long gameRenderer = Memory.Read<long>((IntPtr)offset.GameRenderer);
            long renderView = Memory.Read<long>((IntPtr)(gameRenderer + 0x60));

            if (renderView == 0)
            {
                screenPos = Vector2.Zero;
                return false;
            }

            ulong dxRenderer = Memory.Read<ulong>((IntPtr)offset.DxRenderer);
            ulong screenData = Memory.Read<ulong>((IntPtr)(dxRenderer + 0x38));

            if (screenData == 0)
            {
                screenPos = Vector2.Zero;
                return false;
            }

            // Leer la matriz de proyección y dimensiones de pantalla
            SharpDX.Matrix viewProjection = Memory.Read<SharpDX.Matrix>((IntPtr)(renderView + 0x420));
            int screenWidth = Memory.Read<int>((IntPtr)(screenData + 0x58));
            int screenHeight = Memory.Read<int>((IntPtr)(screenData + 0x5C));

            // Transformar posición del mundo a coordenadas de recorte
            SharpDX.Vector4 clipCoords = SharpDX.Vector4.Transform(new SharpDX.Vector4(worldPos, 1.0f), viewProjection);

            // Verificar si el objeto está detrás de la cámara
            if (clipCoords.W < 0.65f)
            {
                screenPos = Vector2.Zero;
                return false;
            }

            // Convertir a Coordenadas Normalizadas del Dispositivo (NDC)
            SharpDX.Vector3 ndc;
            ndc.X = clipCoords.X / clipCoords.W;
            ndc.Y = clipCoords.Y / clipCoords.W;
            ndc.Z = clipCoords.Z / clipCoords.W;

            // Convertir de NDC a coordenadas de pantalla en 2D
            screenPos.X = (ndc.X + 1.0f) * 0.5f * screenWidth;
            screenPos.Y = (1.0f - ndc.Y) * 0.5f * screenHeight;

            return true;
        }


        public bool WorldToScreen(Vector3 worldPosEx, out Vector3 screenPos)
        {

            SharpDX.Vector3 worldPos = new SharpDX.Vector3(worldPosEx.X, worldPosEx.Y, worldPosEx.Z);
           
            // Leer datos necesarios una vez
            long gameRenderer = Memory.Read<long>((IntPtr)(offset.GameRenderer));
            long renderView = Memory.Read<long>((IntPtr)(gameRenderer + 0x60));

            if (renderView == 0)
            {
                screenPos = Vector3.Zero;
                return false;
            }

            long dxRenderer = Memory.Read<long>((IntPtr)(offset.DxRenderer));
            long screenData = Memory.Read<long>((IntPtr)(dxRenderer + 0x38));

            if (screenData == 0)
            {
                screenPos = Vector3.Zero;
                return false;
            }

            // Leer matriz y dimensiones de pantalla
            SharpDX.Matrix viewProjection = Memory.Read<SharpDX.Matrix>((IntPtr)(renderView + 0x420));
            int screenWidth = Memory.Read<int>((IntPtr)(screenData + 0x58));
            int screenHeight = Memory.Read<int>((IntPtr)(screenData + 0x5C));

            // Transformar posición del mundo a coordenadas de recorte
            SharpDX.Vector4 clipCoords = SharpDX.Vector4.Transform(new SharpDX.Vector4(worldPos, 1.0f), viewProjection);

            // Verificar si el objeto está detrás de la cámara
            if (clipCoords.W < 0.65f)
            {
                screenPos = new Vector3(0, 0, clipCoords.W);
                return false;
            }

            // Convertir a Coordenadas Normalizadas del Dispositivo (NDC)
            SharpDX.Vector3 ndc;
            ndc.X = clipCoords.X / clipCoords.W;
            ndc.Y = clipCoords.Y / clipCoords.W;
            ndc.Z = clipCoords.Z / clipCoords.W;

            // Convertir de NDC a coordenadas de pantalla
            screenPos.X = (ndc.X + 1.0f) * 0.5f * screenWidth;
            screenPos.Y = (1.0f - ndc.Y) * 0.5f * screenHeight;
            screenPos.Z = clipCoords.W;

            return true;
        }

    }

    public enum VehicleType
    {
        UNUSED = 0x0,
        LOCALPLAYER = 0x1,
        LOCALDIRECTION = 0x2,
        FRIENDLYPLAYER = 0x3,
        ENEMYPLAYER = 0x4,
        NEUTRALPLAYER = 0x5,
        SQUADMEMBER = 0x6,
        SQUADLEADER = 0x7,
        SQUADLEADERTARGETED = 0x8,
        VEHICLE = 0x9,
        PRIMARYOBJECTIVE = 0xA,
        PRIMARYOBJECTIVEBLINK = 0xB,
        SECONDARYOBJECTIVE = 0xC,
        AREAMAPMARKER = 0xD,
        OBJECTIVEDESTROY = 0xE,
        OBJECTIVESCOUT = 0xF,
        OBJECTIVEDEFEND = 0x10,
        OBJECTIVEMOVETO = 0x11,
        OBJECTIVEATTACK = 0x12,
        OBJECTIVEFOLLOW = 0x13,
        OBJECTIVEGENERAL = 0x14,
        UAV = 0x15,
        AMMOCRATE = 0x16,
        MEDICBAG = 0x17,
        C4 = 0x18,
        ATMINE = 0x19,
        STATIONARYWEAPON = 0x1A,
        NORTH = 0x1B,
        SOUTH = 0x1C,
        WEST = 0x1D,
        EAST = 0x1E,
        NEUTRALFLAG = 0x1F,
        FRIENDLYFLAG = 0x20,
        ENEMYFLAG = 0x21,
        FRIENDLYBASE = 0x22,
        ENEMYBASE = 0x23,
        TEAM1FLAG = 0x24,
        TEAM2FLAG = 0x25,
        NEUTRALFLAGLIT = 0x26,
        FRIENDLYFLAGLIT = 0x27,
        ENEMYFLAGLIT = 0x28,
        SELECTABLESPAWNPOINT = 0x29,
        SELECTEDSPAWNPOINT = 0x2A,
        NONSELECTABLESPAWNPOINT = 0x2B,
        FRIENDLYFLAGUNDERATTACK = 0x2C,
        ENEMYFLAGUNDERATTACK = 0x2D,
        ORDERATTACK = 0x2E,
        ORDERDEFEND = 0x2F,
        ORDERATTACKOBSERVED = 0x30,
        ORDERDEFENDOBSERVED = 0x31,
        BOAT = 0x32,
        CAR = 0x33,
        JEEP = 0x34,
        HELIATTACK = 0x35,
        HELISCOUT = 0x36,
        TANK = 0x37,
        TANKIFV = 0x38,
        TANKARTY = 0x39,
        TANKAA = 0x3A,
        TANKAT = 0x3B,
        JET = 0x3C,
        JETBOMBER = 0x3D,
        STATIONARY = 0x3E,
        STRATEGIC = 0x3F,
        MOTIONRADARSWEEP = 0x40,
        NEEDBACKUP = 0x41,
        NEEDAMMO = 0x42,
        NEEDMEDIC = 0x43,
        NEEDPICKUP = 0x44,
        NEEDREPAIR = 0x45,
        KITASSAULT = 0x46,
        KITDEMOLITION = 0x47,
        KITRECON = 0x48,
        KITSPECIALIST = 0x49,
        KITSUPPORT = 0x4A,
        KITMEDIC = 0x4B,
        KITENGINEER = 0x4C,
        KITPICKUPASSAULT = 0x4D,
        KITPICKUPDEMOLITION = 0x4E,
        KITPICKUPRECON = 0x4F,
        KITPICKUPSPECIALIST = 0x50,
        KITPICKUPSUPPORT = 0x51,
        KITPICKUPMEDIC = 0x52,
        KITPICKUPENGINEER = 0x53,
        PICKUP = 0x54,
        TAGGEDVEHICLE = 0x55,
        LASERPAINTEDVEHICLE = 0x56,
        HELITARGETENEMY = 0x57,
        HELITARGETFRIENDLY = 0x58,
        ARTILLERYTARGET = 0x59,
        NEUTRALFLAGATTACKER = 0x5A,
        FRIENDLYFLAGATTACKER = 0x5B,
        ENEMYFLAGATTACKER = 0x5C,
        LASERTARGET = 0x5D,
        OBJECTIVEATTACKER = 0x5E,
        OBJECTIVEDEFENDER = 0x5F,
        HEALTHBARBACKGROUND = 0x60,
        HEALTHBAR = 0x61,
        RADARSWEEPCOMPONENT = 0x62,
        BLANK = 0x63,
        LOCALPLAYERBIGICON = 0x64,
        LOCALPLAYEROUTOFMAP = 0x65,
        PRIMARYOBJECTIVELARGE = 0x66,
        TARGETUNLOCKED = 0x67,
        TARGETLOCKED = 0x68,
        TARGETLOCKING = 0x69,
        ARTILLERYSTRIKENAMETAG = 0x6A,
        ARTILLERYSTRIKEMINIMAP = 0x6B,
        CAPTUREPOINTCONTESTED = 0x6C,
        CAPTUREPOINTDEFENDED = 0x6D,
        ROUNDBAR = 0x6E,
        ROUNDBARBG = 0x6F,
        ROUNDBARBGPLATE = 0x70,
        SNAPOVALARROW = 0x71,
        SQUADLEADERBG = 0x72,
        VEHICLEBG = 0x73,
        NONTAKEABLECONTROLPOINT = 0x74,
        SPOTTEDPOSITION = 0x75,
        GRENADE = 0x76,
        REVIVE = 0x77,
        REPAIR = 0x78,
        INTERACT = 0x79,
        VOIP = 0x7A,
        CLAYMORE = 0x7B,
        EODBOT = 0x7C,
        EXPLOSIVE = 0x7D,
        LASERDESIGNATOR = 0x7E,
        MAV = 0x7F,
        MORTAR = 0x80,
        RADIOBEACON = 0x81,
        UGS = 0x82,
        PERCETAGEBARMIDDLE = 0x83,
        PERCETAGEBAREDGE = 0x84,
        PERCENTAGEBARBACKGROUND = 0x85,
        TANKLC = 0x86,
        HELITRANS = 0x87,
        STATICAT = 0x88,
        STATICAA = 0x89,
        SPRINTBOOST = 0x8A,
        AMMOBOOST = 0x8B,
        EXPLOSIVEBOOST = 0x8C,
        EXPLOSIVERESISTBOOST = 0x8D,
        SUPPRESSIONBOOST = 0x8E,
        SUPPRESSIONRESISTBOOST = 0x8F,
        GRENADEBOOST = 0x90,
        HEALSPEEDBOOST = 0x91,
        NEEDAMMOHIGHLIGHT = 0x92,
        NEEDMEDICHIGHLIGHT = 0x93,
        NEEDREPAIRHIGHLIGHT = 0x94,
        NEEDPICKUPHIGHLIGHT = 0x95,
        PLAYERDEAD = 0x96,
        PLAYER = 0x97,
        FLAG = 0x98,
        BASE = 0x99,
        OBJECTIVENEUTRALBOMB = 0x9A,
        OBJECTIVEFRIENDLYBOMB = 0x9B,
        OBJECTIVEENEMYBOMB = 0x9C,
        OBJECTIVEENEMYHVT = 0x9D,
        OBJECTIVEFRIENDLYHVT = 0x9E,
        CANSUPPLYAMMO = 0x9F,
        CANSUPPLYMEDIC = 0xA0,
        CANSUPPLYREPAIR = 0xA1,
        COUNT = 0xA2
    }

    public static class offset
    {
        public const long ClientgameContext = 0x142670D80;
        public const long SyncBFSetting = 0x1423717C0;
        public const long GameRenderer = 0x142672378;
        public const long DxRenderer = 0x142738080;
        public const long ClientWeapons = 0x1423B2EC8;

        public const long PlayerManager = 0x60;
        public const long LocalPlayer = 0x540;
        public const long ClientPlayer = 0x548;
        public const long ClientSoldier = 0x14D0;
        public const long ClientVehicle = 0x14C0;

        public const long PlayerTeam = 0x13CC;
        public const long PlayerName = 0x40;
        public const long Occlude = 0x5B1;
        public const long Spectator = 0x13C9;
    }


    public struct AxisAlignedBox
    {
        public Vector4 Min;
        public Vector4 Max;
    }

    public struct HealthComponent
    {
        public float m_Health;
        public float m_MaxHealth;
        public float m_VehicleHealth;
        public float m_MaxVehicleHealth;
    }

    public class Player
    {
        public long ClientPlayer;
        public long ClientSoldier;
        public long ClientVehicle;

        public string Name;
        public int Team;
        public HealthComponent TmpComponent;
        public HealthComponent HealthBase = new HealthComponent();
        public Vector3 Position;
        public bool Occlude;
        public int Pose;
        public long pQuat;

        public SharpDX.Matrix VehicleTranfsorm;
        public AxisAlignedBox SoldierAABB;
        public AxisAlignedBox VehicleAABB;

        private Utils.Memory Memory;
        public  Player(Utils.Memory mem)
        {
            Memory = mem;
        } 

        public bool InVehicle()
        {
            return ClientVehicle != 0;
        }

        public VehicleType Vehicle()
        {
            return (VehicleType)Memory.Read<long>((IntPtr)(ClientVehicle + 0xE8));
        }

        public bool IsSpectator()
        {
            return Memory.Read<bool>((IntPtr)(ClientPlayer + 0x12C9));
        }

        public bool IsVisible()
        {
            return !Occlude;
        }

        public bool IsDead()
        {
            return HealthBase.m_Health <= 0f && Position == Vector3.Zero;
        }

        public Vector3 GetVelocity()
        {
            long TmpPosition = Memory.Read<long>((IntPtr)(ClientSoldier + 0x490));
            return Memory.Read<Vector3>((IntPtr)(TmpPosition + 0x50));
        }


        private string ReadPlayerName()
        {
            string pName = string.Empty;
            Memory.ReadString((IntPtr)(ClientPlayer + offset.PlayerName), out pName, 10);
            return pName;
        }

        private long GetQuat()
        {
            return Memory.Read<long>((IntPtr)(Memory.Read<long>((IntPtr)(ClientSoldier + 0x580)) + 0xB0));
        }

        public void Update()
        {
            ClientSoldier = Memory.Read<long>((IntPtr)(ClientPlayer + offset.ClientSoldier));
            ClientVehicle = Memory.Read<long>((IntPtr)(ClientPlayer + offset.ClientVehicle));

            // Team
            Team = Memory.Read<int>((IntPtr)(ClientPlayer + offset.PlayerTeam));
            // Health
            long TmpHealthComponent = Memory.Read<long>((IntPtr)(ClientSoldier + 0x140));
           // TmpComponent = Memory.Read<HealthComponent>((IntPtr)(TmpHealthComponent));

            HealthBase.m_Health = Memory.Read<float>((IntPtr)(TmpHealthComponent + 0x0020));
            HealthBase.m_MaxHealth = Memory.Read<float>((IntPtr)(TmpHealthComponent + 0x0024));
            HealthBase.m_VehicleHealth = Memory.Read<float>((IntPtr)(TmpHealthComponent + 0x0038));
            HealthBase.m_MaxVehicleHealth = Memory.Read<float>((IntPtr)(TmpHealthComponent + 0x0042));

            // Position
            if (InVehicle()) // Vehicle
            {
                long DynamicPhysicsEntity = Memory.Read<long>((IntPtr)(ClientVehicle + 0x238));

                if (DynamicPhysicsEntity != 0)
                {
                    long pPhysicsEntity = Memory.Read<long>((IntPtr)(DynamicPhysicsEntity + 0xA0));
                    VehicleTranfsorm = Memory.Read<SharpDX.Matrix>((IntPtr)(pPhysicsEntity));
                    VehicleAABB = Memory.Read<AxisAlignedBox>((IntPtr)(ClientVehicle + 0x250));

                    Position = new Vector3(VehicleTranfsorm.M41, VehicleTranfsorm.M42, VehicleTranfsorm.M43);
                }
            }
            else // Soldier
            {
                long TmpPosition = Memory.Read<long>((IntPtr)(ClientSoldier + 0x490));
                Position = Memory.Read<Vector3>((IntPtr)(TmpPosition + 0x30));
            }

            // Visible
            Occlude = Memory.Read<bool>((IntPtr)(ClientSoldier + 0x05B1));
            Pose = Memory.Read<int>((IntPtr)(ClientSoldier + 0x4F0));
            pQuat = GetQuat();
            Name = ReadPlayerName();
        }

        public AxisAlignedBox GetAABB()
        {
            AxisAlignedBox aabb = new AxisAlignedBox();

            switch (Pose)
            {
                case 0:
                    aabb.Min = new Vector4(-0.350000f, 0.000000f, -0.350000f, 0);
                    aabb.Max = new Vector4(0.350000f, 1.700000f, 0.350000f, 0);
                    break;
                case 1:
                    aabb.Min = new Vector4(-0.350000f, 0.000000f, -0.350000f, 0);
                    aabb.Max = new Vector4(0.350000f, 1.150000f, 0.350000f, 0);
                    break;
                case 2:
                    aabb.Min = new Vector4(-0.350000f, 0.000000f, -0.350000f, 0);
                    aabb.Max = new Vector4(0.350000f, 0.400000f, 0.350000f, 0);
                    break;
                default:
                    break;
            }

            return aabb;
        }

        public Vector3 GetBone(int bone_id)
        {
            return Memory.Read<Vector3>((IntPtr)(pQuat + bone_id * 0x20));
        }

    }


}
