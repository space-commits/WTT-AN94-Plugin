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
                Plugin.RecoilShotCount += 1;
                str *= fc.Item.SelectedFireMode != EFT.InventoryLogic.Weapon.EFireMode.single && Plugin.RecoilShotCount <= 2 ? Plugin.BurstRecoilMulti.Value : 1f;
            }
        }


        [PatchPostfix]
        private static void PatchPostFix(ProceduralWeaponAnimation __instance, ref float str)
        {
            FirearmController fc = (FirearmController)_fcField.GetValue(__instance);
            if (fc == null) return;
            Player player = (Player)_playerField.GetValue(fc);
            if (player != null && player.IsYourPlayer && Plugin.IsAN94 && player.MovementContext.CurrentState.Name != EPlayerState.Stationary)
            {
                Plugin.IsFiring = true;
                Plugin.ShotTimer = 0f;
                Plugin.ROFShotCount += 1;
            }
        }
    }
}
