using System;
using Game;
using Game.Simulation;
using HarmonyLib;

namespace RentControl.Systems
{
    [HarmonyPatch(typeof(GarbageTruckAISystem), "OnCreate")]
    public class GarbageTruckAISystem_OnCreatePatch
    {
        private static bool Prefix(GarbageTruckAISystem __instance)
        {
            __instance.World.GetOrCreateSystemManaged<CustomGarbageTruckAISystem>();
            __instance.World.GetOrCreateSystemManaged<UpdateSystem>().UpdateAt<CustomGarbageTruckAISystem>(SystemUpdatePhase.GameSimulation);
            return true;
        }
    }

    [HarmonyPatch(typeof(GarbageTruckAISystem), "OnCreateForCompiler")]
    public class GarbageTruckAISystem_OnCreateForCompilerPatch
    {
        private static bool Prefix(GarbageTruckAISystem __instance)
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(GarbageTruckAISystem), "OnUpdate")]
    public class GarbageTruckAISystem_OnUpdatePatch
    {
        private static bool Prefix(GarbageTruckAISystem __instance)
        {
            __instance.World.GetOrCreateSystemManaged<CustomGarbageTruckAISystem>().Update();
            return false;
        }
    }
}