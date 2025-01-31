using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using SPT.Common.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using static GClass1907;
using static MineDirectional;

namespace WTTAN94
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static string[] GunIDs =  { "678fe4a4906c7bd23722c71f", "679a6a534f3d279c99b135b9" };

        public static ConfigEntry<float> BurstROFMulti { get; set; }
        public static ConfigEntry<float> BurstRecoilMulti { get; set; }
        public static ConfigEntry<float> ShotResetDelay { get; set; }
        public static ConfigEntry<float> ShotThreshold { get; set; }

        public static bool IsAN94 { get; set; } = false;
        public static bool IsFiring { get; set; } = false;
        public static int RecoilShotCount { get; set; } = 0;
        public static int ROFShotCount { get; set; } = 0;
        public static float ShotTimer { get; set; } = 0f;

        public Player You { get; set; }

        private void Awake()
        {
            string settings = "Settings";
            BurstROFMulti = Config.Bind<float>(settings, "Burst ROF Multi", 3f, new ConfigDescription("", new AcceptableValueRange<float>(1f, 4f), new ConfigurationManagerAttributes { Order = 1 }));
            BurstRecoilMulti = Config.Bind<float>(settings, "Burst Rrecoil Multi", 0.5f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 1f), new ConfigurationManagerAttributes { Order = 10}));
            ShotResetDelay = Config.Bind<float>(settings, "Shot Reset Delay", 0.05f, new ConfigDescription("Time Delay After Firing To Determine If Firing Has Stopped.", new AcceptableValueRange<float>(0.01f, 2f), new ConfigurationManagerAttributes { Order = 15 }));
            ShotThreshold = Config.Bind<float>(settings, "Hyperburst Shot Threshold", 1f, new ConfigDescription("At What Shot Count Does Hyperburst End. It's To Do With Code Execution Timing, Leave It Alone Unless There Are Issues.", new AcceptableValueRange<float>(0f, 5f), new ConfigurationManagerAttributes { Order = 15 }));

            new UpdateWeaponVariablesPatch().Enable();
            new ShootPatch().Enable();
        }

        private void PWAUpdate(Player player, Player.FirearmController fc)
        {
            if (fc != null && Plugin.IsAN94)
            {
                //hyperburst
                fc.Item.MalfState.OverheatFirerateMultInited = true;
                if (Plugin.ROFShotCount <= Plugin.ShotThreshold.Value && fc.Item.SelectedFireMode != EFT.InventoryLogic.Weapon.EFireMode.single)
                {
                    fc.Item.MalfState.OverheatFirerateMult = Plugin.BurstROFMulti.Value;
                }
                else fc.Item.MalfState.OverheatFirerateMult = 1f;
            }

            if (Plugin.IsFiring)
            {
                Plugin.ShotTimer += Time.deltaTime;

                if (!fc.autoFireOn && Plugin.ShotTimer >= Plugin.ShotResetDelay.Value)
                {
                    Plugin.ShotTimer = 0f;
                    Plugin.IsFiring = false;
                    Plugin.RecoilShotCount = 0;
                    Plugin.ROFShotCount = 0;
                }
            }
            //Logger.LogWarning($"Is Firing: {Plugin.IsFiring}, Shot Count: {Plugin.ShotCount}, Is AN94:  {Plugin.IsAN94}, Firemode:  {fc.Item.SelectedFireMode}" );
        }

        void Update() 
        {
            if (You == null)
            {
                GameWorld gameWorld = Singleton<GameWorld>.Instance;
                if (gameWorld?.MainPlayer != null && gameWorld.MainPlayer.IsYourPlayer) You = gameWorld.MainPlayer;
            }
            else
            {
                Player.FirearmController fc = You.HandsController as Player.FirearmController;
                PWAUpdate(You, fc);
            }
        }

    }
}
