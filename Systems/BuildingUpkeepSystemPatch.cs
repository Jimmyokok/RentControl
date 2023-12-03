using System;
using Game;
using Game.Simulation;
using HarmonyLib;

namespace RentControl.Systems
{
    [HarmonyPatch(typeof(BuildingUpkeepSystem), "OnCreate")]
    public class BuildingUpkeepSystem_OnCreatePatch
    {
        private static bool Prefix(BuildingUpkeepSystem __instance)
        {
            __instance.World.GetOrCreateSystemManaged<BuildingUpkeepSystem_Custom>();
            __instance.World.GetOrCreateSystemManaged<UpdateSystem>().UpdateAt<BuildingUpkeepSystem_Custom>(SystemUpdatePhase.GameSimulation);
            return true;
        }
    }

    [HarmonyPatch(typeof(BuildingUpkeepSystem), "OnCreateForCompiler")]
    public class BuildingUpkeepSystem_OnCreateForCompilerPatch
    {
        private static bool Prefix(BuildingUpkeepSystem __instance)
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(BuildingUpkeepSystem), "OnUpdate")]
    public class BuildingUpkeepSystem_OnUpdatePatch
    {
        private static bool Prefix(BuildingUpkeepSystem __instance)
        {
            __instance.World.GetOrCreateSystemManaged<BuildingUpkeepSystem_Custom>().Update();
            return false;
        }
    }
}