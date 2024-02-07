using Game;
using Game.Simulation;
using System.Runtime.CompilerServices;
using Colossal.Mathematics;
using Game.Buildings;
using Game.City;
using Game.Common;
using Game.Economy;
using Game.Net;
using Game.Notifications;
using Game.Objects;
using Game.Pathfind;
using Game.Prefabs;
using Game.Tools;
using Game.Vehicles;
using Game.Zones;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Scripting;

namespace RentControl.Systems
{
    // Token: 0x020015B3 RID: 5555
    [CompilerGenerated]
    public class CustomGarbageTruckAISystem : GameSystemBase
    {
        // Token: 0x06005E86 RID: 24198 RVA: 0x00088ED7 File Offset: 0x000870D7
        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return 16;
        }

        // Token: 0x06005E87 RID: 24199 RVA: 0x0005F777 File Offset: 0x0005D977
        public override int GetUpdateOffset(SystemUpdatePhase phase)
        {
            return 2;
        }

        // Token: 0x06005E88 RID: 24200 RVA: 0x00415E50 File Offset: 0x00414050
        [Preserve]
        protected override void OnCreate()
        {
            base.OnCreate();
            GarbageFeeFactor = Plugin.GarbageFeeFactor.Value;
            GarbageFeeFactor = GarbageFeeFactor > 0 ? GarbageFeeFactor : 0;
            GarbageFeeFactor = GarbageFeeFactor < 100 ? GarbageFeeFactor : 100;
            this.m_EndFrameBarrier = base.World.GetOrCreateSystemManaged<EndFrameBarrier>();
			this.m_PathfindSetupSystem = base.World.GetOrCreateSystemManaged<PathfindSetupSystem>();
			this.m_IconCommandSystem = base.World.GetOrCreateSystemManaged<IconCommandSystem>();
			this.m_ServiceFeeSystem = base.World.GetOrCreateSystemManaged<ServiceFeeSystem>();
			this.m_SimulationSystem = base.World.GetOrCreateSystemManaged<SimulationSystem>();
			this.m_CityStatisticsSystem = base.World.GetOrCreateSystemManaged<CityStatisticsSystem>();
            this.m_VehicleQuery = base.GetEntityQuery(new ComponentType[]
            {
                ComponentType.ReadWrite<CarCurrentLane>(),
                ComponentType.ReadOnly<Owner>(),
                ComponentType.ReadOnly<PrefabRef>(),
                ComponentType.ReadWrite<PathOwner>(),
                ComponentType.ReadWrite<Game.Vehicles.GarbageTruck>(),
                ComponentType.ReadWrite<Target>(),
                ComponentType.Exclude<Deleted>(),
                ComponentType.Exclude<Temp>(),
                ComponentType.Exclude<TripSource>(),
                ComponentType.Exclude<OutOfControl>()
            });
            this.m_GarbageCollectionRequestArchetype = base.EntityManager.CreateArchetype(new ComponentType[]
            {
                ComponentType.ReadWrite<ServiceRequest>(),
                ComponentType.ReadWrite<GarbageCollectionRequest>(),
                ComponentType.ReadWrite<RequestGroup>()
            });
            this.m_HandleRequestArchetype = base.EntityManager.CreateArchetype(new ComponentType[]
            {
                ComponentType.ReadWrite<HandleRequest>(),
                ComponentType.ReadWrite<Game.Common.Event>()
            });
            base.RequireForUpdate(this.m_VehicleQuery);
            base.RequireForUpdate<GarbageParameterData>();
            base.RequireForUpdate<ServiceFeeParameterData>();
            base.RequireForUpdate<BuildingEfficiencyParameterData>();
        }

