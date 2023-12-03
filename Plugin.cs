using System.IO.Compression;
using System.IO;
using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using BepInEx.Configuration;

#if BEPINEX_V6
    using BepInEx.Unity.Mono;
#endif

namespace RentControl
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, "1.0.6")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            Plugin.enable_LandValueCap = base.Config.Bind<bool>("LandValueCapEnabled", "LandValueCapEnabled", true, "是否启用地价上限 | Enable land value upper bound, true or false");
            Plugin.m_LandValueCap = base.Config.Bind<int>("LandValueCap", "LandValueCap", 120000, "地价上限，需填整数(>0) | Land value upper bound, INTEGER needed(>0)");
            Plugin.RentPaymentFactor = base.Config.Bind<float>("RentPaymentFactor", "RentPaymentFactor", (float)0.25, "市民和企业实际支付租金的倍率(0-100) | Proportion% of RENT actually payed by residents and companies(0-100)");
            Plugin.RentUpgradeFactor = base.Config.Bind<float>("RentUpgradeFactor", "RentUpgradeFactor", (float)1.0, "租金加到建筑升级进度上的倍率(0-100) | Proportion% of RENT actually added to the building level-up progress(0-100)");
            Plugin.BuildingUpkeepFactor = base.Config.Bind<float>("BuildingUpkeepFactor", "BuildingUpkeepFactor", (float)0.5, "建筑升级进度扣除建筑维护费的倍率(0-100) | Proportion% of BUILDING UPKEEP actually subtracted from the building level-up progress(0-100)");
            Plugin.GarbageFeeFactor = base.Config.Bind<float>("GarbageFeeFactor", "GarbageFeeFactor", (float)0.5, "建筑升级进度扣除垃圾清运费的倍率(0-100) | Proportion% of GARBAGE FEE actually subtracted from the building level-up progress(0-100)");


            var harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), MyPluginInfo.PLUGIN_GUID + "_Cities2Harmony");
            var patchedMethods = harmony.GetPatchedMethods().ToArray();

            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} made patches! Patched methods: " + patchedMethods.Length);

            foreach (var patchedMethod in patchedMethods) {
                Logger.LogInfo($"Patched method: {patchedMethod.Module.Name}:{patchedMethod.Name}");
            }
        }
        public static ConfigEntry<bool> enable_LandValueCap;
        public static ConfigEntry<int> m_LandValueCap;
        public static ConfigEntry<float> RentPaymentFactor;
        public static ConfigEntry<float> RentUpgradeFactor;
        public static ConfigEntry<float> BuildingUpkeepFactor;
        public static ConfigEntry<float> GarbageFeeFactor;
    }
}
