using Game;
using Game.Simulation;
using System.Runtime.CompilerServices;
using Game.Areas;
using Game.Buildings;
using Game.Common;
using Game.Net;
using Game.Objects;
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
    // Token: 0x02001068 RID: 4200
    [CompilerGenerated]
    public class CustomRentInitializeSystem : GameSystemBase
    {
        // Token: 0x06004949 RID: 18761 RVA: 0x00088ED7 File Offset: 0x000870D7
        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return 16;
        }

        // Token: 0x0600494A RID: 18762 RVA: 0x0031033C File Offset: 0x0030E53C
        [Preserve]
        protected override void OnCreate()
        {
            base.OnCreate();
            RentPaymentFactor = Plugin.RentPaymentFactor.Value;
            RentPaymentFactor = RentPaymentFactor > 0 ? RentPaymentFactor : 0;
            RentPaymentFactor = RentPaymentFactor < 100 ? RentPaymentFactor : 100;
            this.m_EndFrameBarrier = base.World.GetOrCreateSystemManaged<EndFrameBarrier>();
            this.m_RandomSeed = new Unity.Mathematics.Random(4235262U);
            this.m_ResidentialPropertyGroup = base.GetEntityQuery(new ComponentType[]
            {
                ComponentType.ReadOnly<ResidentialProperty>(),
                ComponentType.ReadOnly<Building>(),
                ComponentType.ReadOnly<PrefabRef>(),
                ComponentType.ReadOnly<PropertyToBeOnMarket>(),
                ComponentType.Exclude<CommercialProperty>(),
                ComponentType.Exclude<PropertyOnMarket>(),
                ComponentType.Exclude<Deleted>(),
                ComponentType.Exclude<Temp>()
            });
            this.m_CommercialPropertyGroup = base.GetEntityQuery(new ComponentType[]
            {
                ComponentType.ReadOnly<CommercialProperty>(),
                ComponentType.ReadOnly<PrefabRef>(),
                ComponentType.ReadOnly<PropertyToBeOnMarket>(),
                ComponentType.Exclude<ResidentialProperty>(),
                ComponentType.Exclude<Deleted>(),
                ComponentType.Exclude<Temp>()
            });
            this.m_IndustrialPropertyGroup = base.GetEntityQuery(new ComponentType[]
            {
                ComponentType.ReadOnly<IndustrialProperty>(),
                ComponentType.ReadOnly<PrefabRef>(),
                ComponentType.ReadOnly<PropertyToBeOnMarket>(),
                ComponentType.Exclude<ExtractorProperty>(),
                ComponentType.Exclude<Deleted>(),
                ComponentType.Exclude<Temp>()
            });
            this.m_MixedPropertyGroup = base.GetEntityQuery(new ComponentType[]
            {
                ComponentType.ReadOnly<ResidentialProperty>(),
                ComponentType.ReadOnly<CommercialProperty>(),
                ComponentType.ReadOnly<PrefabRef>(),
                ComponentType.ReadOnly<PropertyToBeOnMarket>(),
                ComponentType.Exclude<Deleted>(),
                ComponentType.Exclude<Temp>()
            });
            this.m_ExtractorPropertyQuery = base.GetEntityQuery(new ComponentType[]
            {
                ComponentType.ReadOnly<ExtractorProperty>(),
                ComponentType.ReadOnly<PrefabRef>(),
                ComponentType.ReadOnly<PropertyToBeOnMarket>(),
                ComponentType.Exclude<Deleted>(),
                ComponentType.Exclude<Temp>()
            });
        }

        // Token: 0x0600494B RID: 18763 RVA: 0x00310540 File Offset: 0x0030E740
        [Preserve]
        protected override void OnUpdate()
        {
            JobHandle jobHandle = default(JobHandle);
            if (!this.m_ResidentialPropertyGroup.IsEmptyIgnoreFilter)
            {
                this.__TypeHandle.__Game_Buildings_PropertyOnMarket_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_ConsumptionData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Net_LandValue_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Buildings_Building_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref base.CheckedStateRef);
                CustomRentInitializeSystem.InitializeResidentialRentJob initializeResidentialRentJob = default(CustomRentInitializeSystem.InitializeResidentialRentJob);
                initializeResidentialRentJob.m_EntityType = this.__TypeHandle.__Unity_Entities_Entity_TypeHandle;
                initializeResidentialRentJob.m_PrefabType = this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;
                initializeResidentialRentJob.m_BuildingType = this.__TypeHandle.__Game_Buildings_Building_RO_ComponentTypeHandle;
                initializeResidentialRentJob.m_BuildingProperties = this.__TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;
                initializeResidentialRentJob.m_BuildingDatas = this.__TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup;
                initializeResidentialRentJob.m_LandValues = this.__TypeHandle.__Game_Net_LandValue_RO_ComponentLookup;
                initializeResidentialRentJob.m_ConsumptionDatas = this.__TypeHandle.__Game_Prefabs_ConsumptionData_RO_ComponentLookup;
                initializeResidentialRentJob.m_PropertiesOnMarket = this.__TypeHandle.__Game_Buildings_PropertyOnMarket_RO_ComponentLookup;
                initializeResidentialRentJob.m_CommandBuffer = this.m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter();
                CustomRentInitializeSystem.InitializeResidentialRentJob jobData = initializeResidentialRentJob;
                jobHandle = JobHandle.CombineDependencies(jobHandle, jobData.ScheduleParallel(this.m_ResidentialPropertyGroup, base.Dependency));
                this.m_EndFrameBarrier.AddJobHandleForProducer(jobHandle);
            }
            if (!this.m_ExtractorPropertyQuery.IsEmptyIgnoreFilter)
            {
                this.__TypeHandle.__Game_Objects_Attached_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Areas_Geometry_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Areas_Lot_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Areas_SubArea_RO_BufferLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_ZoneData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Net_LandValue_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Buildings_PropertyOnMarket_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_ConsumptionData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Buildings_Building_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref base.CheckedStateRef);
                CustomRentInitializeSystem.InitializeExtractorRentJob initializeExtractorRentJob = default(CustomRentInitializeSystem.InitializeExtractorRentJob);
                initializeExtractorRentJob.m_EntityType = this.__TypeHandle.__Unity_Entities_Entity_TypeHandle;
                initializeExtractorRentJob.m_PrefabType = this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;
                initializeExtractorRentJob.m_BuildingType = this.__TypeHandle.__Game_Buildings_Building_RO_ComponentTypeHandle;
                initializeExtractorRentJob.m_BuildingProperties = this.__TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;
                initializeExtractorRentJob.m_ConsumptionDatas = this.__TypeHandle.__Game_Prefabs_ConsumptionData_RO_ComponentLookup;
                initializeExtractorRentJob.m_PropertiesOnMarket = this.__TypeHandle.__Game_Buildings_PropertyOnMarket_RO_ComponentLookup;
                initializeExtractorRentJob.m_LandValues = this.__TypeHandle.__Game_Net_LandValue_RO_ComponentLookup;
                initializeExtractorRentJob.m_BuildingDatas = this.__TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup;
                initializeExtractorRentJob.m_SpawnableBuildingData = this.__TypeHandle.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup;
                initializeExtractorRentJob.m_ZoneData = this.__TypeHandle.__Game_Prefabs_ZoneData_RO_ComponentLookup;
                initializeExtractorRentJob.m_SubAreas = this.__TypeHandle.__Game_Areas_SubArea_RO_BufferLookup;
                initializeExtractorRentJob.m_Lots = this.__TypeHandle.__Game_Areas_Lot_RO_ComponentLookup;
                initializeExtractorRentJob.m_Geometries = this.__TypeHandle.__Game_Areas_Geometry_RO_ComponentLookup;
                initializeExtractorRentJob.m_Attached = this.__TypeHandle.__Game_Objects_Attached_RO_ComponentLookup;
                initializeExtractorRentJob.m_CommandBuffer = this.m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter();
                CustomRentInitializeSystem.InitializeExtractorRentJob jobData2 = initializeExtractorRentJob;
                jobHandle = JobHandle.CombineDependencies(jobHandle, jobData2.ScheduleParallel(this.m_ExtractorPropertyQuery, base.Dependency));
                this.m_EndFrameBarrier.AddJobHandleForProducer(jobHandle);
            }
            if (!this.m_CommercialPropertyGroup.IsEmptyIgnoreFilter)
            {
                this.__TypeHandle.__Game_Prefabs_ZoneData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Net_LandValue_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Buildings_PropertyOnMarket_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_ConsumptionData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Buildings_Building_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref base.CheckedStateRef);
                CustomRentInitializeSystem.InitializeOtherRentJob initializeOtherRentJob = default(CustomRentInitializeSystem.InitializeOtherRentJob);
                initializeOtherRentJob.m_EntityType = this.__TypeHandle.__Unity_Entities_Entity_TypeHandle;
                initializeOtherRentJob.m_PrefabType = this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;
                initializeOtherRentJob.m_BuildingType = this.__TypeHandle.__Game_Buildings_Building_RO_ComponentTypeHandle;
                initializeOtherRentJob.m_BuildingProperties = this.__TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;
                initializeOtherRentJob.m_ConsumptionDatas = this.__TypeHandle.__Game_Prefabs_ConsumptionData_RO_ComponentLookup;
                initializeOtherRentJob.m_PropertiesOnMarket = this.__TypeHandle.__Game_Buildings_PropertyOnMarket_RO_ComponentLookup;
                initializeOtherRentJob.m_LandValues = this.__TypeHandle.__Game_Net_LandValue_RO_ComponentLookup;
                initializeOtherRentJob.m_BuildingDatas = this.__TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup;
                initializeOtherRentJob.m_SpawnableBuildingData = this.__TypeHandle.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup;
                initializeOtherRentJob.m_ZoneData = this.__TypeHandle.__Game_Prefabs_ZoneData_RO_ComponentLookup;
                initializeOtherRentJob.m_CommandBuffer = this.m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter();
                CustomRentInitializeSystem.InitializeOtherRentJob jobData3 = initializeOtherRentJob;
                jobHandle = JobHandle.CombineDependencies(jobHandle, jobData3.ScheduleParallel(this.m_CommercialPropertyGroup, base.Dependency));
                this.m_EndFrameBarrier.AddJobHandleForProducer(jobHandle);
            }
            if (!this.m_IndustrialPropertyGroup.IsEmptyIgnoreFilter)
            {
                this.__TypeHandle.__Game_Prefabs_ZoneData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Net_LandValue_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Buildings_PropertyOnMarket_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_ConsumptionData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Buildings_Building_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref base.CheckedStateRef);
                CustomRentInitializeSystem.InitializeOtherRentJob initializeOtherRentJob = default(CustomRentInitializeSystem.InitializeOtherRentJob);
                initializeOtherRentJob.m_EntityType = this.__TypeHandle.__Unity_Entities_Entity_TypeHandle;
                initializeOtherRentJob.m_PrefabType = this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;
                initializeOtherRentJob.m_BuildingType = this.__TypeHandle.__Game_Buildings_Building_RO_ComponentTypeHandle;
                initializeOtherRentJob.m_BuildingProperties = this.__TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;
                initializeOtherRentJob.m_ConsumptionDatas = this.__TypeHandle.__Game_Prefabs_ConsumptionData_RO_ComponentLookup;
                initializeOtherRentJob.m_PropertiesOnMarket = this.__TypeHandle.__Game_Buildings_PropertyOnMarket_RO_ComponentLookup;
                initializeOtherRentJob.m_LandValues = this.__TypeHandle.__Game_Net_LandValue_RO_ComponentLookup;
                initializeOtherRentJob.m_BuildingDatas = this.__TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup;
                initializeOtherRentJob.m_SpawnableBuildingData = this.__TypeHandle.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup;
                initializeOtherRentJob.m_ZoneData = this.__TypeHandle.__Game_Prefabs_ZoneData_RO_ComponentLookup;
                initializeOtherRentJob.m_CommandBuffer = this.m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter();
                CustomRentInitializeSystem.InitializeOtherRentJob jobData4 = initializeOtherRentJob;
                jobHandle = JobHandle.CombineDependencies(jobHandle, jobData4.ScheduleParallel(this.m_IndustrialPropertyGroup, base.Dependency));
                this.m_EndFrameBarrier.AddJobHandleForProducer(jobHandle);
            }
            if (!this.m_MixedPropertyGroup.IsEmptyIgnoreFilter)
            {
                this.__TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Net_LandValue_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Buildings_PropertyOnMarket_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_ConsumptionData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Buildings_Building_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref base.CheckedStateRef);
                CustomRentInitializeSystem.InitializeMixedPropertiesJob initializeMixedPropertiesJob = default(CustomRentInitializeSystem.InitializeMixedPropertiesJob);
                initializeMixedPropertiesJob.m_EntityType = this.__TypeHandle.__Unity_Entities_Entity_TypeHandle;
                initializeMixedPropertiesJob.m_PrefabType = this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;
                initializeMixedPropertiesJob.m_BuildingType = this.__TypeHandle.__Game_Buildings_Building_RO_ComponentTypeHandle;
                initializeMixedPropertiesJob.m_BuildingProperties = this.__TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;
                initializeMixedPropertiesJob.m_ConsumptionDatas = this.__TypeHandle.__Game_Prefabs_ConsumptionData_RO_ComponentLookup;
                initializeMixedPropertiesJob.m_PropertiesOnMarket = this.__TypeHandle.__Game_Buildings_PropertyOnMarket_RO_ComponentLookup;
                initializeMixedPropertiesJob.m_LandValues = this.__TypeHandle.__Game_Net_LandValue_RO_ComponentLookup;
                initializeMixedPropertiesJob.m_BuildingDatas = this.__TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup;
                initializeMixedPropertiesJob.m_CommandBuffer = this.m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter();
                CustomRentInitializeSystem.InitializeMixedPropertiesJob jobData5 = initializeMixedPropertiesJob;
                jobHandle = JobHandle.CombineDependencies(jobHandle, jobData5.ScheduleParallel(this.m_MixedPropertyGroup, base.Dependency));
                this.m_EndFrameBarrier.AddJobHandleForProducer(jobHandle);
            }
            base.Dependency = jobHandle;
        }

        // Token: 0x0600494C RID: 18764 RVA: 0x0005E08F File Offset: 0x0005C28F
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void __AssignQueries(ref SystemState state)
        {
        }

        // Token: 0x0600494D RID: 18765 RVA: 0x00088EDB File Offset: 0x000870DB
        protected override void OnCreateForCompiler()
        {
            base.OnCreateForCompiler();
            this.__AssignQueries(ref base.CheckedStateRef);
            this.__TypeHandle.__AssignHandles(ref base.CheckedStateRef);
        }

        // Token: 0x0600494E RID: 18766 RVA: 0x0005E948 File Offset: 0x0005CB48
        [Preserve]
        public CustomRentInitializeSystem()
        {
        }

        // Token: 0x04006ED0 RID: 28368
        private EntityQuery m_ResidentialPropertyGroup;

        // Token: 0x04006ED1 RID: 28369
        private EntityQuery m_CommercialPropertyGroup;

        // Token: 0x04006ED2 RID: 28370
        private EntityQuery m_IndustrialPropertyGroup;

        // Token: 0x04006ED3 RID: 28371
        private EntityQuery m_MixedPropertyGroup;

        // Token: 0x04006ED4 RID: 28372
        private EntityQuery m_ExtractorPropertyQuery;

        // Token: 0x04006ED5 RID: 28373
        private Unity.Mathematics.Random m_RandomSeed;

        // Token: 0x04006ED6 RID: 28374
        private EndFrameBarrier m_EndFrameBarrier;

        private static float RentPaymentFactor;

        // Token: 0x04006ED7 RID: 28375
        private CustomRentInitializeSystem.TypeHandle __TypeHandle;

        // Token: 0x02001069 RID: 4201
        [BurstCompile]
        public struct InitializeResidentialRentJob : IJobChunk
        {
            // Token: 0x0600494F RID: 18767 RVA: 0x00310EF8 File Offset: 0x0030F0F8
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> nativeArray = chunk.GetNativeArray(this.m_EntityType);
                NativeArray<PrefabRef> nativeArray2 = chunk.GetNativeArray<PrefabRef>(ref this.m_PrefabType);
                NativeArray<Building> nativeArray3 = chunk.GetNativeArray<Building>(ref this.m_BuildingType);
                for (int i = 0; i < nativeArray.Length; i++)
                {
                    Entity prefab = nativeArray2[i].m_Prefab;
                    BuildingPropertyData buildingProperties = this.m_BuildingProperties[prefab];
                    BuildingData buildingData = this.m_BuildingDatas[prefab];
                    int num = buildingData.m_LotSize.x * buildingData.m_LotSize.y;
                    Entity roadEdge = nativeArray3[i].m_RoadEdge;
                    float landValue = 0f;
                    if (this.m_LandValues.HasComponent(roadEdge))
                    {
                        landValue = (float)num * this.m_LandValues[roadEdge].m_LandValue;
                    }
                    int x = RentAdjustSystem.GetRent(this.m_ConsumptionDatas[prefab], buildingProperties, landValue, Game.Zones.AreaType.Residential).x;
                    if (!this.m_PropertiesOnMarket.HasComponent(nativeArray[i]))
                    {
                        this.m_CommandBuffer.RemoveComponent<PropertyToBeOnMarket>(unfilteredChunkIndex, nativeArray[i]);
                        this.m_CommandBuffer.AddComponent<PropertyOnMarket>(unfilteredChunkIndex, nativeArray[i], new PropertyOnMarket
                        {
                            m_AskingRent = RentPaymentFactor == 1.0 ? x : (int)((float)x * RentPaymentFactor)
                        });
                    }
                    else
                    {
                        this.m_CommandBuffer.RemoveComponent<PropertyOnMarket>(unfilteredChunkIndex, nativeArray[i]);
                    }
                }
            }

            // Token: 0x06004950 RID: 18768 RVA: 0x00088F00 File Offset: 0x00087100
            void IJobChunk.Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                this.Execute(chunk, unfilteredChunkIndex, useEnabledMask, chunkEnabledMask);
            }

            // Token: 0x04006ED8 RID: 28376
            [ReadOnly]
            public EntityTypeHandle m_EntityType;

            // Token: 0x04006ED9 RID: 28377
            [ReadOnly]
            public ComponentTypeHandle<PrefabRef> m_PrefabType;

            // Token: 0x04006EDA RID: 28378
            [ReadOnly]
            public ComponentTypeHandle<Building> m_BuildingType;

            // Token: 0x04006EDB RID: 28379
            public EntityCommandBuffer.ParallelWriter m_CommandBuffer;

            // Token: 0x04006EDC RID: 28380
            [ReadOnly]
            public ComponentLookup<BuildingData> m_BuildingDatas;

            // Token: 0x04006EDD RID: 28381
            [ReadOnly]
            public ComponentLookup<ConsumptionData> m_ConsumptionDatas;

            // Token: 0x04006EDE RID: 28382
            [ReadOnly]
            public ComponentLookup<BuildingPropertyData> m_BuildingProperties;

            // Token: 0x04006EDF RID: 28383
            [ReadOnly]
            public ComponentLookup<LandValue> m_LandValues;

            // Token: 0x04006EE0 RID: 28384
            [ReadOnly]
            public ComponentLookup<PropertyOnMarket> m_PropertiesOnMarket;
        }

        // Token: 0x0200106A RID: 4202
        public struct InitializeExtractorRentJob : IJobChunk
        {
            // Token: 0x06004951 RID: 18769 RVA: 0x00311050 File Offset: 0x0030F250
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> nativeArray = chunk.GetNativeArray(this.m_EntityType);
                NativeArray<PrefabRef> nativeArray2 = chunk.GetNativeArray<PrefabRef>(ref this.m_PrefabType);
                NativeArray<Building> nativeArray3 = chunk.GetNativeArray<Building>(ref this.m_BuildingType);
                for (int i = 0; i < nativeArray.Length; i++)
                {
                    Entity entity = nativeArray[i];
                    Entity prefab = nativeArray2[i].m_Prefab;
                    ConsumptionData consumptionData = this.m_ConsumptionDatas[prefab];
                    BuildingData buildingData = this.m_BuildingDatas[prefab];
                    BuildingPropertyData buildingProperties = this.m_BuildingProperties[prefab];
                    Game.Zones.AreaType areaType = Game.Zones.AreaType.None;
                    if (this.m_SpawnableBuildingData.HasComponent(prefab))
                    {
                        SpawnableBuildingData spawnableBuildingData = this.m_SpawnableBuildingData[prefab];
                        areaType = this.m_ZoneData[spawnableBuildingData.m_ZonePrefab].m_AreaType;
                    }
                    int num = buildingData.m_LotSize.x * buildingData.m_LotSize.y;
                    float num2 = 0f;
                    Attached attached;
                    DynamicBuffer<Game.Areas.SubArea> subAreas;
                    if (this.m_Attached.TryGetComponent(entity, out attached) && this.m_SubAreas.TryGetBuffer(attached.m_Parent, out subAreas))
                    {
                        num2 = ExtractorAISystem.GetArea(subAreas, this.m_Lots, this.m_Geometries);
                    }
                    num += Mathf.RoundToInt(0.5f * num2 / 64f);
                    Entity roadEdge = nativeArray3[i].m_RoadEdge;
                    float num3 = 0f;
                    if (this.m_LandValues.HasComponent(roadEdge))
                    {
                        num3 = (float)num * this.m_LandValues[roadEdge].m_LandValue;
                    }
                    int askingRent = Mathf.RoundToInt((float)RentAdjustSystem.GetRent(consumptionData, buildingProperties, num3, areaType).x + num3);
                    if (!this.m_PropertiesOnMarket.HasComponent(entity))
                    {
                        this.m_CommandBuffer.RemoveComponent<PropertyToBeOnMarket>(unfilteredChunkIndex, entity);
                        this.m_CommandBuffer.AddComponent<PropertyOnMarket>(unfilteredChunkIndex, entity, new PropertyOnMarket
                        {
                            m_AskingRent = RentPaymentFactor == 1.0 ? askingRent : (int)((float)askingRent * RentPaymentFactor)
                        });
                    }
                    else
                    {
                        this.m_CommandBuffer.RemoveComponent<PropertyOnMarket>(unfilteredChunkIndex, entity);
                    }
                }
            }

            // Token: 0x06004952 RID: 18770 RVA: 0x00088F0D File Offset: 0x0008710D
            void IJobChunk.Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                this.Execute(chunk, unfilteredChunkIndex, useEnabledMask, chunkEnabledMask);
            }

            // Token: 0x04006EE1 RID: 28385
            [ReadOnly]
            public EntityTypeHandle m_EntityType;

            // Token: 0x04006EE2 RID: 28386
            [ReadOnly]
            public ComponentTypeHandle<PrefabRef> m_PrefabType;

            // Token: 0x04006EE3 RID: 28387
            [ReadOnly]
            public ComponentTypeHandle<Building> m_BuildingType;

            // Token: 0x04006EE4 RID: 28388
            public EntityCommandBuffer.ParallelWriter m_CommandBuffer;

            // Token: 0x04006EE5 RID: 28389
            [ReadOnly]
            public ComponentLookup<BuildingData> m_BuildingDatas;

            // Token: 0x04006EE6 RID: 28390
            [ReadOnly]
            public ComponentLookup<ConsumptionData> m_ConsumptionDatas;

            // Token: 0x04006EE7 RID: 28391
            [ReadOnly]
            public ComponentLookup<BuildingPropertyData> m_BuildingProperties;

            // Token: 0x04006EE8 RID: 28392
            [ReadOnly]
            public ComponentLookup<PropertyOnMarket> m_PropertiesOnMarket;

            // Token: 0x04006EE9 RID: 28393
            [ReadOnly]
            public ComponentLookup<LandValue> m_LandValues;

            // Token: 0x04006EEA RID: 28394
            [ReadOnly]
            public ComponentLookup<SpawnableBuildingData> m_SpawnableBuildingData;

            // Token: 0x04006EEB RID: 28395
            [ReadOnly]
            public ComponentLookup<ZoneData> m_ZoneData;

            // Token: 0x04006EEC RID: 28396
            [ReadOnly]
            public BufferLookup<Game.Areas.SubArea> m_SubAreas;

            // Token: 0x04006EED RID: 28397
            [ReadOnly]
            public ComponentLookup<Game.Areas.Lot> m_Lots;

            // Token: 0x04006EEE RID: 28398
            [ReadOnly]
            public ComponentLookup<Geometry> m_Geometries;

            // Token: 0x04006EEF RID: 28399
            [ReadOnly]
            public ComponentLookup<Attached> m_Attached;
        }

        // Token: 0x0200106B RID: 4203
        public struct InitializeOtherRentJob : IJobChunk
        {
            // Token: 0x06004953 RID: 18771 RVA: 0x00311238 File Offset: 0x0030F438
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> nativeArray = chunk.GetNativeArray(this.m_EntityType);
                NativeArray<PrefabRef> nativeArray2 = chunk.GetNativeArray<PrefabRef>(ref this.m_PrefabType);
                NativeArray<Building> nativeArray3 = chunk.GetNativeArray<Building>(ref this.m_BuildingType);
                for (int i = 0; i < nativeArray.Length; i++)
                {
                    Entity prefab = nativeArray2[i].m_Prefab;
                    ConsumptionData consumptionData = this.m_ConsumptionDatas[prefab];
                    BuildingData buildingData = this.m_BuildingDatas[prefab];
                    BuildingPropertyData buildingProperties = this.m_BuildingProperties[prefab];
                    Game.Zones.AreaType areaType = Game.Zones.AreaType.None;
                    if (this.m_SpawnableBuildingData.HasComponent(prefab))
                    {
                        SpawnableBuildingData spawnableBuildingData = this.m_SpawnableBuildingData[prefab];
                        areaType = this.m_ZoneData[spawnableBuildingData.m_ZonePrefab].m_AreaType;
                    }
                    int num = buildingData.m_LotSize.x * buildingData.m_LotSize.y;
                    Entity roadEdge = nativeArray3[i].m_RoadEdge;
                    float num2 = 0f;
                    if (this.m_LandValues.HasComponent(roadEdge))
                    {
                        num2 = (float)num * this.m_LandValues[roadEdge].m_LandValue;
                    }
                    int askingRent = Mathf.RoundToInt((float)RentAdjustSystem.GetRent(consumptionData, buildingProperties, num2, areaType).x + num2);
                    if (!this.m_PropertiesOnMarket.HasComponent(nativeArray[i]))
                    {
                        this.m_CommandBuffer.RemoveComponent<PropertyToBeOnMarket>(unfilteredChunkIndex, nativeArray[i]);
                        this.m_CommandBuffer.AddComponent<PropertyOnMarket>(unfilteredChunkIndex, nativeArray[i], new PropertyOnMarket
                        {
                            m_AskingRent = RentPaymentFactor == 1.0 ? askingRent : (int)((float)askingRent * RentPaymentFactor)
                        });
                    }
                    else
                    {
                        this.m_CommandBuffer.RemoveComponent<PropertyOnMarket>(unfilteredChunkIndex, nativeArray[i]);
                    }
                }
            }

            // Token: 0x06004954 RID: 18772 RVA: 0x00088F1A File Offset: 0x0008711A
            void IJobChunk.Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                this.Execute(chunk, unfilteredChunkIndex, useEnabledMask, chunkEnabledMask);
            }

            // Token: 0x04006EF0 RID: 28400
            [ReadOnly]
            public EntityTypeHandle m_EntityType;

            // Token: 0x04006EF1 RID: 28401
            [ReadOnly]
            public ComponentTypeHandle<PrefabRef> m_PrefabType;

            // Token: 0x04006EF2 RID: 28402
            [ReadOnly]
            public ComponentTypeHandle<Building> m_BuildingType;

            // Token: 0x04006EF3 RID: 28403
            public EntityCommandBuffer.ParallelWriter m_CommandBuffer;

            // Token: 0x04006EF4 RID: 28404
            [ReadOnly]
            public ComponentLookup<BuildingData> m_BuildingDatas;

            // Token: 0x04006EF5 RID: 28405
            [ReadOnly]
            public ComponentLookup<ConsumptionData> m_ConsumptionDatas;

            // Token: 0x04006EF6 RID: 28406
            [ReadOnly]
            public ComponentLookup<BuildingPropertyData> m_BuildingProperties;

            // Token: 0x04006EF7 RID: 28407
            [ReadOnly]
            public ComponentLookup<PropertyOnMarket> m_PropertiesOnMarket;

            // Token: 0x04006EF8 RID: 28408
            [ReadOnly]
            public ComponentLookup<LandValue> m_LandValues;

            // Token: 0x04006EF9 RID: 28409
            [ReadOnly]
            public ComponentLookup<SpawnableBuildingData> m_SpawnableBuildingData;

            // Token: 0x04006EFA RID: 28410
            [ReadOnly]
            public ComponentLookup<ZoneData> m_ZoneData;
        }

        // Token: 0x0200106C RID: 4204
        public struct InitializeMixedPropertiesJob : IJobChunk
        {
            // Token: 0x06004955 RID: 18773 RVA: 0x003113D4 File Offset: 0x0030F5D4
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> nativeArray = chunk.GetNativeArray(this.m_EntityType);
                NativeArray<PrefabRef> nativeArray2 = chunk.GetNativeArray<PrefabRef>(ref this.m_PrefabType);
                NativeArray<Building> nativeArray3 = chunk.GetNativeArray<Building>(ref this.m_BuildingType);
                for (int i = 0; i < nativeArray.Length; i++)
                {
                    Entity prefab = nativeArray2[i].m_Prefab;
                    ConsumptionData consumptionData = this.m_ConsumptionDatas[prefab];
                    BuildingPropertyData buildingProperties = this.m_BuildingProperties[prefab];
                    BuildingData buildingData = this.m_BuildingDatas[prefab];
                    int num = buildingData.m_LotSize.x * buildingData.m_LotSize.y;
                    Entity roadEdge = nativeArray3[i].m_RoadEdge;
                    float landValue = 0f;
                    if (this.m_LandValues.HasComponent(roadEdge))
                    {
                        landValue = (float)num * this.m_LandValues[roadEdge].m_LandValue;
                    }
                    int x = RentAdjustSystem.GetRent(consumptionData, buildingProperties, landValue, Game.Zones.AreaType.Residential).x;
                    if (!this.m_PropertiesOnMarket.HasComponent(nativeArray[i]))
                    {
                        this.m_CommandBuffer.RemoveComponent<PropertyToBeOnMarket>(unfilteredChunkIndex, nativeArray[i]);
                        this.m_CommandBuffer.AddComponent<PropertyOnMarket>(unfilteredChunkIndex, nativeArray[i], new PropertyOnMarket
                        {
                            m_AskingRent = RentPaymentFactor == 1.0 ? x : (int)((float)x * RentPaymentFactor)
                        });
                    }
                    else
                    {
                        this.m_CommandBuffer.RemoveComponent<PropertyOnMarket>(unfilteredChunkIndex, nativeArray[i]);
                    }
                }
            }

            // Token: 0x06004956 RID: 18774 RVA: 0x00088F27 File Offset: 0x00087127
            void IJobChunk.Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                this.Execute(chunk, unfilteredChunkIndex, useEnabledMask, chunkEnabledMask);
            }

            // Token: 0x04006EFB RID: 28411
            [ReadOnly]
            public EntityTypeHandle m_EntityType;

            // Token: 0x04006EFC RID: 28412
            [ReadOnly]
            public ComponentTypeHandle<PrefabRef> m_PrefabType;

            // Token: 0x04006EFD RID: 28413
            [ReadOnly]
            public ComponentTypeHandle<Building> m_BuildingType;

            // Token: 0x04006EFE RID: 28414
            public EntityCommandBuffer.ParallelWriter m_CommandBuffer;

            // Token: 0x04006EFF RID: 28415
            [ReadOnly]
            public ComponentLookup<ConsumptionData> m_ConsumptionDatas;

            // Token: 0x04006F00 RID: 28416
            [ReadOnly]
            public ComponentLookup<BuildingPropertyData> m_BuildingProperties;

            // Token: 0x04006F01 RID: 28417
            [ReadOnly]
            public ComponentLookup<PropertyOnMarket> m_PropertiesOnMarket;

            // Token: 0x04006F02 RID: 28418
            [ReadOnly]
            public ComponentLookup<BuildingData> m_BuildingDatas;

            // Token: 0x04006F03 RID: 28419
            [ReadOnly]
            public ComponentLookup<LandValue> m_LandValues;
        }

        // Token: 0x0200106D RID: 4205
        private struct TypeHandle
        {
            // Token: 0x06004957 RID: 18775 RVA: 0x0031152C File Offset: 0x0030F72C
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void __AssignHandles(ref SystemState state)
            {
                this.__Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
                this.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle = state.GetComponentTypeHandle<PrefabRef>(true);
                this.__Game_Buildings_Building_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Building>(true);
                this.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup = state.GetComponentLookup<BuildingPropertyData>(true);
                this.__Game_Prefabs_BuildingData_RO_ComponentLookup = state.GetComponentLookup<BuildingData>(true);
                this.__Game_Net_LandValue_RO_ComponentLookup = state.GetComponentLookup<LandValue>(true);
                this.__Game_Prefabs_ConsumptionData_RO_ComponentLookup = state.GetComponentLookup<ConsumptionData>(true);
                this.__Game_Buildings_PropertyOnMarket_RO_ComponentLookup = state.GetComponentLookup<PropertyOnMarket>(true);
                this.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup = state.GetComponentLookup<SpawnableBuildingData>(true);
                this.__Game_Prefabs_ZoneData_RO_ComponentLookup = state.GetComponentLookup<ZoneData>(true);
                this.__Game_Areas_SubArea_RO_BufferLookup = state.GetBufferLookup<Game.Areas.SubArea>(true);
                this.__Game_Areas_Lot_RO_ComponentLookup = state.GetComponentLookup<Game.Areas.Lot>(true);
                this.__Game_Areas_Geometry_RO_ComponentLookup = state.GetComponentLookup<Geometry>(true);
                this.__Game_Objects_Attached_RO_ComponentLookup = state.GetComponentLookup<Attached>(true);
            }

            // Token: 0x04006F04 RID: 28420
            [ReadOnly]
            public EntityTypeHandle __Unity_Entities_Entity_TypeHandle;

            // Token: 0x04006F05 RID: 28421
            [ReadOnly]
            public ComponentTypeHandle<PrefabRef> __Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;

            // Token: 0x04006F06 RID: 28422
            [ReadOnly]
            public ComponentTypeHandle<Building> __Game_Buildings_Building_RO_ComponentTypeHandle;

            // Token: 0x04006F07 RID: 28423
            [ReadOnly]
            public ComponentLookup<BuildingPropertyData> __Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;

            // Token: 0x04006F08 RID: 28424
            [ReadOnly]
            public ComponentLookup<BuildingData> __Game_Prefabs_BuildingData_RO_ComponentLookup;

            // Token: 0x04006F09 RID: 28425
            [ReadOnly]
            public ComponentLookup<LandValue> __Game_Net_LandValue_RO_ComponentLookup;

            // Token: 0x04006F0A RID: 28426
            [ReadOnly]
            public ComponentLookup<ConsumptionData> __Game_Prefabs_ConsumptionData_RO_ComponentLookup;

            // Token: 0x04006F0B RID: 28427
            [ReadOnly]
            public ComponentLookup<PropertyOnMarket> __Game_Buildings_PropertyOnMarket_RO_ComponentLookup;

            // Token: 0x04006F0C RID: 28428
            [ReadOnly]
            public ComponentLookup<SpawnableBuildingData> __Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup;

            // Token: 0x04006F0D RID: 28429
            [ReadOnly]
            public ComponentLookup<ZoneData> __Game_Prefabs_ZoneData_RO_ComponentLookup;

            // Token: 0x04006F0E RID: 28430
            [ReadOnly]
            public BufferLookup<Game.Areas.SubArea> __Game_Areas_SubArea_RO_BufferLookup;

            // Token: 0x04006F0F RID: 28431
            [ReadOnly]
            public ComponentLookup<Game.Areas.Lot> __Game_Areas_Lot_RO_ComponentLookup;

            // Token: 0x04006F10 RID: 28432
            [ReadOnly]
            public ComponentLookup<Geometry> __Game_Areas_Geometry_RO_ComponentLookup;

            // Token: 0x04006F11 RID: 28433
            [ReadOnly]
            public ComponentLookup<Attached> __Game_Objects_Attached_RO_ComponentLookup;
        }
    }
}