        // Token: 0x06005E89 RID: 24201 RVA: 0x00415FD4 File Offset: 0x004141D4
        [Preserve]
        protected override void OnUpdate()
        {
            GarbageParameterData singleton = this.__query_647374862_0.GetSingleton<GarbageParameterData>();
            NativeQueue<CustomGarbageTruckAISystem.GarbageAction> actionQueue = new NativeQueue<CustomGarbageTruckAISystem.GarbageAction>(Allocator.TempJob);
            this.__TypeHandle.__Game_Pathfind_PathElement_RW_BufferLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Net_SubLane_RO_BufferLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Objects_SubObject_RO_BufferLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_ConnectedBuilding_RO_BufferLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_GarbageFacility_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_GarbageProducer_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Simulation_GarbageCollectionRequest_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_ZoneData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_GarbageTruckData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_CarData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Net_SlaveLane_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Net_Curve_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Net_PedestrianLane_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Net_EdgeLane_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Objects_Quantity_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Pathfind_PathInformation_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Common_Owner_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Simulation_ServiceDispatch_RW_BufferTypeHandle.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Vehicles_CarNavigationLane_RW_BufferTypeHandle.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Pathfind_PathOwner_RW_ComponentTypeHandle.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Common_Target_RW_ComponentTypeHandle.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Vehicles_CarCurrentLane_RW_ComponentTypeHandle.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Vehicles_Car_RW_ComponentTypeHandle.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Vehicles_GarbageTruck_RW_ComponentTypeHandle.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Pathfind_PathInformation_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Objects_Unspawned_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Common_Owner_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref base.CheckedStateRef);
            CustomGarbageTruckAISystem.GarbageTruckTickJob garbageTruckTickJob = default(CustomGarbageTruckAISystem.GarbageTruckTickJob);
            garbageTruckTickJob.m_EntityType = this.__TypeHandle.__Unity_Entities_Entity_TypeHandle;
            garbageTruckTickJob.m_OwnerType = this.__TypeHandle.__Game_Common_Owner_RO_ComponentTypeHandle;
            garbageTruckTickJob.m_UnspawnedType = this.__TypeHandle.__Game_Objects_Unspawned_RO_ComponentTypeHandle;
            garbageTruckTickJob.m_PrefabRefType = this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;
            garbageTruckTickJob.m_PathInformationType = this.__TypeHandle.__Game_Pathfind_PathInformation_RO_ComponentTypeHandle;
            garbageTruckTickJob.m_GarbageTruckType = this.__TypeHandle.__Game_Vehicles_GarbageTruck_RW_ComponentTypeHandle;
            garbageTruckTickJob.m_CarType = this.__TypeHandle.__Game_Vehicles_Car_RW_ComponentTypeHandle;
            garbageTruckTickJob.m_CurrentLaneType = this.__TypeHandle.__Game_Vehicles_CarCurrentLane_RW_ComponentTypeHandle;
            garbageTruckTickJob.m_TargetType = this.__TypeHandle.__Game_Common_Target_RW_ComponentTypeHandle;
            garbageTruckTickJob.m_PathOwnerType = this.__TypeHandle.__Game_Pathfind_PathOwner_RW_ComponentTypeHandle;
            garbageTruckTickJob.m_CarNavigationLaneType = this.__TypeHandle.__Game_Vehicles_CarNavigationLane_RW_BufferTypeHandle;
            garbageTruckTickJob.m_ServiceDispatchType = this.__TypeHandle.__Game_Simulation_ServiceDispatch_RW_BufferTypeHandle;
            garbageTruckTickJob.m_OwnerData = this.__TypeHandle.__Game_Common_Owner_RO_ComponentLookup;
            garbageTruckTickJob.m_PathInformationData = this.__TypeHandle.__Game_Pathfind_PathInformation_RO_ComponentLookup;
            garbageTruckTickJob.m_QuantityData = this.__TypeHandle.__Game_Objects_Quantity_RO_ComponentLookup;
            garbageTruckTickJob.m_EdgeLaneData = this.__TypeHandle.__Game_Net_EdgeLane_RO_ComponentLookup;
            garbageTruckTickJob.m_PedestrianLaneData = this.__TypeHandle.__Game_Net_PedestrianLane_RO_ComponentLookup;
            garbageTruckTickJob.m_CurveData = this.__TypeHandle.__Game_Net_Curve_RO_ComponentLookup;
            garbageTruckTickJob.m_SlaveLaneData = this.__TypeHandle.__Game_Net_SlaveLane_RO_ComponentLookup;
            garbageTruckTickJob.m_PrefabCarData = this.__TypeHandle.__Game_Prefabs_CarData_RO_ComponentLookup;
            garbageTruckTickJob.m_PrefabGarbageTruckData = this.__TypeHandle.__Game_Prefabs_GarbageTruckData_RO_ComponentLookup;
            garbageTruckTickJob.m_PrefabRefData = this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup;
            garbageTruckTickJob.m_PrefabSpawnableBuildingData = this.__TypeHandle.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup;
            garbageTruckTickJob.m_PrefabZoneData = this.__TypeHandle.__Game_Prefabs_ZoneData_RO_ComponentLookup;
            garbageTruckTickJob.m_GarbageCollectionRequestData = this.__TypeHandle.__Game_Simulation_GarbageCollectionRequest_RO_ComponentLookup;
            garbageTruckTickJob.m_GarbageProducerData = this.__TypeHandle.__Game_Buildings_GarbageProducer_RO_ComponentLookup;
            garbageTruckTickJob.m_GarbageFacilityData = this.__TypeHandle.__Game_Buildings_GarbageFacility_RO_ComponentLookup;
            garbageTruckTickJob.m_ConnectedBuildings = this.__TypeHandle.__Game_Buildings_ConnectedBuilding_RO_BufferLookup;
            garbageTruckTickJob.m_SubObjects = this.__TypeHandle.__Game_Objects_SubObject_RO_BufferLookup;
            garbageTruckTickJob.m_SubLanes = this.__TypeHandle.__Game_Net_SubLane_RO_BufferLookup;
            garbageTruckTickJob.m_PathElements = this.__TypeHandle.__Game_Pathfind_PathElement_RW_BufferLookup;
            garbageTruckTickJob.m_SimulationFrameIndex = this.m_SimulationSystem.frameIndex;
            garbageTruckTickJob.m_GarbageCollectionRequestArchetype = this.m_GarbageCollectionRequestArchetype;
            garbageTruckTickJob.m_HandleRequestArchetype = this.m_HandleRequestArchetype;
            garbageTruckTickJob.m_GarbageParameters = singleton;
            garbageTruckTickJob.m_CommandBuffer = this.m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter();
            garbageTruckTickJob.m_PathfindQueue = this.m_PathfindSetupSystem.GetQueue(this, 64).AsParallelWriter();
            garbageTruckTickJob.m_ActionQueue = actionQueue.AsParallelWriter();
            CustomGarbageTruckAISystem.GarbageTruckTickJob jobData = garbageTruckTickJob;
            this.__TypeHandle.__Game_Objects_OutsideConnection_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Common_Owner_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_Efficiency_RW_BufferLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_BuildingCondition_RW_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Economy_Resources_RW_BufferLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_GarbageProducer_RW_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Vehicles_GarbageTruck_RW_ComponentLookup.Update(ref base.CheckedStateRef);
            CustomGarbageTruckAISystem.GarbageActionJob garbageActionJob = default(CustomGarbageTruckAISystem.GarbageActionJob);
            garbageActionJob.m_GarbageTruckData = this.__TypeHandle.__Game_Vehicles_GarbageTruck_RW_ComponentLookup;
            garbageActionJob.m_GarbageProducerData = this.__TypeHandle.__Game_Buildings_GarbageProducer_RW_ComponentLookup;
            garbageActionJob.m_EconomyResources = this.__TypeHandle.__Game_Economy_Resources_RW_BufferLookup;
            garbageActionJob.m_BuildingConditions = this.__TypeHandle.__Game_Buildings_BuildingCondition_RW_ComponentLookup;
            garbageActionJob.m_Efficiencies = this.__TypeHandle.__Game_Buildings_Efficiency_RW_BufferLookup;
            garbageActionJob.m_Owners = this.__TypeHandle.__Game_Common_Owner_RO_ComponentLookup;
            garbageActionJob.m_OutsideConnections = this.__TypeHandle.__Game_Objects_OutsideConnection_RO_ComponentLookup;
            garbageActionJob.m_GarbageParameters = singleton;
            garbageActionJob.m_GarbageFee = this.__query_647374862_1.GetSingleton<ServiceFeeParameterData>().m_GarbageFee.m_Default;
            garbageActionJob.m_GarbageEfficiencyPenalty = this.__query_647374862_2.GetSingleton<BuildingEfficiencyParameterData>().m_GarbagePenalty;
            garbageActionJob.m_ActionQueue = actionQueue;
            JobHandle job;
            garbageActionJob.m_FeeQueue = this.m_ServiceFeeSystem.GetFeeQueue(out job);
            JobHandle job2;
            garbageActionJob.m_StatisticsEventQueue = this.m_CityStatisticsSystem.GetStatisticsEventQueue(out job2).AsParallelWriter();
            garbageActionJob.m_IconCommandBuffer = this.m_IconCommandSystem.CreateCommandBuffer();
            garbageActionJob.m_CommandBuffer = this.m_EndFrameBarrier.CreateCommandBuffer();
            CustomGarbageTruckAISystem.GarbageActionJob jobData2 = garbageActionJob;
            JobHandle jobHandle = jobData.ScheduleParallel(this.m_VehicleQuery, base.Dependency);
            JobHandle jobHandle2 = jobData2.Schedule(JobHandle.CombineDependencies(job, jobHandle));
            actionQueue.Dispose(jobHandle2);
            this.m_PathfindSetupSystem.AddQueueWriter(jobHandle);
            this.m_EndFrameBarrier.AddJobHandleForProducer(jobHandle2);
            this.m_IconCommandSystem.AddCommandBufferWriter(jobHandle2);
            this.m_ServiceFeeSystem.AddQueueWriter(jobHandle2);
            this.m_CityStatisticsSystem.AddWriter(jobHandle2);
            base.Dependency = jobHandle2;
        }

        // Token: 0x06005E8A RID: 24202 RVA: 0x00416760 File Offset: 0x00414960
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void __AssignQueries(ref SystemState state)
        {
            this.__query_647374862_0 = state.GetEntityQuery(new EntityQueryDesc[]
            {
                new EntityQueryDesc
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<GarbageParameterData>()
                    },
                    Any = new ComponentType[0],
                    None = new ComponentType[0],
                    Disabled = new ComponentType[0],
                    Absent = new ComponentType[0],
                    Options = EntityQueryOptions.IncludeSystems
                }
            });
            this.__query_647374862_1 = state.GetEntityQuery(new EntityQueryDesc[]
            {
                new EntityQueryDesc
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<ServiceFeeParameterData>()
                    },
                    Any = new ComponentType[0],
                    None = new ComponentType[0],
                    Disabled = new ComponentType[0],
                    Absent = new ComponentType[0],
                    Options = EntityQueryOptions.IncludeSystems
                }
            });
            this.__query_647374862_2 = state.GetEntityQuery(new EntityQueryDesc[]
            {
                new EntityQueryDesc
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<BuildingEfficiencyParameterData>()
                    },
                    Any = new ComponentType[0],
                    None = new ComponentType[0],
                    Disabled = new ComponentType[0],
                    Absent = new ComponentType[0],
                    Options = EntityQueryOptions.IncludeSystems
                }
            });
        }

        // Token: 0x06005E8B RID: 24203 RVA: 0x000929A5 File Offset: 0x00090BA5
        protected override void OnCreateForCompiler()
        {
            base.OnCreateForCompiler();
            this.__AssignQueries(ref base.CheckedStateRef);
            this.__TypeHandle.__AssignHandles(ref base.CheckedStateRef);
        }

        // Token: 0x06005E8C RID: 24204 RVA: 0x0005E948 File Offset: 0x0005CB48
        [Preserve]
        public CustomGarbageTruckAISystem()
        {
        }

        // Token: 0x0400A7CC RID: 42956
        private EndFrameBarrier m_EndFrameBarrier;

        // Token: 0x0400A7CD RID: 42957
        private PathfindSetupSystem m_PathfindSetupSystem;

        // Token: 0x0400A7CE RID: 42958
        private IconCommandSystem m_IconCommandSystem;

        // Token: 0x0400A7CF RID: 42959
        private ServiceFeeSystem m_ServiceFeeSystem;

        // Token: 0x0400A7D0 RID: 42960
        private SimulationSystem m_SimulationSystem;

        private CityStatisticsSystem m_CityStatisticsSystem;

        // Token: 0x0400A7D1 RID: 42961
        private EntityQuery m_VehicleQuery;

        // Token: 0x0400A7D2 RID: 42962
        private EntityArchetype m_GarbageCollectionRequestArchetype;

        // Token: 0x0400A7D3 RID: 42963
        private EntityArchetype m_HandleRequestArchetype;

        // Token: 0x0400A7D4 RID: 42964
        private CustomGarbageTruckAISystem.TypeHandle __TypeHandle;

        // Token: 0x0400A7D5 RID: 42965
        private EntityQuery __query_647374862_0;

        // Token: 0x0400A7D6 RID: 42966
        private EntityQuery __query_647374862_1;

        // Token: 0x0400A7D7 RID: 42967
        private EntityQuery __query_647374862_2;

        private static float GarbageFeeFactor;

        // Token: 0x020015B4 RID: 5556
        private enum GarbageActionType
        {
            // Token: 0x0400A7D9 RID: 42969
            Collect,
            // Token: 0x0400A7DA RID: 42970
            Unload,
            // Token: 0x0400A7DB RID: 42971
            AddRequest
        }

        // Token: 0x020015B5 RID: 5557
        private struct GarbageAction
        {
            // Token: 0x0400A7DC RID: 42972
            public Entity m_Vehicle;

            // Token: 0x0400A7DD RID: 42973
            public Entity m_Target;

            // Token: 0x0400A7DE RID: 42974
            public Entity m_Request;

            // Token: 0x0400A7DF RID: 42975
            public CustomGarbageTruckAISystem.GarbageActionType m_Type;

            // Token: 0x0400A7E0 RID: 42976
            public int m_Capacity;

            // Token: 0x0400A7E1 RID: 42977
            public int m_MaxAmount;
        }

        // Token: 0x020015B6 RID: 5558
        [BurstCompile]
        private struct GarbageTruckTickJob : IJobChunk
        {
            // Token: 0x06005F9F RID: 24479 RVA: 0x003B5DBC File Offset: 0x003B3FBC
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> nativeArray = chunk.GetNativeArray(this.m_EntityType);
                NativeArray<Owner> nativeArray2 = chunk.GetNativeArray<Owner>(ref this.m_OwnerType);
                NativeArray<PrefabRef> nativeArray3 = chunk.GetNativeArray<PrefabRef>(ref this.m_PrefabRefType);
                NativeArray<PathInformation> nativeArray4 = chunk.GetNativeArray<PathInformation>(ref this.m_PathInformationType);
                NativeArray<CarCurrentLane> nativeArray5 = chunk.GetNativeArray<CarCurrentLane>(ref this.m_CurrentLaneType);
                NativeArray<Game.Vehicles.GarbageTruck> nativeArray6 = chunk.GetNativeArray<Game.Vehicles.GarbageTruck>(ref this.m_GarbageTruckType);
                NativeArray<Car> nativeArray7 = chunk.GetNativeArray<Car>(ref this.m_CarType);
                NativeArray<Target> nativeArray8 = chunk.GetNativeArray<Target>(ref this.m_TargetType);
                NativeArray<PathOwner> nativeArray9 = chunk.GetNativeArray<PathOwner>(ref this.m_PathOwnerType);
                BufferAccessor<CarNavigationLane> bufferAccessor = chunk.GetBufferAccessor<CarNavigationLane>(ref this.m_CarNavigationLaneType);
                BufferAccessor<ServiceDispatch> bufferAccessor2 = chunk.GetBufferAccessor<ServiceDispatch>(ref this.m_ServiceDispatchType);
                bool isUnspawned = chunk.Has<Unspawned>(ref this.m_UnspawnedType);
                for (int i = 0; i < nativeArray.Length; i++)
                {
                    Entity entity = nativeArray[i];
                    Owner owner = nativeArray2[i];
                    PrefabRef prefabRef = nativeArray3[i];
                    PathInformation pathInformation = nativeArray4[i];
                    Game.Vehicles.GarbageTruck value = nativeArray6[i];
                    Car value2 = nativeArray7[i];
                    CarCurrentLane carCurrentLane = nativeArray5[i];
                    PathOwner value3 = nativeArray9[i];
                    Target value4 = nativeArray8[i];
                    DynamicBuffer<CarNavigationLane> navigationLanes = bufferAccessor[i];
                    DynamicBuffer<ServiceDispatch> serviceDispatches = bufferAccessor2[i];
                    VehicleUtils.CheckUnspawned(unfilteredChunkIndex, entity, carCurrentLane, isUnspawned, this.m_CommandBuffer);
                    this.Tick(unfilteredChunkIndex, entity, owner, prefabRef, pathInformation, navigationLanes, serviceDispatches, ref value, ref value2, ref carCurrentLane, ref value3, ref value4);
                    nativeArray6[i] = value;
                    nativeArray7[i] = value2;
                    nativeArray5[i] = carCurrentLane;
                    nativeArray9[i] = value3;
                    nativeArray8[i] = value4;
                }
            }

            // Token: 0x06005FA0 RID: 24480 RVA: 0x003B5F68 File Offset: 0x003B4168
            private void Tick(int jobIndex, Entity vehicleEntity, Owner owner, PrefabRef prefabRef, PathInformation pathInformation, DynamicBuffer<CarNavigationLane> navigationLanes, DynamicBuffer<ServiceDispatch> serviceDispatches, ref Game.Vehicles.GarbageTruck garbageTruck, ref Car car, ref CarCurrentLane currentLane, ref PathOwner pathOwner, ref Target target)
            {
                if (VehicleUtils.ResetUpdatedPath(ref pathOwner))
                {
                    this.ResetPath(jobIndex, vehicleEntity, pathInformation, serviceDispatches, ref garbageTruck, ref car, ref currentLane);
                }
                GarbageTruckData garbageTruckData = this.m_PrefabGarbageTruckData[prefabRef.m_Prefab];
                if (!this.m_PrefabRefData.HasComponent(target.m_Target) || VehicleUtils.PathfindFailed(pathOwner))
                {
                    if (VehicleUtils.IsStuck(pathOwner) || (garbageTruck.m_State & GarbageTruckFlags.Returning) != (GarbageTruckFlags)0U)
                    {
                        if (this.UnloadGarbage(jobIndex, vehicleEntity, garbageTruckData, owner.m_Owner, ref garbageTruck, true))
                        {
                            this.m_CommandBuffer.AddComponent<Deleted>(jobIndex, vehicleEntity, default(Deleted));
                        }
                        return;
                    }
                    this.ReturnToDepot(owner, serviceDispatches, ref garbageTruck, ref car, ref pathOwner, ref target);
                }
                else if (VehicleUtils.PathEndReached(currentLane))
                {
                    if ((garbageTruck.m_State & GarbageTruckFlags.Returning) != (GarbageTruckFlags)0U)
                    {
                        if (this.UnloadGarbage(jobIndex, vehicleEntity, garbageTruckData, owner.m_Owner, ref garbageTruck, false))
                        {
                            this.m_CommandBuffer.AddComponent<Deleted>(jobIndex, vehicleEntity, default(Deleted));
                        }
                        return;
                    }
                    this.TryCollectGarbage(jobIndex, vehicleEntity, garbageTruckData, ref garbageTruck, ref car, ref currentLane, target.m_Target);
                    this.TryCollectGarbage(jobIndex, vehicleEntity, garbageTruckData, ref garbageTruck, ref car, ref target);
                    this.CheckServiceDispatches(vehicleEntity, serviceDispatches, ref garbageTruck, ref pathOwner);
                    if (!this.SelectNextDispatch(jobIndex, vehicleEntity, navigationLanes, serviceDispatches, ref garbageTruck, ref car, ref currentLane, ref pathOwner, ref target))
                    {
                        this.ReturnToDepot(owner, serviceDispatches, ref garbageTruck, ref car, ref pathOwner, ref target);
                    }
                }
                else if (VehicleUtils.WaypointReached(currentLane))
                {
                    currentLane.m_LaneFlags &= ~Game.Vehicles.CarLaneFlags.Waypoint;
                    this.TryCollectGarbage(jobIndex, vehicleEntity, garbageTruckData, ref garbageTruck, ref car, ref currentLane, Entity.Null);
                }
                if ((garbageTruck.m_State & GarbageTruckFlags.Returning) == (GarbageTruckFlags)0U)
                {
                    if (garbageTruck.m_Garbage >= garbageTruckData.m_GarbageCapacity || (garbageTruck.m_State & GarbageTruckFlags.Disabled) != (GarbageTruckFlags)0U)
                    {
                        this.ReturnToDepot(owner, serviceDispatches, ref garbageTruck, ref car, ref pathOwner, ref target);
                    }
                    else
                    {
                        this.CheckGarbagePresence(ref currentLane, ref garbageTruck, ref car, navigationLanes);
                    }
                }
                if (garbageTruck.m_Garbage + garbageTruck.m_EstimatedGarbage >= garbageTruckData.m_GarbageCapacity)
                {
                    garbageTruck.m_State |= GarbageTruckFlags.EstimatedFull;
                }
                else
                {
                    garbageTruck.m_State &= ~GarbageTruckFlags.EstimatedFull;
                }
                if (garbageTruck.m_Garbage < garbageTruckData.m_GarbageCapacity && (garbageTruck.m_State & GarbageTruckFlags.Disabled) == (GarbageTruckFlags)0U)
                {
                    this.CheckServiceDispatches(vehicleEntity, serviceDispatches, ref garbageTruck, ref pathOwner);
                    if ((garbageTruck.m_State & GarbageTruckFlags.Returning) != (GarbageTruckFlags)0U)
                    {
                        this.SelectNextDispatch(jobIndex, vehicleEntity, navigationLanes, serviceDispatches, ref garbageTruck, ref car, ref currentLane, ref pathOwner, ref target);
                    }
                    if (garbageTruck.m_RequestCount <= 1 && (garbageTruck.m_State & GarbageTruckFlags.EstimatedFull) == (GarbageTruckFlags)0U)
                    {
                        this.RequestTargetIfNeeded(jobIndex, vehicleEntity, ref garbageTruck);
                    }
                }
                else
                {
                    serviceDispatches.Clear();
                }
                this.FindPathIfNeeded(vehicleEntity, prefabRef, ref currentLane, ref pathOwner, ref target);
            }

            // Token: 0x06005FA1 RID: 24481 RVA: 0x003B61F0 File Offset: 0x003B43F0
            private void FindPathIfNeeded(Entity vehicleEntity, PrefabRef prefabRefData, ref CarCurrentLane currentLaneData, ref PathOwner pathOwnerData, ref Target target)
            {
                if (VehicleUtils.RequireNewPath(pathOwnerData))
                {
                    CarData carData = this.m_PrefabCarData[prefabRefData.m_Prefab];
                    PathfindParameters parameters = new PathfindParameters
                    {
                        m_MaxSpeed = carData.m_MaxSpeed,
                        m_WalkSpeed = 5.555556f,
                        m_Weights = new PathfindWeights(1f, 1f, 1f, 1f),
                        m_Methods = PathMethod.Road,
                        m_IgnoredFlags = (EdgeFlags.ForbidCombustionEngines | EdgeFlags.ForbidTransitTraffic | EdgeFlags.ForbidHeavyTraffic | EdgeFlags.ForbidPrivateTraffic | EdgeFlags.ForbidSlowTraffic)
                    };
                    SetupQueueTarget origin = new SetupQueueTarget
                    {
                        m_Type = SetupTargetType.CurrentLocation,
                        m_Methods = PathMethod.Road,
                        m_RoadTypes = RoadTypes.Car
                    };
                    SetupQueueTarget destination = new SetupQueueTarget
                    {
                        m_Type = SetupTargetType.CurrentLocation,
                        m_Methods = PathMethod.Road,
                        m_RoadTypes = RoadTypes.Car,
                        m_Entity = target.m_Target
                    };
                    SetupQueueItem item = new SetupQueueItem(vehicleEntity, parameters, origin, destination);
                    VehicleUtils.SetupPathfind(ref currentLaneData, ref pathOwnerData, this.m_PathfindQueue, item);
                }
            }

            // Token: 0x06005FA2 RID: 24482 RVA: 0x003B62F4 File Offset: 0x003B44F4
            private void CheckServiceDispatches(Entity vehicleEntity, DynamicBuffer<ServiceDispatch> serviceDispatches, ref Game.Vehicles.GarbageTruck garbageTruck, ref PathOwner pathOwner)
            {
                if (serviceDispatches.Length > garbageTruck.m_RequestCount)
                {
                    int num = -1;
                    Entity entity = Entity.Null;
                    PathElement pathElement = default(PathElement);
                    bool flag = false;
                    int num2 = 0;
                    if (garbageTruck.m_RequestCount >= 1 && (garbageTruck.m_State & GarbageTruckFlags.Returning) == (GarbageTruckFlags)0U)
                    {
                        DynamicBuffer<PathElement> dynamicBuffer = this.m_PathElements[vehicleEntity];
                        num2 = 1;
                        if (pathOwner.m_ElementIndex < dynamicBuffer.Length)
                        {
                            pathElement = dynamicBuffer[dynamicBuffer.Length - 1];
                            flag = true;
                        }
                    }
                    for (int i = num2; i < garbageTruck.m_RequestCount; i++)
                    {
                        Entity request = serviceDispatches[i].m_Request;
                        DynamicBuffer<PathElement> dynamicBuffer2;
                        if (this.m_PathElements.TryGetBuffer(request, out dynamicBuffer2) && dynamicBuffer2.Length != 0)
                        {
                            pathElement = dynamicBuffer2[dynamicBuffer2.Length - 1];
                            flag = true;
                        }
                    }
                    for (int j = garbageTruck.m_RequestCount; j < serviceDispatches.Length; j++)
                    {
                        Entity request2 = serviceDispatches[j].m_Request;
                        if (this.m_GarbageCollectionRequestData.HasComponent(request2))
                        {
                            GarbageCollectionRequest garbageCollectionRequest = this.m_GarbageCollectionRequestData[request2];
                            DynamicBuffer<PathElement> dynamicBuffer3;
                            if (flag && this.m_PathElements.TryGetBuffer(request2, out dynamicBuffer3) && dynamicBuffer3.Length != 0)
                            {
                                PathElement pathElement2 = dynamicBuffer3[0];
                                if (pathElement2.m_Target != pathElement.m_Target || pathElement2.m_TargetDelta.x != pathElement.m_TargetDelta.y)
                                {
                                    goto IL_17C;
                                }
                            }
                            if (this.m_PrefabRefData.HasComponent(garbageCollectionRequest.m_Target) && garbageCollectionRequest.m_Priority > num)
                            {
                                num = garbageCollectionRequest.m_Priority;
                                entity = request2;
                            }
                        }
                    IL_17C:;
                    }
                    if (entity != Entity.Null)
                    {
                        int requestCount = garbageTruck.m_RequestCount;
                        garbageTruck.m_RequestCount = requestCount + 1;
                        serviceDispatches[requestCount] = new ServiceDispatch(entity);
                        this.PreAddCollectionRequests(entity, ref garbageTruck);
                    }
                    if (serviceDispatches.Length > garbageTruck.m_RequestCount)
                    {
                        serviceDispatches.RemoveRange(garbageTruck.m_RequestCount, serviceDispatches.Length - garbageTruck.m_RequestCount);
                    }
                }
            }

            // Token: 0x06005FA3 RID: 24483 RVA: 0x003B64F0 File Offset: 0x003B46F0
            private void PreAddCollectionRequests(Entity request, ref Game.Vehicles.GarbageTruck garbageTruck)
            {
                if (this.m_PathElements.HasBuffer(request))
                {
                    DynamicBuffer<PathElement> dynamicBuffer = this.m_PathElements[request];
                    Entity rhs = Entity.Null;
                    for (int i = 0; i < dynamicBuffer.Length; i++)
                    {
                        PathElement pathElement = dynamicBuffer[i];
                        if (!this.m_EdgeLaneData.HasComponent(pathElement.m_Target))
                        {
                            rhs = Entity.Null;
                        }
                        else
                        {
                            Owner owner = this.m_OwnerData[pathElement.m_Target];
                            if (!(owner.m_Owner == rhs))
                            {
                                rhs = owner.m_Owner;
                                if (this.HasSidewalk(owner.m_Owner))
                                {
                                    garbageTruck.m_EstimatedGarbage += this.AddCollectionRequests(owner.m_Owner, request, ref garbageTruck);
                                }
                            }
                        }
                    }
                }
            }

            // Token: 0x06005FA4 RID: 24484 RVA: 0x003B65AC File Offset: 0x003B47AC
            private bool HasSidewalk(Entity owner)
            {
                if (this.m_SubLanes.HasBuffer(owner))
                {
                    DynamicBuffer<Game.Net.SubLane> dynamicBuffer = this.m_SubLanes[owner];
                    for (int i = 0; i < dynamicBuffer.Length; i++)
                    {
                        Entity subLane = dynamicBuffer[i].m_SubLane;
                        if (this.m_PedestrianLaneData.HasComponent(subLane))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            // Token: 0x06005FA5 RID: 24485 RVA: 0x003B6608 File Offset: 0x003B4808
            private void RequestTargetIfNeeded(int jobIndex, Entity entity, ref Game.Vehicles.GarbageTruck garbageTruck)
            {
                if (this.m_GarbageCollectionRequestData.HasComponent(garbageTruck.m_TargetRequest))
                {
                    return;
                }
                uint num = math.max(512U, 16U);
                if ((this.m_SimulationFrameIndex & num - 1U) == 2U)
                {
                    Entity e = this.m_CommandBuffer.CreateEntity(jobIndex, this.m_GarbageCollectionRequestArchetype);
                    this.m_CommandBuffer.SetComponent<ServiceRequest>(jobIndex, e, new ServiceRequest(true));
                    this.m_CommandBuffer.SetComponent<GarbageCollectionRequest>(jobIndex, e, new GarbageCollectionRequest(entity, 1, ((garbageTruck.m_State & GarbageTruckFlags.IndustrialWasteOnly) != (GarbageTruckFlags)0U) ? GarbageCollectionRequestFlags.IndustrialWaste : ((GarbageCollectionRequestFlags)0)));
                    this.m_CommandBuffer.SetComponent<RequestGroup>(jobIndex, e, new RequestGroup(32U));
                }
            }

            // Token: 0x06005FA6 RID: 24486 RVA: 0x003B66A0 File Offset: 0x003B48A0
            private bool SelectNextDispatch(int jobIndex, Entity vehicleEntity, DynamicBuffer<CarNavigationLane> navigationLanes, DynamicBuffer<ServiceDispatch> serviceDispatches, ref Game.Vehicles.GarbageTruck garbageTruck, ref Car car, ref CarCurrentLane currentLane, ref PathOwner pathOwner, ref Target target)
            {
                if ((garbageTruck.m_State & GarbageTruckFlags.Returning) == (GarbageTruckFlags)0U && garbageTruck.m_RequestCount > 0 && serviceDispatches.Length > 0)
                {
                    serviceDispatches.RemoveAt(0);
                    garbageTruck.m_RequestCount--;
                }
                while (garbageTruck.m_RequestCount > 0 && serviceDispatches.Length > 0)
                {
                    Entity request = serviceDispatches[0].m_Request;
                    Entity entity = Entity.Null;
                    if (this.m_GarbageCollectionRequestData.HasComponent(request))
                    {
                        entity = this.m_GarbageCollectionRequestData[request].m_Target;
                    }
                    if (this.m_PrefabRefData.HasComponent(entity))
                    {
                        garbageTruck.m_State &= ~GarbageTruckFlags.Returning;
                        car.m_Flags |= CarFlags.UsePublicTransportLanes;
                        Entity e = this.m_CommandBuffer.CreateEntity(jobIndex, this.m_HandleRequestArchetype);
                        this.m_CommandBuffer.SetComponent<HandleRequest>(jobIndex, e, new HandleRequest(request, vehicleEntity, false, true));
                        if (this.m_GarbageCollectionRequestData.HasComponent(garbageTruck.m_TargetRequest))
                        {
                            e = this.m_CommandBuffer.CreateEntity(jobIndex, this.m_HandleRequestArchetype);
                            this.m_CommandBuffer.SetComponent<HandleRequest>(jobIndex, e, new HandleRequest(garbageTruck.m_TargetRequest, Entity.Null, true, false));
                        }
                        if (this.m_PathElements.HasBuffer(request))
                        {
                            DynamicBuffer<PathElement> appendPath = this.m_PathElements[request];
                            if (appendPath.Length != 0)
                            {
                                DynamicBuffer<PathElement> dynamicBuffer = this.m_PathElements[vehicleEntity];
                                PathUtils.TrimPath(dynamicBuffer, ref pathOwner);
                                float num = garbageTruck.m_PathElementTime * (float)dynamicBuffer.Length + this.m_PathInformationData[request].m_Duration;
                                int num2;
                                if (PathUtils.TryAppendPath(ref currentLane, navigationLanes, dynamicBuffer, appendPath, this.m_SlaveLaneData, this.m_OwnerData, this.m_SubLanes, out num2))
                                {
                                    int num3 = dynamicBuffer.Length - num2;
                                    int num4 = 0;
                                    for (int i = 0; i < num3; i++)
                                    {
                                        PathElement pathElement = dynamicBuffer[i];
                                        if (this.m_PedestrianLaneData.HasComponent(pathElement.m_Target))
                                        {
                                            Owner owner = this.m_OwnerData[pathElement.m_Target];
                                            num4 += this.AddCollectionRequests(owner.m_Owner, request, ref garbageTruck);
                                        }
                                    }
                                    if (num2 > 0)
                                    {
                                        NativeArray<PathElement> nativeArray = new NativeArray<PathElement>(num2, Allocator.Temp, NativeArrayOptions.ClearMemory);
                                        for (int j = 0; j < num2; j++)
                                        {
                                            nativeArray[j] = dynamicBuffer[num3 + j];
                                        }
                                        dynamicBuffer.RemoveRange(num3, num2);
                                        Entity @null = Entity.Null;
                                        for (int k = 0; k < nativeArray.Length; k++)
                                        {
                                            num4 += this.AddPathElement(dynamicBuffer, nativeArray[k], request, ref @null, ref garbageTruck);
                                        }
                                        nativeArray.Dispose();
                                    }
                                    if (garbageTruck.m_RequestCount == 1)
                                    {
                                        garbageTruck.m_EstimatedGarbage = num4;
                                    }
                                    car.m_Flags |= CarFlags.StayOnRoad;
                                    garbageTruck.m_PathElementTime = num / (float)math.max(1, dynamicBuffer.Length);
                                    target.m_Target = entity;
                                    VehicleUtils.ClearEndOfPath(ref currentLane, navigationLanes);
                                    return true;
                                }
                            }
                        }
                        VehicleUtils.SetTarget(ref pathOwner, ref target, entity);
                        return true;
                    }
                    serviceDispatches.RemoveAt(0);
                    garbageTruck.m_EstimatedGarbage -= garbageTruck.m_EstimatedGarbage / garbageTruck.m_RequestCount;
                    garbageTruck.m_RequestCount--;
                }
                return false;
            }

            // Token: 0x06005FA7 RID: 24487 RVA: 0x003B69CC File Offset: 0x003B4BCC
            private void ReturnToDepot(Owner ownerData, DynamicBuffer<ServiceDispatch> serviceDispatches, ref Game.Vehicles.GarbageTruck garbageTruck, ref Car car, ref PathOwner pathOwnerData, ref Target targetData)
            {
                serviceDispatches.Clear();
                garbageTruck.m_RequestCount = 0;
                garbageTruck.m_EstimatedGarbage = 0;
                garbageTruck.m_State |= GarbageTruckFlags.Returning;
                car.m_Flags &= ~(CarFlags.Warning | CarFlags.Working);
                VehicleUtils.SetTarget(ref pathOwnerData, ref targetData, ownerData.m_Owner);
            }

            // Token: 0x06005FA8 RID: 24488 RVA: 0x003B6A18 File Offset: 0x003B4C18
            private void ResetPath(int jobIndex, Entity vehicleEntity, PathInformation pathInformation, DynamicBuffer<ServiceDispatch> serviceDispatches, ref Game.Vehicles.GarbageTruck garbageTruck, ref Car carData, ref CarCurrentLane currentLane)
            {
                DynamicBuffer<PathElement> path = this.m_PathElements[vehicleEntity];
                PathUtils.ResetPath(ref currentLane, path, this.m_SlaveLaneData, this.m_OwnerData, this.m_SubLanes);
                if ((garbageTruck.m_State & GarbageTruckFlags.Returning) == (GarbageTruckFlags)0U && garbageTruck.m_RequestCount > 0 && serviceDispatches.Length > 0)
                {
                    Entity request = serviceDispatches[0].m_Request;
                    if (this.m_GarbageCollectionRequestData.HasComponent(request))
                    {
                        NativeArray<PathElement> nativeArray = new NativeArray<PathElement>(path.Length, Allocator.Temp, NativeArrayOptions.ClearMemory);
                        nativeArray.CopyFrom(path.AsNativeArray());
                        path.Clear();
                        Entity @null = Entity.Null;
                        int estimatedGarbage = 0;
                        for (int i = 0; i < nativeArray.Length; i++)
                        {
                            estimatedGarbage = this.AddPathElement(path, nativeArray[i], request, ref @null, ref garbageTruck);
                        }
                        if (garbageTruck.m_RequestCount == 1)
                        {
                            garbageTruck.m_EstimatedGarbage = estimatedGarbage;
                        }
                        nativeArray.Dispose();
                    }
                    carData.m_Flags |= CarFlags.StayOnRoad;
                }
                else
                {
                    carData.m_Flags &= ~CarFlags.StayOnRoad;
                }
                carData.m_Flags |= CarFlags.UsePublicTransportLanes;
                garbageTruck.m_PathElementTime = pathInformation.m_Duration / (float)math.max(1, path.Length);
            }

            // Token: 0x06005FA9 RID: 24489 RVA: 0x003B6B4C File Offset: 0x003B4D4C
            private int AddPathElement(DynamicBuffer<PathElement> path, PathElement pathElement, Entity request, ref Entity lastOwner, ref Game.Vehicles.GarbageTruck garbageTruck)
            {
                int result = 0;
                if (!this.m_EdgeLaneData.HasComponent(pathElement.m_Target))
                {
                    path.Add(pathElement);
                    lastOwner = Entity.Null;
                    return result;
                }
                Owner owner = this.m_OwnerData[pathElement.m_Target];
                if (owner.m_Owner == lastOwner)
                {
                    path.Add(pathElement);
                    return result;
                }
                lastOwner = owner.m_Owner;
                float y = pathElement.m_TargetDelta.y;
                Entity target;
                if (this.FindClosestSidewalk(pathElement.m_Target, owner.m_Owner, ref y, out target))
                {
                    result = this.AddCollectionRequests(owner.m_Owner, request, ref garbageTruck);
                    path.Add(pathElement);
                    path.Add(new PathElement(target, y, (PathElementFlags)0));
                }
                else
                {
                    path.Add(pathElement);
                }
                return result;
            }

            // Token: 0x06005FAA RID: 24490 RVA: 0x003B6C24 File Offset: 0x003B4E24
            private bool FindClosestSidewalk(Entity lane, Entity owner, ref float curvePos, out Entity sidewalk)
            {
                bool result = false;
                sidewalk = Entity.Null;
                if (this.m_SubLanes.HasBuffer(owner))
                {
                    float3 position = MathUtils.Position(this.m_CurveData[lane].m_Bezier, curvePos);
                    DynamicBuffer<Game.Net.SubLane> dynamicBuffer = this.m_SubLanes[owner];
                    float num = float.MaxValue;
                    for (int i = 0; i < dynamicBuffer.Length; i++)
                    {
                        Entity subLane = dynamicBuffer[i].m_SubLane;
                        if (this.m_PedestrianLaneData.HasComponent(subLane))
                        {
                            float num3;
                            float num2 = MathUtils.Distance(MathUtils.Line(this.m_CurveData[subLane].m_Bezier), position, out num3);
                            if (num2 < num)
                            {
                                curvePos = num3;
                                sidewalk = subLane;
                                num = num2;
                                result = true;
                            }
                        }
                    }
                }
                return result;
            }

            // Token: 0x06005FAB RID: 24491 RVA: 0x003B6CEC File Offset: 0x003B4EEC
            private int AddCollectionRequests(Entity edgeEntity, Entity request, ref Game.Vehicles.GarbageTruck garbageTruck)
            {
                int num = 0;
                if (this.m_ConnectedBuildings.HasBuffer(edgeEntity))
                {
                    DynamicBuffer<ConnectedBuilding> dynamicBuffer = this.m_ConnectedBuildings[edgeEntity];
                    for (int i = 0; i < dynamicBuffer.Length; i++)
                    {
                        Entity building = dynamicBuffer[i].m_Building;
                        GarbageProducer garbageProducer;
                        if (this.m_GarbageProducerData.TryGetComponent(building, out garbageProducer) && ((garbageTruck.m_State & GarbageTruckFlags.IndustrialWasteOnly) == (GarbageTruckFlags)0U || this.IsIndustrial(this.m_PrefabRefData[building].m_Prefab)))
                        {
                            num += garbageProducer.m_Garbage;
                            this.m_ActionQueue.Enqueue(new CustomGarbageTruckAISystem.GarbageAction
                            {
                                m_Type = CustomGarbageTruckAISystem.GarbageActionType.AddRequest,
                                m_Request = request,
                                m_Target = building
                            });
                        }
                    }
                }
                return num;
            }

            // Token: 0x06005FAC RID: 24492 RVA: 0x003B6DA8 File Offset: 0x003B4FA8
            private void CheckGarbagePresence(ref CarCurrentLane currentLaneData, ref Game.Vehicles.GarbageTruck garbageTruck, ref Car car, DynamicBuffer<CarNavigationLane> navigationLanes)
            {
                if ((currentLaneData.m_LaneFlags & (Game.Vehicles.CarLaneFlags.Waypoint | Game.Vehicles.CarLaneFlags.Checked)) == Game.Vehicles.CarLaneFlags.Waypoint)
                {
                    if (!this.CheckGarbagePresence(currentLaneData.m_Lane, ref garbageTruck))
                    {
                        currentLaneData.m_LaneFlags &= ~Game.Vehicles.CarLaneFlags.Waypoint;
                        car.m_Flags &= ~(CarFlags.Warning | CarFlags.Working);
                    }
                    currentLaneData.m_LaneFlags |= Game.Vehicles.CarLaneFlags.Checked;
                }
                if ((currentLaneData.m_LaneFlags & Game.Vehicles.CarLaneFlags.Waypoint) != (Game.Vehicles.CarLaneFlags)0U)
                {
                    car.m_Flags |= ((math.abs(currentLaneData.m_CurvePosition.x - currentLaneData.m_CurvePosition.z) < 0.5f) ? (CarFlags.Warning | CarFlags.Working) : CarFlags.Warning);
                    return;
                }
                for (int i = 0; i < navigationLanes.Length; i++)
                {
                    ref CarNavigationLane ptr = ref navigationLanes.ElementAt(i);
                    if ((ptr.m_Flags & (Game.Vehicles.CarLaneFlags.Waypoint | Game.Vehicles.CarLaneFlags.Checked)) == Game.Vehicles.CarLaneFlags.Waypoint)
                    {
                        if (!this.CheckGarbagePresence(ptr.m_Lane, ref garbageTruck))
                        {
                            ptr.m_Flags &= ~Game.Vehicles.CarLaneFlags.Waypoint;
                            car.m_Flags &= ~CarFlags.Warning;
                        }
                        ptr.m_Flags |= Game.Vehicles.CarLaneFlags.Checked;
                        car.m_Flags &= ~CarFlags.Working;
                    }
                    if ((ptr.m_Flags & (Game.Vehicles.CarLaneFlags.Reserved | Game.Vehicles.CarLaneFlags.Waypoint)) != Game.Vehicles.CarLaneFlags.Reserved)
                    {
                        car.m_Flags &= ~CarFlags.Working;
                        return;
                    }
                }
            }

            // Token: 0x06005FAD RID: 24493 RVA: 0x003B6EE8 File Offset: 0x003B50E8
            private bool CheckGarbagePresence(Entity laneEntity, ref Game.Vehicles.GarbageTruck garbageTruck)
            {
                if (this.m_EdgeLaneData.HasComponent(laneEntity) && this.m_OwnerData.HasComponent(laneEntity))
                {
                    Entity owner = this.m_OwnerData[laneEntity].m_Owner;
                    if (this.m_ConnectedBuildings.HasBuffer(owner))
                    {
                        DynamicBuffer<ConnectedBuilding> dynamicBuffer = this.m_ConnectedBuildings[owner];
                        for (int i = 0; i < dynamicBuffer.Length; i++)
                        {
                            Entity building = dynamicBuffer[i].m_Building;
                            if (this.m_GarbageProducerData.HasComponent(building) && this.m_GarbageProducerData[building].m_Garbage > this.m_GarbageParameters.m_CollectionGarbageLimit && ((garbageTruck.m_State & GarbageTruckFlags.IndustrialWasteOnly) == (GarbageTruckFlags)0U || this.IsIndustrial(this.m_PrefabRefData[building].m_Prefab)))
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }

            // Token: 0x06005FAE RID: 24494 RVA: 0x003B6FB6 File Offset: 0x003B51B6
            private void TryCollectGarbage(int jobIndex, Entity vehicleEntity, GarbageTruckData prefabGarbageTruckData, ref Game.Vehicles.GarbageTruck garbageTruck, ref Car car, ref CarCurrentLane currentLaneData, Entity ignoreBuilding)
            {
                if (garbageTruck.m_Garbage < prefabGarbageTruckData.m_GarbageCapacity)
                {
                    this.TryCollectGarbageFromLane(jobIndex, vehicleEntity, prefabGarbageTruckData, ref garbageTruck, ref car, currentLaneData.m_Lane, ignoreBuilding);
                }
            }

            // Token: 0x06005FAF RID: 24495 RVA: 0x003B6FDD File Offset: 0x003B51DD
            private void TryCollectGarbage(int jobIndex, Entity vehicleEntity, GarbageTruckData prefabGarbageTruckData, ref Game.Vehicles.GarbageTruck garbageTruck, ref Car car, ref Target target)
            {
                if (garbageTruck.m_Garbage < prefabGarbageTruckData.m_GarbageCapacity)
                {
                    this.TryCollectGarbageFromBuilding(jobIndex, vehicleEntity, prefabGarbageTruckData, ref garbageTruck, ref car, target.m_Target);
                }
            }

            // Token: 0x06005FB0 RID: 24496 RVA: 0x003B7004 File Offset: 0x003B5204
            private void TryCollectGarbageFromLane(int jobIndex, Entity vehicleEntity, GarbageTruckData prefabGarbageTruckData, ref Game.Vehicles.GarbageTruck garbageTruck, ref Car car, Entity laneEntity, Entity ignoreBuilding)
            {
                Owner owner;
                DynamicBuffer<ConnectedBuilding> dynamicBuffer;
                if (this.m_EdgeLaneData.HasComponent(laneEntity) && this.m_OwnerData.TryGetComponent(laneEntity, out owner) && this.m_ConnectedBuildings.TryGetBuffer(owner.m_Owner, out dynamicBuffer))
                {
                    for (int i = 0; i < dynamicBuffer.Length; i++)
                    {
                        Entity building = dynamicBuffer[i].m_Building;
                        if (building != ignoreBuilding)
                        {
                            this.TryCollectGarbageFromBuilding(jobIndex, vehicleEntity, prefabGarbageTruckData, ref garbageTruck, ref car, building);
                        }
                    }
                }
            }

            // Token: 0x06005FB1 RID: 24497 RVA: 0x003B7080 File Offset: 0x003B5280
            private void TryCollectGarbageFromBuilding(int jobIndex, Entity vehicleEntity, GarbageTruckData prefabGarbageTruckData, ref Game.Vehicles.GarbageTruck garbageTruck, ref Car car, Entity buildingEntity)
            {
                GarbageProducer garbageProducer;
                if (this.m_GarbageProducerData.TryGetComponent(buildingEntity, out garbageProducer) && garbageProducer.m_Garbage > this.m_GarbageParameters.m_CollectionGarbageLimit)
                {
                    if ((garbageTruck.m_State & GarbageTruckFlags.IndustrialWasteOnly) != (GarbageTruckFlags)0U && !this.IsIndustrial(this.m_PrefabRefData[buildingEntity].m_Prefab))
                    {
                        return;
                    }
                    this.m_ActionQueue.Enqueue(new CustomGarbageTruckAISystem.GarbageAction
                    {
                        m_Type = CustomGarbageTruckAISystem.GarbageActionType.Collect,
                        m_Vehicle = vehicleEntity,
                        m_Target = buildingEntity,
                        m_Capacity = prefabGarbageTruckData.m_GarbageCapacity
                    });
                    if (garbageProducer.m_Garbage >= this.m_GarbageParameters.m_RequestGarbageLimit)
                    {
                        this.QuantityUpdated(jobIndex, buildingEntity, false);
                    }
                    car.m_Flags |= (CarFlags.Warning | CarFlags.Working);
                }
            }

            // Token: 0x06005FB2 RID: 24498 RVA: 0x003B7144 File Offset: 0x003B5344
            private void QuantityUpdated(int jobIndex, Entity buildingEntity, bool updateAll = false)
            {
                DynamicBuffer<Game.Objects.SubObject> dynamicBuffer;
                if (this.m_SubObjects.TryGetBuffer(buildingEntity, out dynamicBuffer))
                {
                    for (int i = 0; i < dynamicBuffer.Length; i++)
                    {
                        Entity subObject = dynamicBuffer[i].m_SubObject;
                        bool updateAll2 = false;
                        if (updateAll || this.m_QuantityData.HasComponent(subObject))
                        {
                            this.m_CommandBuffer.AddComponent<BatchesUpdated>(jobIndex, subObject, default(BatchesUpdated));
                            updateAll2 = true;
                        }
                        this.QuantityUpdated(jobIndex, subObject, updateAll2);
                    }
                }
            }

            // Token: 0x06005FB3 RID: 24499 RVA: 0x003B71B8 File Offset: 0x003B53B8
            private bool IsIndustrial(Entity prefab)
            {
                if (this.m_PrefabSpawnableBuildingData.HasComponent(prefab))
                {
                    SpawnableBuildingData spawnableBuildingData = this.m_PrefabSpawnableBuildingData[prefab];
                    if (this.m_PrefabZoneData.HasComponent(spawnableBuildingData.m_ZonePrefab))
                    {
                        return this.m_PrefabZoneData[spawnableBuildingData.m_ZonePrefab].m_AreaType == AreaType.Industrial;
                    }
                }
                return false;
            }

            // Token: 0x06005FB4 RID: 24500 RVA: 0x003B7210 File Offset: 0x003B5410
            private bool UnloadGarbage(int jobIndex, Entity vehicleEntity, GarbageTruckData prefabGarbageTruckData, Entity facilityEntity, ref Game.Vehicles.GarbageTruck garbageTruck, bool instant)
            {
                if (garbageTruck.m_Garbage > 0 && this.m_GarbageFacilityData.HasComponent(facilityEntity))
                {
                    this.m_ActionQueue.Enqueue(new CustomGarbageTruckAISystem.GarbageAction
                    {
                        m_Type = CustomGarbageTruckAISystem.GarbageActionType.Unload,
                        m_Vehicle = vehicleEntity,
                        m_Target = facilityEntity,
                        m_MaxAmount = math.select(Mathf.RoundToInt((float)(prefabGarbageTruckData.m_UnloadRate * 16) / 60f), garbageTruck.m_Garbage, instant)
                    });
                    return false;
                }
                if ((garbageTruck.m_State & GarbageTruckFlags.Unloading) != (GarbageTruckFlags)0U)
                {
                    garbageTruck.m_State &= ~GarbageTruckFlags.Unloading;
                    this.m_CommandBuffer.AddComponent<EffectsUpdated>(jobIndex, vehicleEntity, default(EffectsUpdated));
                }
                return true;
            }

            // Token: 0x06005FB5 RID: 24501 RVA: 0x003B72BE File Offset: 0x003B54BE
            void IJobChunk.Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                this.Execute(chunk, unfilteredChunkIndex, useEnabledMask, chunkEnabledMask);
            }

            // Token: 0x0400A8E3 RID: 43235
            [ReadOnly]
            public EntityTypeHandle m_EntityType;

            // Token: 0x0400A8E4 RID: 43236
            [ReadOnly]
            public ComponentTypeHandle<Owner> m_OwnerType;

            // Token: 0x0400A8E5 RID: 43237
            [ReadOnly]
            public ComponentTypeHandle<Unspawned> m_UnspawnedType;

            // Token: 0x0400A8E6 RID: 43238
            [ReadOnly]
            public ComponentTypeHandle<PrefabRef> m_PrefabRefType;

            // Token: 0x0400A8E7 RID: 43239
            [ReadOnly]
            public ComponentTypeHandle<PathInformation> m_PathInformationType;

            // Token: 0x0400A8E8 RID: 43240
            public ComponentTypeHandle<Game.Vehicles.GarbageTruck> m_GarbageTruckType;

            // Token: 0x0400A8E9 RID: 43241
            public ComponentTypeHandle<Car> m_CarType;

            // Token: 0x0400A8EA RID: 43242
            public ComponentTypeHandle<CarCurrentLane> m_CurrentLaneType;

            // Token: 0x0400A8EB RID: 43243
            public ComponentTypeHandle<Target> m_TargetType;

            // Token: 0x0400A8EC RID: 43244
            public ComponentTypeHandle<PathOwner> m_PathOwnerType;

            // Token: 0x0400A8ED RID: 43245
            public BufferTypeHandle<CarNavigationLane> m_CarNavigationLaneType;

            // Token: 0x0400A8EE RID: 43246
            public BufferTypeHandle<ServiceDispatch> m_ServiceDispatchType;

            // Token: 0x0400A8EF RID: 43247
            [ReadOnly]
            public ComponentLookup<Owner> m_OwnerData;

            // Token: 0x0400A8F0 RID: 43248
            [ReadOnly]
            public ComponentLookup<PathInformation> m_PathInformationData;

            // Token: 0x0400A8F1 RID: 43249
            [ReadOnly]
            public ComponentLookup<Quantity> m_QuantityData;

            // Token: 0x0400A8F2 RID: 43250
            [ReadOnly]
            public ComponentLookup<SlaveLane> m_SlaveLaneData;

            // Token: 0x0400A8F3 RID: 43251
            [ReadOnly]
            public ComponentLookup<EdgeLane> m_EdgeLaneData;

            // Token: 0x0400A8F4 RID: 43252
            [ReadOnly]
            public ComponentLookup<Game.Net.PedestrianLane> m_PedestrianLaneData;

            // Token: 0x0400A8F5 RID: 43253
            [ReadOnly]
            public ComponentLookup<Curve> m_CurveData;

            // Token: 0x0400A8F6 RID: 43254
            [ReadOnly]
            public ComponentLookup<CarData> m_PrefabCarData;

            // Token: 0x0400A8F7 RID: 43255
            [ReadOnly]
            public ComponentLookup<GarbageTruckData> m_PrefabGarbageTruckData;

            // Token: 0x0400A8F8 RID: 43256
            [ReadOnly]
            public ComponentLookup<PrefabRef> m_PrefabRefData;

            // Token: 0x0400A8F9 RID: 43257
            [ReadOnly]
            public ComponentLookup<SpawnableBuildingData> m_PrefabSpawnableBuildingData;

            // Token: 0x0400A8FA RID: 43258
            [ReadOnly]
            public ComponentLookup<ZoneData> m_PrefabZoneData;

            // Token: 0x0400A8FB RID: 43259
            [ReadOnly]
            public ComponentLookup<GarbageCollectionRequest> m_GarbageCollectionRequestData;

            // Token: 0x0400A8FC RID: 43260
            [ReadOnly]
            public ComponentLookup<GarbageProducer> m_GarbageProducerData;

            // Token: 0x0400A8FD RID: 43261
            [ReadOnly]
            public ComponentLookup<Game.Buildings.GarbageFacility> m_GarbageFacilityData;

            // Token: 0x0400A8FE RID: 43262
            [ReadOnly]
            public BufferLookup<Game.Objects.SubObject> m_SubObjects;

            // Token: 0x0400A8FF RID: 43263
            [ReadOnly]
            public BufferLookup<ConnectedBuilding> m_ConnectedBuildings;

            // Token: 0x0400A900 RID: 43264
            [ReadOnly]
            public BufferLookup<Game.Net.SubLane> m_SubLanes;

            // Token: 0x0400A901 RID: 43265
            [NativeDisableParallelForRestriction]
            public BufferLookup<PathElement> m_PathElements;

            // Token: 0x0400A902 RID: 43266
            [ReadOnly]
            public uint m_SimulationFrameIndex;

            // Token: 0x0400A903 RID: 43267
            [ReadOnly]
            public EntityArchetype m_GarbageCollectionRequestArchetype;

            // Token: 0x0400A904 RID: 43268
            [ReadOnly]
            public EntityArchetype m_HandleRequestArchetype;

            // Token: 0x0400A905 RID: 43269
            [ReadOnly]
            public GarbageParameterData m_GarbageParameters;

            // Token: 0x0400A906 RID: 43270
            public EntityCommandBuffer.ParallelWriter m_CommandBuffer;

            // Token: 0x0400A907 RID: 43271
            public NativeQueue<SetupQueueItem>.ParallelWriter m_PathfindQueue;

            // Token: 0x0400A908 RID: 43272
            public NativeQueue<CustomGarbageTruckAISystem.GarbageAction>.ParallelWriter m_ActionQueue;
        }

        [BurstCompile]
        private struct GarbageActionJob : IJob
        {
            // Token: 0x06005FB6 RID: 24502 RVA: 0x003B72CC File Offset: 0x003B54CC
            public void Execute()
            {
                CustomGarbageTruckAISystem.GarbageAction garbageAction;
                while (this.m_ActionQueue.TryDequeue(out garbageAction))
                {
                    switch (garbageAction.m_Type)
                    {
                        case CustomGarbageTruckAISystem.GarbageActionType.Collect:
                            {
                                Game.Vehicles.GarbageTruck garbageTruck = this.m_GarbageTruckData[garbageAction.m_Vehicle];
                                GarbageProducer garbageProducer = this.m_GarbageProducerData[garbageAction.m_Target];
                                int num = math.min(garbageAction.m_Capacity - garbageTruck.m_Garbage, garbageProducer.m_Garbage);
                                if (num > 0)
                                {
                                    garbageTruck.m_Garbage += num;
                                    garbageTruck.m_EstimatedGarbage = math.max(0, garbageTruck.m_EstimatedGarbage - num);
                                    garbageProducer.m_Garbage -= num;
                                    if ((garbageProducer.m_Flags & GarbageProducerFlags.GarbagePilingUpWarning) != GarbageProducerFlags.None && garbageProducer.m_Garbage <= this.m_GarbageParameters.m_WarningGarbageLimit)
                                    {
                                        this.m_IconCommandBuffer.Remove(garbageAction.m_Target, this.m_GarbageParameters.m_GarbageNotificationPrefab, default(Entity), (IconFlags)0);
                                        garbageProducer.m_Flags &= ~GarbageProducerFlags.GarbagePilingUpWarning;
                                    }
                                    this.m_GarbageTruckData[garbageAction.m_Vehicle] = garbageTruck;
                                    this.m_GarbageProducerData[garbageAction.m_Target] = garbageProducer;
                                    int num2 = Mathf.RoundToInt(this.m_GarbageFee * (float)num);
                                    if (this.m_BuildingConditions.HasComponent(garbageAction.m_Target))
                                    {
                                        BuildingCondition value = this.m_BuildingConditions[garbageAction.m_Target];
                                        if (GarbageFeeFactor == 1.0)
                                        {
                                            value.m_Condition -= num2;
                                        }
                                        else
                                        {
                                            value.m_Condition -= (int)((float)num2 * GarbageFeeFactor);
                                        }
                                        this.m_BuildingConditions[garbageAction.m_Target] = value;
                                        if (this.m_Owners.HasComponent(garbageAction.m_Vehicle) && !this.m_OutsideConnections.HasComponent(this.m_Owners[garbageAction.m_Vehicle].m_Owner))
                                        {
                                            this.m_StatisticsEventQueue.Enqueue(new StatisticsEvent
                                            {
                                                m_Statistic = StatisticType.Income,
                                                m_Change = (float)num2,
                                                m_Parameter = 12
                                            });
                                            this.m_FeeQueue.Enqueue(new ServiceFeeSystem.FeeEvent
                                            {
                                                m_Amount = (float)num,
                                                m_Cost = (float)num2,
                                                m_Outside = false,
                                                m_Resource = PlayerResource.Garbage
                                            });
                                        }
                                    }
                                    DynamicBuffer<Efficiency> buffer;
                                    if (this.m_Efficiencies.TryGetBuffer(garbageAction.m_Target, out buffer))
                                    {
                                        float garbageEfficiencyFactor = GarbageAccumulationSystem.GetGarbageEfficiencyFactor(garbageProducer.m_Garbage, this.m_GarbageParameters, this.m_GarbageEfficiencyPenalty);
                                        BuildingUtils.SetEfficiencyFactor(buffer, EfficiencyFactor.Garbage, garbageEfficiencyFactor);
                                    }
                                }
                                break;
                            }
                        case CustomGarbageTruckAISystem.GarbageActionType.Unload:
                            {
                                Game.Vehicles.GarbageTruck garbageTruck2 = this.m_GarbageTruckData[garbageAction.m_Vehicle];
                                int num3 = garbageTruck2.m_Garbage;
                                num3 = math.min(num3, garbageAction.m_MaxAmount);
                                if (num3 > 0)
                                {
                                    garbageTruck2.m_Garbage -= num3;
                                    if (this.m_EconomyResources.HasBuffer(garbageAction.m_Target))
                                    {
                                        DynamicBuffer<Game.Economy.Resources> resources = this.m_EconomyResources[garbageAction.m_Target];
                                        EconomyUtils.AddResources(Resource.Garbage, num3, resources);
                                    }
                                    if ((garbageTruck2.m_State & GarbageTruckFlags.Unloading) == (GarbageTruckFlags)0U)
                                    {
                                        garbageTruck2.m_State |= GarbageTruckFlags.Unloading;
                                        this.m_CommandBuffer.AddComponent<EffectsUpdated>(garbageAction.m_Vehicle, default(EffectsUpdated));
                                    }
                                    this.m_GarbageTruckData[garbageAction.m_Vehicle] = garbageTruck2;
                                }
                                else if ((garbageTruck2.m_State & GarbageTruckFlags.Unloading) != (GarbageTruckFlags)0U)
                                {
                                    garbageTruck2.m_State &= ~GarbageTruckFlags.Unloading;
                                    this.m_CommandBuffer.AddComponent<EffectsUpdated>(garbageAction.m_Vehicle, default(EffectsUpdated));
                                }
                                break;
                            }
                        case CustomGarbageTruckAISystem.GarbageActionType.AddRequest:
                            {
                                GarbageProducer value2 = this.m_GarbageProducerData[garbageAction.m_Target];
                                value2.m_CollectionRequest = garbageAction.m_Request;
                                this.m_GarbageProducerData[garbageAction.m_Target] = value2;
                                break;
                            }
                    }
                }
            }

            // Token: 0x0400A909 RID: 43273
            public ComponentLookup<Game.Vehicles.GarbageTruck> m_GarbageTruckData;

            // Token: 0x0400A90A RID: 43274
            public ComponentLookup<GarbageProducer> m_GarbageProducerData;

            // Token: 0x0400A90B RID: 43275
            public BufferLookup<Game.Economy.Resources> m_EconomyResources;

            // Token: 0x0400A90C RID: 43276
            public ComponentLookup<BuildingCondition> m_BuildingConditions;

            // Token: 0x0400A90D RID: 43277
            public BufferLookup<Efficiency> m_Efficiencies;

            // Token: 0x0400A90E RID: 43278
            [ReadOnly]
            public ComponentLookup<Owner> m_Owners;

            // Token: 0x0400A90F RID: 43279
            [ReadOnly]
            public ComponentLookup<Game.Objects.OutsideConnection> m_OutsideConnections;

            // Token: 0x0400A910 RID: 43280
            [ReadOnly]
            public GarbageParameterData m_GarbageParameters;

            // Token: 0x0400A911 RID: 43281
            public float m_GarbageFee;

            // Token: 0x0400A912 RID: 43282
            public float m_GarbageEfficiencyPenalty;

            // Token: 0x0400A913 RID: 43283
            public NativeQueue<CustomGarbageTruckAISystem.GarbageAction> m_ActionQueue;

            // Token: 0x0400A914 RID: 43284
            public NativeQueue<ServiceFeeSystem.FeeEvent> m_FeeQueue;

            // Token: 0x0400A915 RID: 43285
            public NativeQueue<StatisticsEvent>.ParallelWriter m_StatisticsEventQueue;

            // Token: 0x0400A916 RID: 43286
            public IconCommandBuffer m_IconCommandBuffer;

            // Token: 0x0400A917 RID: 43287
            public EntityCommandBuffer m_CommandBuffer;
        }

        // Token: 0x020015EE RID: 5614
        private struct TypeHandle
        {
            // Token: 0x06005FB7 RID: 24503 RVA: 0x003B7664 File Offset: 0x003B5864
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void __AssignHandles(ref SystemState state)
            {
                this.__Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
                this.__Game_Common_Owner_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Owner>(true);
                this.__Game_Objects_Unspawned_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Unspawned>(true);
                this.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle = state.GetComponentTypeHandle<PrefabRef>(true);
                this.__Game_Pathfind_PathInformation_RO_ComponentTypeHandle = state.GetComponentTypeHandle<PathInformation>(true);
                this.__Game_Vehicles_GarbageTruck_RW_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Vehicles.GarbageTruck>(false);
                this.__Game_Vehicles_Car_RW_ComponentTypeHandle = state.GetComponentTypeHandle<Car>(false);
                this.__Game_Vehicles_CarCurrentLane_RW_ComponentTypeHandle = state.GetComponentTypeHandle<CarCurrentLane>(false);
                this.__Game_Common_Target_RW_ComponentTypeHandle = state.GetComponentTypeHandle<Target>(false);
                this.__Game_Pathfind_PathOwner_RW_ComponentTypeHandle = state.GetComponentTypeHandle<PathOwner>(false);
                this.__Game_Vehicles_CarNavigationLane_RW_BufferTypeHandle = state.GetBufferTypeHandle<CarNavigationLane>(false);
                this.__Game_Simulation_ServiceDispatch_RW_BufferTypeHandle = state.GetBufferTypeHandle<ServiceDispatch>(false);
                this.__Game_Common_Owner_RO_ComponentLookup = state.GetComponentLookup<Owner>(true);
                this.__Game_Pathfind_PathInformation_RO_ComponentLookup = state.GetComponentLookup<PathInformation>(true);
                this.__Game_Objects_Quantity_RO_ComponentLookup = state.GetComponentLookup<Quantity>(true);
                this.__Game_Net_EdgeLane_RO_ComponentLookup = state.GetComponentLookup<EdgeLane>(true);
                this.__Game_Net_PedestrianLane_RO_ComponentLookup = state.GetComponentLookup<Game.Net.PedestrianLane>(true);
                this.__Game_Net_Curve_RO_ComponentLookup = state.GetComponentLookup<Curve>(true);
                this.__Game_Net_SlaveLane_RO_ComponentLookup = state.GetComponentLookup<SlaveLane>(true);
                this.__Game_Prefabs_CarData_RO_ComponentLookup = state.GetComponentLookup<CarData>(true);
                this.__Game_Prefabs_GarbageTruckData_RO_ComponentLookup = state.GetComponentLookup<GarbageTruckData>(true);
                this.__Game_Prefabs_PrefabRef_RO_ComponentLookup = state.GetComponentLookup<PrefabRef>(true);
                this.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup = state.GetComponentLookup<SpawnableBuildingData>(true);
                this.__Game_Prefabs_ZoneData_RO_ComponentLookup = state.GetComponentLookup<ZoneData>(true);
                this.__Game_Simulation_GarbageCollectionRequest_RO_ComponentLookup = state.GetComponentLookup<GarbageCollectionRequest>(true);
                this.__Game_Buildings_GarbageProducer_RO_ComponentLookup = state.GetComponentLookup<GarbageProducer>(true);
                this.__Game_Buildings_GarbageFacility_RO_ComponentLookup = state.GetComponentLookup<Game.Buildings.GarbageFacility>(true);
                this.__Game_Buildings_ConnectedBuilding_RO_BufferLookup = state.GetBufferLookup<ConnectedBuilding>(true);
                this.__Game_Objects_SubObject_RO_BufferLookup = state.GetBufferLookup<Game.Objects.SubObject>(true);
                this.__Game_Net_SubLane_RO_BufferLookup = state.GetBufferLookup<Game.Net.SubLane>(true);
                this.__Game_Pathfind_PathElement_RW_BufferLookup = state.GetBufferLookup<PathElement>(false);
                this.__Game_Vehicles_GarbageTruck_RW_ComponentLookup = state.GetComponentLookup<Game.Vehicles.GarbageTruck>(false);
                this.__Game_Buildings_GarbageProducer_RW_ComponentLookup = state.GetComponentLookup<GarbageProducer>(false);
                this.__Game_Economy_Resources_RW_BufferLookup = state.GetBufferLookup<Game.Economy.Resources>(false);
                this.__Game_Buildings_BuildingCondition_RW_ComponentLookup = state.GetComponentLookup<BuildingCondition>(false);
                this.__Game_Buildings_Efficiency_RW_BufferLookup = state.GetBufferLookup<Efficiency>(false);
                this.__Game_Objects_OutsideConnection_RO_ComponentLookup = state.GetComponentLookup<Game.Objects.OutsideConnection>(true);
            }

            // Token: 0x0400A918 RID: 43288
            [ReadOnly]
            public EntityTypeHandle __Unity_Entities_Entity_TypeHandle;

            // Token: 0x0400A919 RID: 43289
            [ReadOnly]
            public ComponentTypeHandle<Owner> __Game_Common_Owner_RO_ComponentTypeHandle;

            // Token: 0x0400A91A RID: 43290
            [ReadOnly]
            public ComponentTypeHandle<Unspawned> __Game_Objects_Unspawned_RO_ComponentTypeHandle;

            // Token: 0x0400A91B RID: 43291
            [ReadOnly]
            public ComponentTypeHandle<PrefabRef> __Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;

            // Token: 0x0400A91C RID: 43292
            [ReadOnly]
            public ComponentTypeHandle<PathInformation> __Game_Pathfind_PathInformation_RO_ComponentTypeHandle;

            // Token: 0x0400A91D RID: 43293
            public ComponentTypeHandle<Game.Vehicles.GarbageTruck> __Game_Vehicles_GarbageTruck_RW_ComponentTypeHandle;

            // Token: 0x0400A91E RID: 43294
            public ComponentTypeHandle<Car> __Game_Vehicles_Car_RW_ComponentTypeHandle;

            // Token: 0x0400A91F RID: 43295
            public ComponentTypeHandle<CarCurrentLane> __Game_Vehicles_CarCurrentLane_RW_ComponentTypeHandle;

            // Token: 0x0400A920 RID: 43296
            public ComponentTypeHandle<Target> __Game_Common_Target_RW_ComponentTypeHandle;

            // Token: 0x0400A921 RID: 43297
            public ComponentTypeHandle<PathOwner> __Game_Pathfind_PathOwner_RW_ComponentTypeHandle;

            // Token: 0x0400A922 RID: 43298
            public BufferTypeHandle<CarNavigationLane> __Game_Vehicles_CarNavigationLane_RW_BufferTypeHandle;

            // Token: 0x0400A923 RID: 43299
            public BufferTypeHandle<ServiceDispatch> __Game_Simulation_ServiceDispatch_RW_BufferTypeHandle;

            // Token: 0x0400A924 RID: 43300
            [ReadOnly]
            public ComponentLookup<Owner> __Game_Common_Owner_RO_ComponentLookup;

            // Token: 0x0400A925 RID: 43301
            [ReadOnly]
            public ComponentLookup<PathInformation> __Game_Pathfind_PathInformation_RO_ComponentLookup;

            // Token: 0x0400A926 RID: 43302
            [ReadOnly]
            public ComponentLookup<Quantity> __Game_Objects_Quantity_RO_ComponentLookup;

            // Token: 0x0400A927 RID: 43303
            [ReadOnly]
            public ComponentLookup<EdgeLane> __Game_Net_EdgeLane_RO_ComponentLookup;

            // Token: 0x0400A928 RID: 43304
            [ReadOnly]
            public ComponentLookup<Game.Net.PedestrianLane> __Game_Net_PedestrianLane_RO_ComponentLookup;

            // Token: 0x0400A929 RID: 43305
            [ReadOnly]
            public ComponentLookup<Curve> __Game_Net_Curve_RO_ComponentLookup;

            // Token: 0x0400A92A RID: 43306
            [ReadOnly]
            public ComponentLookup<SlaveLane> __Game_Net_SlaveLane_RO_ComponentLookup;

            // Token: 0x0400A92B RID: 43307
            [ReadOnly]
            public ComponentLookup<CarData> __Game_Prefabs_CarData_RO_ComponentLookup;

            // Token: 0x0400A92C RID: 43308
            [ReadOnly]
            public ComponentLookup<GarbageTruckData> __Game_Prefabs_GarbageTruckData_RO_ComponentLookup;

            // Token: 0x0400A92D RID: 43309
            [ReadOnly]
            public ComponentLookup<PrefabRef> __Game_Prefabs_PrefabRef_RO_ComponentLookup;

            // Token: 0x0400A92E RID: 43310
            [ReadOnly]
            public ComponentLookup<SpawnableBuildingData> __Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup;

            // Token: 0x0400A92F RID: 43311
            [ReadOnly]
            public ComponentLookup<ZoneData> __Game_Prefabs_ZoneData_RO_ComponentLookup;

            // Token: 0x0400A930 RID: 43312
            [ReadOnly]
            public ComponentLookup<GarbageCollectionRequest> __Game_Simulation_GarbageCollectionRequest_RO_ComponentLookup;

            // Token: 0x0400A931 RID: 43313
            [ReadOnly]
            public ComponentLookup<GarbageProducer> __Game_Buildings_GarbageProducer_RO_ComponentLookup;

            // Token: 0x0400A932 RID: 43314
            [ReadOnly]
            public ComponentLookup<Game.Buildings.GarbageFacility> __Game_Buildings_GarbageFacility_RO_ComponentLookup;

            // Token: 0x0400A933 RID: 43315
            [ReadOnly]
            public BufferLookup<ConnectedBuilding> __Game_Buildings_ConnectedBuilding_RO_BufferLookup;

            // Token: 0x0400A934 RID: 43316
            [ReadOnly]
            public BufferLookup<Game.Objects.SubObject> __Game_Objects_SubObject_RO_BufferLookup;

            // Token: 0x0400A935 RID: 43317
            [ReadOnly]
            public BufferLookup<Game.Net.SubLane> __Game_Net_SubLane_RO_BufferLookup;

            // Token: 0x0400A936 RID: 43318
            public BufferLookup<PathElement> __Game_Pathfind_PathElement_RW_BufferLookup;

            // Token: 0x0400A937 RID: 43319
            public ComponentLookup<Game.Vehicles.GarbageTruck> __Game_Vehicles_GarbageTruck_RW_ComponentLookup;

            // Token: 0x0400A938 RID: 43320
            public ComponentLookup<GarbageProducer> __Game_Buildings_GarbageProducer_RW_ComponentLookup;

            // Token: 0x0400A939 RID: 43321
            public BufferLookup<Game.Economy.Resources> __Game_Economy_Resources_RW_BufferLookup;

            // Token: 0x0400A93A RID: 43322
            public ComponentLookup<BuildingCondition> __Game_Buildings_BuildingCondition_RW_ComponentLookup;

            // Token: 0x0400A93B RID: 43323
            public BufferLookup<Efficiency> __Game_Buildings_Efficiency_RW_BufferLookup;

            // Token: 0x0400A93C RID: 43324
            [ReadOnly]
            public ComponentLookup<Game.Objects.OutsideConnection> __Game_Objects_OutsideConnection_RO_ComponentLookup;
        }
    }
}