using System;
using System;
using Game;
using Game.Simulation;
using HarmonyLib;

namespace RentControl.Systems
{
    [HarmonyPatch(typeof(RentInitializeSystem), "OnCreate")]
    public class RentInitializeSystem_OnCreatePatch
    {
        private static bool Prefix(RentInitializeSystem __instance)
        {
            __instance.World.GetOrCreateSystemManaged<CustomRentInitializeSystem>();
            __instance.World.GetOrCreateSystemManaged<UpdateSystem>().UpdateAt<CustomRentInitializeSystem>(SystemUpdatePhase.GameSimulation);
            return true;
        }
    }

    [HarmonyPatch(typeof(RentInitializeSystem), "OnCreateForCompiler")]
    public class RentInitializeSystem_OnCreateForCompilerPatch
    {
        private static bool Prefix(RentInitializeSystem __instance)
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(RentInitializeSystem), "OnUpdate")]
    public class RentInitializeSystem_OnUpdatePatch
    {
        private static bool Prefix(RentInitializeSystem __instance)
        {
            __instance.World.GetOrCreateSystemManaged<CustomRentInitializeSystem>().Update();
            return false;
        }
    }
}