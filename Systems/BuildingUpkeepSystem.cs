using Game;
using Game.Simulation;
using System.Runtime.CompilerServices;
using Game.Buildings;
using Game.Common;
using Game.Economy;
using Game.Net;
using Game.Prefabs;
using Game.Tools;
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
    // Token: 0x020011FF RID: 4607
    [CompilerGenerated]
    public class BuildingUpkeepSystem_Custom : GameSystemBase
    {
        // Token: 0x06005005 RID: 20485 RVA: 0x0008B9F1 File Offset: 0x00089BF1
        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return 262144 / (BuildingUpkeepSystem_Custom.kUpdatesPerDay * 16);
        }

        // Token: 0x06005006 RID: 20486 RVA: 0x0008BA01 File Offset: 0x00089C01
        public static float GetHeatingMultiplier(float temperature)
        {
            return math.max(0f, 15f - temperature);
        }

        // Token: 0x06005007 RID: 20487 RVA: 0x00376890 File Offset: 0x00374A90
        [Preserve]
        protected override void OnCreate()
        {
            base.OnCreate();
            BuildingUpkeepFactor = Plugin.BuildingUpkeepFactor.Value;
            BuildingUpkeepFactor = BuildingUpkeepFactor > 0 ? BuildingUpkeepFactor : 0;
            BuildingUpkeepFactor = BuildingUpkeepFactor < 100 ? BuildingUpkeepFactor : 100;
            this.m_SimulationSystem = base.World.GetOrCreateSystemManaged<SimulationSystem>();
            this.m_EndFrameBarrier = base.World.GetOrCreateSystemManaged<EndFrameBarrier>();
            this.m_PropertyRenterSystem = base.World.GetOrCreateSystemManaged<CustomPropertyRenterSystem>();
            this.m_ResourceSystem = base.World.GetOrCreateSystemManaged<ResourceSystem>();
            this.m_ClimateSystem = base.World.GetOrCreateSystemManaged<ClimateSystem>();
            this.m_ExpenseQueue = new NativeQueue<int>(Allocator.Persistent);
            this.m_BuildingGroup = base.GetEntityQuery(new EntityQueryDesc[]
            {
                new EntityQueryDesc
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<BuildingCondition>(),
                        ComponentType.ReadOnly<PrefabRef>(),
                        ComponentType.ReadOnly<UpdateFrame>()
                    },
                    Any = new ComponentType[0],
                    None = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Abandoned>(),
                        ComponentType.ReadOnly<Destroyed>(),
                        ComponentType.ReadOnly<Deleted>(),
                        ComponentType.ReadOnly<Temp>()
                    }
                }
            });
        }

        // Token: 0x06005008 RID: 20488 RVA: 0x0008BA14 File Offset: 0x00089C14
        [Preserve]
        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.m_ExpenseQueue.Dispose();
        }

        // Token: 0x06005009 RID: 20489 RVA: 0x003769A0 File Offset: 0x00374BA0
        [Preserve]
        protected override void OnUpdate()
        {
            uint updateFrame = SimulationUtils.GetUpdateFrame(this.m_SimulationSystem.frameIndex, BuildingUpkeepSystem_Custom.kUpdatesPerDay, 16);
            this.__TypeHandle.__Game_Prefabs_ResourceData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_ZoneData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Net_ResourceAvailability_RO_BufferLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_ConsumptionData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Simulation_UpdateFrame_SharedComponentTypeHandle.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_Building_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
            this.__TypeHandle.__Game_Buildings_BuildingCondition_RW_ComponentTypeHandle.Update(ref base.CheckedStateRef);
            BuildingUpkeepSystem_Custom.BuildingUpkeepJob buildingUpkeepJob = default(BuildingUpkeepSystem_Custom.BuildingUpkeepJob);
            buildingUpkeepJob.m_ConditionType = this.__TypeHandle.__Game_Buildings_BuildingCondition_RW_ComponentTypeHandle;
            buildingUpkeepJob.m_PrefabType = this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;
            buildingUpkeepJob.m_EntityType = this.__TypeHandle.__Unity_Entities_Entity_TypeHandle;
            buildingUpkeepJob.m_BuildingType = this.__TypeHandle.__Game_Buildings_Building_RO_ComponentTypeHandle;
            buildingUpkeepJob.m_UpdateFrameType = this.__TypeHandle.__Game_Simulation_UpdateFrame_SharedComponentTypeHandle;
            buildingUpkeepJob.m_ConsumptionDatas = this.__TypeHandle.__Game_Prefabs_ConsumptionData_RO_ComponentLookup;
            buildingUpkeepJob.m_Availabilities = this.__TypeHandle.__Game_Net_ResourceAvailability_RO_BufferLookup;
            buildingUpkeepJob.m_BuildingDatas = this.__TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup;
            buildingUpkeepJob.m_BuildingPropertyDatas = this.__TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;
            buildingUpkeepJob.m_SpawnableBuildingData = this.__TypeHandle.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup;
            buildingUpkeepJob.m_ZoneData = this.__TypeHandle.__Game_Prefabs_ZoneData_RO_ComponentLookup;
            buildingUpkeepJob.m_ResourcePrefabs = this.m_ResourceSystem.GetPrefabs();
            buildingUpkeepJob.m_ResourceDatas = this.__TypeHandle.__Game_Prefabs_ResourceData_RO_ComponentLookup;
            buildingUpkeepJob.m_UpdateFrameIndex = updateFrame;
            buildingUpkeepJob.m_SimulationFrame = this.m_SimulationSystem.frameIndex;
            buildingUpkeepJob.m_CommandBuffer = this.m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter();
            buildingUpkeepJob.m_LandlordExpenseQueue = this.m_ExpenseQueue.AsParallelWriter();
            buildingUpkeepJob.m_TemperatureUpkeep = BuildingUpkeepSystem_Custom.GetHeatingMultiplier(this.m_ClimateSystem.temperature);
            BuildingUpkeepSystem_Custom.BuildingUpkeepJob jobData = buildingUpkeepJob;
            base.Dependency = jobData.ScheduleParallel(this.m_BuildingGroup, base.Dependency);
            this.m_EndFrameBarrier.AddJobHandleForProducer(base.Dependency);
            this.m_ResourceSystem.AddPrefabsReader(base.Dependency);
            this.__TypeHandle.__Game_Economy_Resources_RW_BufferLookup.Update(ref base.CheckedStateRef);
            BuildingUpkeepSystem_Custom.LandlordUpkeepJob landlordUpkeepJob = default(BuildingUpkeepSystem_Custom.LandlordUpkeepJob);
            landlordUpkeepJob.m_Resources = this.__TypeHandle.__Game_Economy_Resources_RW_BufferLookup;
            landlordUpkeepJob.m_Landlords = this.m_PropertyRenterSystem.Landlords;
            landlordUpkeepJob.m_Queue = this.m_ExpenseQueue;
            BuildingUpkeepSystem_Custom.LandlordUpkeepJob jobData2 = landlordUpkeepJob;
            base.Dependency = jobData2.Schedule(base.Dependency);
        }

        // Token: 0x0600500A RID: 20490 RVA: 0x0005E08F File Offset: 0x0005C28F
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void __AssignQueries(ref SystemState state)
        {
        }

        // Token: 0x0600500B RID: 20491 RVA: 0x0008BA27 File Offset: 0x00089C27
        protected override void OnCreateForCompiler()
        {
            base.OnCreateForCompiler();
            this.__AssignQueries(ref base.CheckedStateRef);
            this.__TypeHandle.__AssignHandles(ref base.CheckedStateRef);
        }

        // Token: 0x0600500C RID: 20492 RVA: 0x0005E948 File Offset: 0x0005CB48
        [Preserve]
        public BuildingUpkeepSystem_Custom()
        {
        }

        // Token: 0x0400844F RID: 33871
        public static readonly int kUpdatesPerDay = 16;

        // Token: 0x04008450 RID: 33872
        public static readonly int kMaterialUpkeep = 4;

        // Token: 0x04008451 RID: 33873
        private SimulationSystem m_SimulationSystem;

        // Token: 0x04008452 RID: 33874
        private EndFrameBarrier m_EndFrameBarrier;

        // Token: 0x04008453 RID: 33875
        private CustomPropertyRenterSystem m_PropertyRenterSystem;

        // Token: 0x04008454 RID: 33876
        private ResourceSystem m_ResourceSystem;

        // Token: 0x04008455 RID: 33877
        private ClimateSystem m_ClimateSystem;

        // Token: 0x04008456 RID: 33878
        private NativeQueue<int> m_ExpenseQueue;

        // Token: 0x04008457 RID: 33879
        private EntityQuery m_BuildingGroup;

        private static float BuildingUpkeepFactor;

        // Token: 0x04008458 RID: 33880
        private BuildingUpkeepSystem_Custom.TypeHandle __TypeHandle;

        // Token: 0x02001200 RID: 4608
        [BurstCompile]
        private struct LandlordUpkeepJob : IJob
        {
            // Token: 0x0600500E RID: 20494 RVA: 0x00376CC4 File Offset: 0x00374EC4
            public void Execute()
            {
                if (this.m_Resources.HasBuffer(this.m_Landlords))
                {
                    int num = 0;
                    int num2;
                    while (this.m_Queue.TryDequeue(out num2))
                    {
                        num += num2;
                    }
                    EconomyUtils.AddResources(Resource.Money, num, this.m_Resources[this.m_Landlords]);
                }
            }

            // Token: 0x04008459 RID: 33881
            public BufferLookup<Game.Economy.Resources> m_Resources;

            // Token: 0x0400845A RID: 33882
            public Entity m_Landlords;

            // Token: 0x0400845B RID: 33883
            public NativeQueue<int> m_Queue;
        }

        // Token: 0x02001201 RID: 4609
        [BurstCompile]
        private struct BuildingUpkeepJob : IJobChunk
        {
            // Token: 0x0600500F RID: 20495 RVA: 0x00376D18 File Offset: 0x00374F18
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                if (chunk.GetSharedComponent<UpdateFrame>(this.m_UpdateFrameType).m_Index != this.m_UpdateFrameIndex)
                {
                    return;
                }
                NativeArray<Entity> nativeArray = chunk.GetNativeArray(this.m_EntityType);
                NativeArray<PrefabRef> nativeArray2 = chunk.GetNativeArray<PrefabRef>(ref this.m_PrefabType);
                NativeArray<BuildingCondition> nativeArray3 = chunk.GetNativeArray<BuildingCondition>(ref this.m_ConditionType);
                NativeArray<Building> nativeArray4 = chunk.GetNativeArray<Building>(ref this.m_BuildingType);
                int num = 0;
                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity entity = nativeArray[i];
                    Entity prefab = nativeArray2[i].m_Prefab;
                    BuildingData buildingData = this.m_BuildingDatas[prefab];
                    BuildingPropertyData buildingPropertyData = this.m_BuildingPropertyDatas[prefab];
                    BuildingCondition buildingCondition = nativeArray3[i];
                    int num2 = this.m_ConsumptionDatas[prefab].m_Upkeep / BuildingUpkeepSystem_Custom.kUpdatesPerDay;
                    if (buildingCondition.m_Condition < 0)
                    {
                        AreaType type = AreaType.None;
                        if (this.m_SpawnableBuildingData.HasComponent(prefab))
                        {
                            SpawnableBuildingData spawnableBuildingData = this.m_SpawnableBuildingData[prefab];
                            type = this.m_ZoneData[spawnableBuildingData.m_ZonePrefab].m_AreaType;
                        }
                        num2 = Mathf.RoundToInt((float)num2 / PropertyRenterSystem.GetUpkeepExponent(type));
                    }
                    if(BuildingUpkeepFactor == 1.0)
                    {
                        buildingCondition.m_Condition -= num2;
                    }
                    else
                    {
                        buildingCondition.m_Condition -= (int)((float)num2 * BuildingUpkeepFactor);
                    }
                    nativeArray3[i] = buildingCondition;
                    int num3 = num2 / BuildingUpkeepSystem_Custom.kMaterialUpkeep;
                    num += num2 - num3;
                    Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)(1L + (long)entity.Index * (long)((ulong)this.m_SimulationFrame)));
                    Resource resource = random.NextBool() ? Resource.Timber : Resource.Concrete;
                    float price = this.m_ResourceDatas[this.m_ResourcePrefabs[resource]].m_Price;
                    float num4 = math.sqrt((float)(buildingData.m_LotSize.x * buildingData.m_LotSize.y * buildingPropertyData.CountProperties())) * this.m_TemperatureUpkeep / (float)BuildingUpkeepSystem_Custom.kUpdatesPerDay;
                    if (random.NextInt(Mathf.RoundToInt(4000f * price)) < num3)
                    {
                        Entity e = this.m_CommandBuffer.CreateEntity(unfilteredChunkIndex);
                        this.m_CommandBuffer.AddComponent<GoodsDeliveryRequest>(unfilteredChunkIndex, e, new GoodsDeliveryRequest
                        {
                            m_Amount = math.max(num2, 4000),
                            m_Flags = (GoodsDeliveryFlags.BuildingUpkeep | GoodsDeliveryFlags.CommercialAllowed | GoodsDeliveryFlags.IndustrialAllowed | GoodsDeliveryFlags.ImportAllowed),
                            m_Resource = resource,
                            m_Target = entity
                        });
                    }
                    Building building = nativeArray4[i];
                    if (this.m_Availabilities.HasBuffer(building.m_RoadEdge))
                    {
                        float availability = NetUtils.GetAvailability(this.m_Availabilities[building.m_RoadEdge], AvailableResource.WoodSupply, building.m_CurvePosition);
                        float availability2 = NetUtils.GetAvailability(this.m_Availabilities[building.m_RoadEdge], AvailableResource.PetrochemicalsSupply, building.m_CurvePosition);
                        float num5 = availability + availability2;
                        if (num5 < 0.001f)
                        {
                            resource = (random.NextBool() ? Resource.Wood : Resource.Petrochemicals);
                        }
                        else
                        {
                            resource = ((random.NextFloat(num5) <= availability) ? Resource.Wood : Resource.Petrochemicals);
                            num2 = ((resource == Resource.Wood) ? 4000 : 800);
                        }
                        price = this.m_ResourceDatas[this.m_ResourcePrefabs[resource]].m_Price;
                        if (random.NextFloat((float)num2 * price) < num4)
                        {
                            Entity e2 = this.m_CommandBuffer.CreateEntity(unfilteredChunkIndex);
                            int num6 = Mathf.RoundToInt((float)num2 * price);
                            this.m_CommandBuffer.AddComponent<GoodsDeliveryRequest>(unfilteredChunkIndex, e2, new GoodsDeliveryRequest
                            {
                                m_Amount = num2,
                                m_Flags = (GoodsDeliveryFlags.BuildingUpkeep | GoodsDeliveryFlags.CommercialAllowed | GoodsDeliveryFlags.IndustrialAllowed | GoodsDeliveryFlags.ImportAllowed),
                                m_Resource = resource,
                                m_Target = entity
                            });
                            num += num6;
                        }
                    }
                }
                this.m_LandlordExpenseQueue.Enqueue(-num);
            }

            // Token: 0x06005010 RID: 20496 RVA: 0x0008BA5B File Offset: 0x00089C5B
            void IJobChunk.Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                this.Execute(chunk, unfilteredChunkIndex, useEnabledMask, chunkEnabledMask);
            }

            // Token: 0x0400845C RID: 33884
            [ReadOnly]
            public EntityTypeHandle m_EntityType;

            // Token: 0x0400845D RID: 33885
            public ComponentTypeHandle<BuildingCondition> m_ConditionType;

            // Token: 0x0400845E RID: 33886
            [ReadOnly]
            public SharedComponentTypeHandle<UpdateFrame> m_UpdateFrameType;

            // Token: 0x0400845F RID: 33887
            [ReadOnly]
            public ComponentTypeHandle<PrefabRef> m_PrefabType;

            // Token: 0x04008460 RID: 33888
            [ReadOnly]
            public ComponentTypeHandle<Building> m_BuildingType;

            // Token: 0x04008461 RID: 33889
            [ReadOnly]
            public ResourcePrefabs m_ResourcePrefabs;

            // Token: 0x04008462 RID: 33890
            [ReadOnly]
            public ComponentLookup<ResourceData> m_ResourceDatas;

            // Token: 0x04008463 RID: 33891
            [ReadOnly]
            public ComponentLookup<BuildingData> m_BuildingDatas;

            // Token: 0x04008464 RID: 33892
            [ReadOnly]
            public ComponentLookup<BuildingPropertyData> m_BuildingPropertyDatas;

            // Token: 0x04008465 RID: 33893
            [ReadOnly]
            public ComponentLookup<SpawnableBuildingData> m_SpawnableBuildingData;

            // Token: 0x04008466 RID: 33894
            [ReadOnly]
            public ComponentLookup<ZoneData> m_ZoneData;

            // Token: 0x04008467 RID: 33895
            [ReadOnly]
            public ComponentLookup<ConsumptionData> m_ConsumptionDatas;

            // Token: 0x04008468 RID: 33896
            [ReadOnly]
            public BufferLookup<ResourceAvailability> m_Availabilities;

            // Token: 0x04008469 RID: 33897
            public uint m_UpdateFrameIndex;

            // Token: 0x0400846A RID: 33898
            public uint m_SimulationFrame;

            // Token: 0x0400846B RID: 33899
            public float m_TemperatureUpkeep;

            // Token: 0x0400846C RID: 33900
            public NativeQueue<int>.ParallelWriter m_LandlordExpenseQueue;

            // Token: 0x0400846D RID: 33901
            public EntityCommandBuffer.ParallelWriter m_CommandBuffer;
        }

        // Token: 0x02001202 RID: 4610
        private struct TypeHandle
        {
            // Token: 0x06005011 RID: 20497 RVA: 0x003770C0 File Offset: 0x003752C0
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void __AssignHandles(ref SystemState state)
            {
                this.__Game_Buildings_BuildingCondition_RW_ComponentTypeHandle = state.GetComponentTypeHandle<BuildingCondition>(false);
                this.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle = state.GetComponentTypeHandle<PrefabRef>(true);
                this.__Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
                this.__Game_Buildings_Building_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Building>(true);
                this.__Game_Simulation_UpdateFrame_SharedComponentTypeHandle = state.GetSharedComponentTypeHandle<UpdateFrame>();
                this.__Game_Prefabs_ConsumptionData_RO_ComponentLookup = state.GetComponentLookup<ConsumptionData>(true);
                this.__Game_Net_ResourceAvailability_RO_BufferLookup = state.GetBufferLookup<ResourceAvailability>(true);
                this.__Game_Prefabs_BuildingData_RO_ComponentLookup = state.GetComponentLookup<BuildingData>(true);
                this.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup = state.GetComponentLookup<BuildingPropertyData>(true);
                this.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup = state.GetComponentLookup<SpawnableBuildingData>(true);
                this.__Game_Prefabs_ZoneData_RO_ComponentLookup = state.GetComponentLookup<ZoneData>(true);
                this.__Game_Prefabs_ResourceData_RO_ComponentLookup = state.GetComponentLookup<ResourceData>(true);
                this.__Game_Economy_Resources_RW_BufferLookup = state.GetBufferLookup<Game.Economy.Resources>(false);
            }

            // Token: 0x0400846E RID: 33902
            public ComponentTypeHandle<BuildingCondition> __Game_Buildings_BuildingCondition_RW_ComponentTypeHandle;

            // Token: 0x0400846F RID: 33903
            [ReadOnly]
            public ComponentTypeHandle<PrefabRef> __Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;

            // Token: 0x04008470 RID: 33904
            [ReadOnly]
            public EntityTypeHandle __Unity_Entities_Entity_TypeHandle;

            // Token: 0x04008471 RID: 33905
            [ReadOnly]
            public ComponentTypeHandle<Building> __Game_Buildings_Building_RO_ComponentTypeHandle;

            // Token: 0x04008472 RID: 33906
            public SharedComponentTypeHandle<UpdateFrame> __Game_Simulation_UpdateFrame_SharedComponentTypeHandle;

            // Token: 0x04008473 RID: 33907
            [ReadOnly]
            public ComponentLookup<ConsumptionData> __Game_Prefabs_ConsumptionData_RO_ComponentLookup;

            // Token: 0x04008474 RID: 33908
            [ReadOnly]
            public BufferLookup<ResourceAvailability> __Game_Net_ResourceAvailability_RO_BufferLookup;

            // Token: 0x04008475 RID: 33909
            [ReadOnly]
            public ComponentLookup<BuildingData> __Game_Prefabs_BuildingData_RO_ComponentLookup;

            // Token: 0x04008476 RID: 33910
            [ReadOnly]
            public ComponentLookup<BuildingPropertyData> __Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;

            // Token: 0x04008477 RID: 33911
            [ReadOnly]
            public ComponentLookup<SpawnableBuildingData> __Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup;

            // Token: 0x04008478 RID: 33912
            [ReadOnly]
            public ComponentLookup<ZoneData> __Game_Prefabs_ZoneData_RO_ComponentLookup;

            // Token: 0x04008479 RID: 33913
            [ReadOnly]
            public ComponentLookup<ResourceData> __Game_Prefabs_ResourceData_RO_ComponentLookup;

            // Token: 0x0400847A RID: 33914
            public BufferLookup<Game.Economy.Resources> __Game_Economy_Resources_RW_BufferLookup;
        }
    }
}
