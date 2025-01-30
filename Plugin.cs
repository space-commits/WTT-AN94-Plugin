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

        public static bool IsAN94 { get; set; } = false;
        public static bool IsFiring { get; set; } = false;
        public static int ShotCount { get; set; } = 0;
        public static float ShotTimer { get; set; } = 0f;

        private void Awake()
        {
            string settings = "Settings";
            BurstROFMulti = Config.Bind<float>(settings, "Burst ROF Multi", 3f, new ConfigDescription("", new AcceptableValueRange<float>(1f, 4f), new ConfigurationManagerAttributes { Order = 1 }));
            BurstRecoilMulti = Config.Bind<float>(settings, "Burst Rrecoil Multi", 0.5f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 1f), new ConfigurationManagerAttributes { Order = 10}));
            ShotResetDelay = Config.Bind<float>(settings, "Shot Reset Delay", 0.05f, new ConfigDescription("Time Delay After Firing To Determine If Firing Has Stopped.", new AcceptableValueRange<float>(0.01f, 2f), new ConfigurationManagerAttributes { Order = 15 }));

            new UpdateWeaponVariablesPatch().Enable();
            new ShootPatch().Enable();
            new PlayerUpdatePatch().Enable();
        }

    }
}
