using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using EFT.UI.Settings;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using EFT.Animations.NewRecoil;
using EFT.Animations;
using EFT;
using HarmonyLib;
using static EFT.Player;
using static EFT.ScenesPreset;
using static GClass605;

namespace WTTAN94
{
    public class UpdateWeaponVariablesPatch : ModulePatch
    {
        private static FieldInfo playerField;
        private static FieldInfo fcField;

        protected override MethodBase GetTargetMethod()
        {
            playerField = AccessTools.Field(typeof(FirearmController), "_player");
            fcField = AccessTools.Field(typeof(ProceduralWeaponAnimation), "_firearmController");
            return typeof(EFT.Animations.ProceduralWeaponAnimation).GetMethod("UpdateWeaponVariables", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(EFT.Animations.ProceduralWeaponAnimation __instance)
        {
            FirearmController fc = (FirearmController)fcField.GetValue(__instance);
            if (fc == null) return;
            Player player = (Player)playerField.GetValue(fc);
            if (player != null && player.IsYourPlayer && player.MovementContext.CurrentState.Name != EPlayerState.Stationary)
            {
                Plugin.IsAN94 = Plugin.GunIDs.Contains(fc.Weapon.TemplateId.ToString());         
            }
        }
    }

    public class ShootPatch : ModulePatch
    {
        private static FieldInfo _playerField;
        private static FieldInfo _fcField;

        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(FirearmController), "_player");
            _fcField = AccessTools.Field(typeof(ProceduralWeaponAnimation), "_firearmController");
            return typeof(ProceduralWeaponAnimation).GetMethod("Shoot", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void PatchPrefix(ProceduralWeaponAnimation __instance, ref float str)
        {
            FirearmController fc = (FirearmController)_fcField.GetValue(__instance);
            if (fc == null) return;
            Player player = (Player)_playerField.GetValue(fc);
            if (player != null && player.IsYourPlayer && Plugin.IsAN94 && player.MovementContext.CurrentState.Name != EPlayerState.Stationary)
            {
                Plugin.IsFiring = true;
                Plugin.ShotTimer = 0f;
                Plugin.ShotCount += 1;
                str *= fc.Item.SelectedFireMode != EFT.InventoryLogic.Weapon.EFireMode.single && Plugin.ShotCount <= 2 ? Plugin.BurstRecoilMulti.Value : 1f;
            }
        }
    }

    public class PlayerUpdatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.Public);
        }

        private static void PWAUpdate(Player player, Player.FirearmController fc)
        {
            if (fc != null && Plugin.IsAN94)
            {
                //hyperburst
                fc.Item.MalfState.OverheatFirerateMultInited = true;
                if (Plugin.ShotCount <= 2 && fc.Item.SelectedFireMode != EFT.InventoryLogic.Weapon.EFireMode.single)
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
                    Plugin.ShotCount = 0;
                }
            }

            //Logger.LogWarning($"Is Firing: {Plugin.IsFiring}, Shot Count: {Plugin.ShotCount}, Is AN94:  {Plugin.IsAN94}, Firemode:  {fc.Item.SelectedFireMode}" );
 
        }

        [PatchPostfix]
        private static void PatchPostfix(Player __instance)
        {
            if (__instance.IsYourPlayer)
            {
                Player.FirearmController fc = __instance.HandsController as Player.FirearmController;
                PWAUpdate(__instance, fc);
            }
        }
    }
}
