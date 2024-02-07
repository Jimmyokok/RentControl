using Game;
using Game.Simulation;
using System.Runtime.CompilerServices;
using Game.Areas;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Economy;
using Game.Net;
using Game.Objects;
using Game.Prefabs;
using Game.Tools;
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
    // Token: 0x02001338 RID: 4920
    [CompilerGenerated]
    public class LandValueSystem_Custom : GameSystemBase
    {
        // Token: 0x06005570 RID: 21872 RVA: 0x00088ED7 File Offset: 0x000870D7
        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return 16;
        }

        // Token: 0x06005571 RID: 21873 RVA: 0x003B103C File Offset: 0x003AF23C
        [Preserve]
        protected override void OnCreate()
        {
            base.OnCreate();
            enable_LandValueCap = Plugin.enable_LandValueCap.Value;
            m_LandValueCap = (float)Plugin.m_LandValueCap.Value / 240;
            m_LandValueCap = m_LandValueCap > 0 ? m_LandValueCap : 0;
            this.m_GroundPollutionSystem = base.World.GetOrCreateSystemManaged<GroundPollutionSystem>();
            this.m_PollutionParameterQuery = base.GetEntityQuery(new ComponentType[]
            {
                ComponentType.ReadOnly<PollutionParameterData>()
            });
            this.m_EdgeGroup = base.GetEntityQuery(new EntityQueryDesc[]
            {
                new EntityQueryDesc
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Edge>(),
                        ComponentType.ReadWrite<LandValue>(),
                        ComponentType.ReadOnly<Curve>(),
                        ComponentType.ReadOnly<ConnectedBuilding>()
                    },
                    Any = new ComponentType[0],
                    None = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Deleted>(),
                        ComponentType.ReadOnly<Temp>()
                    }
                }
            });
            this.m_NodeGroup = base.GetEntityQuery(new EntityQueryDesc[]
            {
                new EntityQueryDesc
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Game.Net.Node>(),
                        ComponentType.ReadWrite<LandValue>(),
                        ComponentType.ReadOnly<ConnectedEdge>()
                    },
                    Any = new ComponentType[0],
                    None = new ComponentType[]
                    {
                        ComponentType.ReadOnly<Deleted>(),
                        ComponentType.ReadOnly<Temp>()
                    }
                }
            });
            base.RequireAnyForUpdate(new EntityQuery[]
            {
                this.m_EdgeGroup,
                this.m_NodeGroup
            });
        }

        // Token: 0x06005572 RID: 21874 RVA: 0x003B117C File Offset: 0x003AF37C
        [Preserve]
        protected override void OnUpdate()
        {
            JobHandle jobHandle = base.Dependency;
            if (!this.m_EdgeGroup.IsEmptyIgnoreFilter)
            {
                this.__TypeHandle.__Game_Areas_Geometry_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Areas_Lot_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Areas_SubArea_RO_BufferLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Objects_Attached_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Objects_Placeholder_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Citizens_Household_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_ConsumptionData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Common_Destroyed_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Buildings_Abandoned_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Buildings_Renter_RO_BufferLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Net_LandValue_RW_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Objects_Transform_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Buildings_ConnectedBuilding_RO_BufferTypeHandle.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Net_Curve_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Net_Edge_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref base.CheckedStateRef);
                LandValueSystem_Custom.EdgeUpdateJob jobData = default(LandValueSystem_Custom.EdgeUpdateJob);
                jobData.m_EntityType = this.__TypeHandle.__Unity_Entities_Entity_TypeHandle;
                jobData.m_EdgeType = this.__TypeHandle.__Game_Net_Edge_RO_ComponentTypeHandle;
                jobData.m_CurveType = this.__TypeHandle.__Game_Net_Curve_RO_ComponentTypeHandle;
                jobData.m_ConnectedBuildingType = this.__TypeHandle.__Game_Buildings_ConnectedBuilding_RO_BufferTypeHandle;
                jobData.m_BuildingDatas = this.__TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup;
                jobData.m_Transforms = this.__TypeHandle.__Game_Objects_Transform_RO_ComponentLookup;
                jobData.m_LandValues = this.__TypeHandle.__Game_Net_LandValue_RW_ComponentLookup;
                jobData.m_Prefabs = this.__TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup;
                jobData.m_PropertyRenters = this.__TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentLookup;
                jobData.m_RenterBuffers = this.__TypeHandle.__Game_Buildings_Renter_RO_BufferLookup;
                jobData.m_Abandoneds = this.__TypeHandle.__Game_Buildings_Abandoned_RO_ComponentLookup;
                jobData.m_Destroyeds = this.__TypeHandle.__Game_Common_Destroyed_RO_ComponentLookup;
                jobData.m_ConsumptionDatas = this.__TypeHandle.__Game_Prefabs_ConsumptionData_RO_ComponentLookup;
                jobData.m_PropertyDatas = this.__TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;
                jobData.m_Households = this.__TypeHandle.__Game_Citizens_Household_RO_ComponentLookup;
                jobData.m_Placeholders = this.__TypeHandle.__Game_Objects_Placeholder_RO_ComponentLookup;
                jobData.m_Attached = this.__TypeHandle.__Game_Objects_Attached_RO_ComponentLookup;
                jobData.m_SubAreas = this.__TypeHandle.__Game_Areas_SubArea_RO_BufferLookup;
                jobData.m_Lots = this.__TypeHandle.__Game_Areas_Lot_RO_ComponentLookup;
                jobData.m_Geometries = this.__TypeHandle.__Game_Areas_Geometry_RO_ComponentLookup;
                JobHandle job;
                jobData.m_PollutionMap = this.m_GroundPollutionSystem.GetMap(true, out job);
                jobData.m_PollutionParameters = this.m_PollutionParameterQuery.GetSingleton<PollutionParameterData>();
                jobHandle = jobData.ScheduleParallel(this.m_EdgeGroup, JobHandle.CombineDependencies(base.Dependency, job));
                this.m_GroundPollutionSystem.AddReader(jobHandle);
            }
            if (!this.m_NodeGroup.IsEmptyIgnoreFilter)
            {
                this.__TypeHandle.__Game_Net_LandValue_RW_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Net_Curve_RO_ComponentLookup.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Net_ConnectedEdge_RO_BufferTypeHandle.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Game_Net_Node_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
                this.__TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref base.CheckedStateRef);
                LandValueSystem_Custom.NodeUpdateJob jobData2 = default(LandValueSystem_Custom.NodeUpdateJob);
                jobData2.m_EntityType = this.__TypeHandle.__Unity_Entities_Entity_TypeHandle;
                jobData2.m_NodeType = this.__TypeHandle.__Game_Net_Node_RO_ComponentTypeHandle;
                jobData2.m_ConnectedEdgeType = this.__TypeHandle.__Game_Net_ConnectedEdge_RO_BufferTypeHandle;
                jobData2.m_Curves = this.__TypeHandle.__Game_Net_Curve_RO_ComponentLookup;
                jobData2.m_LandValues = this.__TypeHandle.__Game_Net_LandValue_RW_ComponentLookup;
                jobHandle = jobData2.ScheduleParallel(this.m_NodeGroup, jobHandle);
            }
            base.Dependency = jobHandle;
        }

        // Token: 0x06005573 RID: 21875 RVA: 0x0008E3E5 File Offset: 0x0008C5E5
        private static float GetDistanceFade(float distance)
        {
            return math.saturate(1f - distance / 2000f);
        }

        // Token: 0x06005574 RID: 21876 RVA: 0x0005E08F File Offset: 0x0005C28F
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void __AssignQueries(ref SystemState state)
        {
        }

        // Token: 0x06005575 RID: 21877 RVA: 0x0008E3F9 File Offset: 0x0008C5F9
        protected override void OnCreateForCompiler()
        {
            base.OnCreateForCompiler();
            this.__AssignQueries(ref base.CheckedStateRef);
            this.__TypeHandle.__AssignHandles(ref base.CheckedStateRef);
        }

        // Token: 0x06005576 RID: 21878 RVA: 0x0005E948 File Offset: 0x0005CB48
        [Preserve]
        public LandValueSystem_Custom()
        {
        }

        private GroundPollutionSystem m_GroundPollutionSystem;
        // Token: 0x04008FF7 RID: 36855
        private EntityQuery m_EdgeGroup;

        // Token: 0x04008FF8 RID: 36856
        private EntityQuery m_NodeGroup;

        private EntityQuery m_PollutionParameterQuery;

        private static float m_LandValueCap;
        private static bool enable_LandValueCap;

        // Token: 0x04008FF9 RID: 36857
        private LandValueSystem_Custom.TypeHandle __TypeHandle;

        // Token: 0x02001339 RID: 4921
        [BurstCompile]
        private struct EdgeUpdateJob : IJobChunk
        {
            // Token: 0x06005577 RID: 21879 RVA: 0x003B15A8 File Offset: 0x003AF7A8
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> nativeArray = chunk.GetNativeArray(this.m_EntityType);
                NativeArray<Edge> nativeArray2 = chunk.GetNativeArray<Edge>(ref this.m_EdgeType);
                NativeArray<Curve> nativeArray3 = chunk.GetNativeArray<Curve>(ref this.m_CurveType);
                BufferAccessor<ConnectedBuilding> bufferAccessor = chunk.GetBufferAccessor<ConnectedBuilding>(ref this.m_ConnectedBuildingType);
                for (int i = 0; i < nativeArray2.Length; i++)
                {
                    Entity entity = nativeArray[i];
                    Entity start = nativeArray2[i].m_Start;
                    Entity end = nativeArray2[i].m_End;
                    LandValue landValue = this.m_LandValues[entity];
                    int num = 0;
                    float num2 = 0f;
                    int num3 = 0;
                    float pollution_factor = 0f;
                    DynamicBuffer<ConnectedBuilding> dynamicBuffer = bufferAccessor[i];
                    for (int j = 0; j < dynamicBuffer.Length; j++)
                    {
                        Entity building = dynamicBuffer[j].m_Building;
                        if (this.m_Prefabs.HasComponent(building) && !this.m_Placeholders.HasComponent(building))
                        {
                            Entity prefab = this.m_Prefabs[building].m_Prefab;
                            if (this.m_PropertyDatas.HasComponent(prefab) && !this.m_Abandoneds.HasComponent(building) && !this.m_Destroyeds.HasComponent(building))
                            {
                                BuildingPropertyData buildingPropertyData = this.m_PropertyDatas[prefab];
                                if (buildingPropertyData.m_AllowedStored == Resource.NoResource)
                                {
                                    BuildingData buildingData = this.m_BuildingDatas[prefab];
                                    ConsumptionData consumptionData = this.m_ConsumptionDatas[prefab];
                                    int num4 = buildingPropertyData.CountProperties();
                                    bool flag = buildingPropertyData.m_ResidentialProperties > 0 && (buildingPropertyData.m_AllowedSold != Resource.NoResource || buildingPropertyData.m_AllowedManufactured > Resource.NoResource);
                                    int num5 = buildingData.m_LotSize.x * buildingData.m_LotSize.y;
                                    if (this.m_Attached.HasComponent(building))
                                    {
                                        Entity parent = this.m_Attached[building].m_Parent;
                                        if (this.m_SubAreas.HasBuffer(parent))
                                        {
                                            DynamicBuffer<Game.Areas.SubArea> subAreas = this.m_SubAreas[parent];
                                            num5 += Mathf.CeilToInt(ExtractorAISystem.GetArea(subAreas, this.m_Lots, this.m_Geometries));
                                        }
                                    }
                                    float num6 = landValue.m_LandValue * (float)num5 / (float)math.max(1, num4);
                                    float num7 = (float)consumptionData.m_Upkeep / (float)math.max(1, num4);
                                    if (this.m_RenterBuffers.HasBuffer(building))
                                    {
                                        DynamicBuffer<Renter> dynamicBuffer2 = this.m_RenterBuffers[building];
                                        for (int k = 0; k < dynamicBuffer2.Length; k++)
                                        {
                                            Entity renter = dynamicBuffer2[k].m_Renter;
                                            if (this.m_PropertyRenters.HasComponent(renter))
                                            {
                                                PropertyRenter propertyRenter = this.m_PropertyRenters[renter];
                                                if (flag && !this.m_Households.HasComponent(renter))
                                                {
                                                    num2 += (float)(((float)propertyRenter.m_MaxRent - RentAdjustSystem.kMixedCompanyRent * (float)consumptionData.m_Upkeep >= 3f * RentAdjustSystem.kMixedCompanyRent * landValue.m_LandValue) ? Mathf.RoundToInt(RentAdjustSystem.kMixedCompanyRent * (float)buildingPropertyData.m_ResidentialProperties) : (-(float)Mathf.RoundToInt(RentAdjustSystem.kMixedCompanyRent * (float)buildingPropertyData.m_ResidentialProperties)));
                                                }
                                                else
                                                {
                                                    num2 += (((float)propertyRenter.m_MaxRent - num7 >= 3f * num6) ? 1f : -1f);
                                                }
                                                num3++;
                                            }
                                        }
                                        num += num5;
                                        int num8 = num4 - dynamicBuffer2.Length;
                                        num2 -= (float)num8;
                                        num3 += num8;
                                    }
                                    if (this.m_Transforms.HasComponent(building))
                                    {
                                        pollution_factor = (float)GroundPollutionSystem.GetPollution(this.m_Transforms[building].m_Position, this.m_PollutionMap).m_Pollution / (float)this.m_PollutionParameters.m_GroundPollutionLandValueDivisor;
                                    }
                                }
                            }
                        }
                        else
                        {
                            num++;
                        }
                    }
                    float length = nativeArray3[i].m_Length;
                    float distanceFade = LandValueSystem_Custom.GetDistanceFade(length);
                    int num9 = num;
                    num = math.max(num, Mathf.CeilToInt(length / 4f));
                    num3 -= num - num9;
                    float2 @float = new float2(math.max(1f, this.m_LandValues[start].m_Weight), math.max(1f, this.m_LandValues[end].m_Weight));
                    float num10 = @float.x + @float.y;
                    float2 float2 = new float2(this.m_LandValues[start].m_LandValue, this.m_LandValues[end].m_LandValue);
                    @float *= distanceFade;
                    float y = math.lerp(float2.x, float2.y, @float.y / num10);
                    float y2 = 0f;
                    if (num3 > 0)
                    {
                        y2 = 0.1f * num2 / (float)num3;
                    }
                    if (pollution_factor > 0f)
                    {
                        pollution_factor = math.lerp(0f, 2f, pollution_factor / 50f);
                    }
                    landValue.m_Weight = math.max(1f, math.lerp(landValue.m_Weight, (float)num, 0.1f));
                    float s = num10 / (99f * landValue.m_Weight + num10);
                    landValue.m_LandValue = math.lerp(landValue.m_LandValue, y, s);
                    if (landValue.m_LandValue > 30f)
                    {
                        y2 -= pollution_factor * 0.2f;
                    }
                    landValue.m_LandValue += math.min(1f, math.max(-1f, y2));
                    landValue.m_LandValue = math.max(landValue.m_LandValue, 0f);
                    if (enable_LandValueCap)
                    {
                        landValue.m_LandValue = math.min(landValue.m_LandValue, m_LandValueCap);
                    }
                    landValue.m_Weight = math.lerp(landValue.m_Weight, math.max(1f, 0.5f * num10), s);
                    this.m_LandValues[entity] = landValue;
                }
            }

            // Token: 0x06005578 RID: 21880 RVA: 0x0008E41E File Offset: 0x0008C61E
            void IJobChunk.Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                this.Execute(chunk, unfilteredChunkIndex, useEnabledMask, chunkEnabledMask);
            }

            // Token: 0x04008FFA RID: 36858
            [ReadOnly]
            public EntityTypeHandle m_EntityType;

            // Token: 0x04008FFB RID: 36859
            [ReadOnly]
            public BufferTypeHandle<ConnectedBuilding> m_ConnectedBuildingType;

            // Token: 0x04008FFC RID: 36860
            [ReadOnly]
            public ComponentTypeHandle<Edge> m_EdgeType;

            // Token: 0x04008FFD RID: 36861
            [ReadOnly]
            public ComponentTypeHandle<Curve> m_CurveType;

            // Token: 0x04008FFE RID: 36862
            [NativeDisableParallelForRestriction]
            public ComponentLookup<LandValue> m_LandValues;

            // Token: 0x04008FFF RID: 36863
            [ReadOnly]
            public BufferLookup<Renter> m_RenterBuffers;

            [ReadOnly]
            public ComponentLookup<Game.Objects.Transform> m_Transforms;

            // Token: 0x04009000 RID: 36864
            [ReadOnly]
            public ComponentLookup<PropertyRenter> m_PropertyRenters;

            // Token: 0x04009001 RID: 36865
            [ReadOnly]
            public ComponentLookup<PrefabRef> m_Prefabs;

            // Token: 0x04009002 RID: 36866
            [ReadOnly]
            public ComponentLookup<BuildingData> m_BuildingDatas;

            // Token: 0x04009003 RID: 36867
            [ReadOnly]
            public ComponentLookup<Abandoned> m_Abandoneds;

            // Token: 0x04009004 RID: 36868
            [ReadOnly]
            public ComponentLookup<Destroyed> m_Destroyeds;

            // Token: 0x04009005 RID: 36869
            [ReadOnly]
            public ComponentLookup<ConsumptionData> m_ConsumptionDatas;

            // Token: 0x04009006 RID: 36870
            [ReadOnly]
            public ComponentLookup<BuildingPropertyData> m_PropertyDatas;

            // Token: 0x04009007 RID: 36871
            [ReadOnly]
            public ComponentLookup<Household> m_Households;

            // Token: 0x04009008 RID: 36872
            [ReadOnly]
            public ComponentLookup<Placeholder> m_Placeholders;

            // Token: 0x04009009 RID: 36873
            [ReadOnly]
            public ComponentLookup<Attached> m_Attached;

            // Token: 0x0400900A RID: 36874
            [ReadOnly]
            public BufferLookup<Game.Areas.SubArea> m_SubAreas;

            // Token: 0x0400900B RID: 36875
            [ReadOnly]
            public ComponentLookup<Game.Areas.Lot> m_Lots;

            // Token: 0x0400900C RID: 36876
            [ReadOnly]
            public ComponentLookup<Geometry> m_Geometries;

            [ReadOnly]
            public NativeArray<GroundPollution> m_PollutionMap;

            // Token: 0x0400910D RID: 37133
            [ReadOnly]
            public PollutionParameterData m_PollutionParameters;
        }

        // Token: 0x0200133A RID: 4922
        [BurstCompile]
        private struct NodeUpdateJob : IJobChunk
        {
            // Token: 0x06005579 RID: 21881 RVA: 0x003B1AF0 File Offset: 0x003AFCF0
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> nativeArray = chunk.GetNativeArray(this.m_EntityType);
                NativeArray<Game.Net.Node> nativeArray2 = chunk.GetNativeArray<Game.Net.Node>(ref this.m_NodeType);
                BufferAccessor<ConnectedEdge> bufferAccessor = chunk.GetBufferAccessor<ConnectedEdge>(ref this.m_ConnectedEdgeType);
                for (int i = 0; i < nativeArray2.Length; i++)
                {
                    Entity entity = nativeArray[i];
                    float num = 0f;
                    float num2 = 0f;
                    DynamicBuffer<ConnectedEdge> dynamicBuffer = bufferAccessor[i];
                    for (int j = 0; j < dynamicBuffer.Length; j++)
                    {
                        Entity edge = dynamicBuffer[j].m_Edge;
                        if (this.m_LandValues.HasComponent(edge))
                        {
                            float landValue = this.m_LandValues[edge].m_LandValue;
                            float num3 = this.m_LandValues[edge].m_Weight;
                            if (this.m_Curves.HasComponent(edge))
                            {
                                float distanceFade = LandValueSystem_Custom.GetDistanceFade(this.m_Curves[edge].m_Length);
                                num3 *= distanceFade;
                            }
                            num += landValue * num3;
                            num2 += num3;
                        }
                    }
                    if (num2 != 0f)
                    {
                        num /= num2;
                        LandValue landValue2 = this.m_LandValues[entity];
                        landValue2.m_LandValue = math.lerp(landValue2.m_LandValue, num, 0.05f);
                        landValue2.m_Weight = math.max(1f, math.lerp(landValue2.m_Weight, num2 / (float)dynamicBuffer.Length, 0.05f));
                        this.m_LandValues[entity] = landValue2;
                    }
                    if (enable_LandValueCap)
                    {
                        LandValue landValue3 = this.m_LandValues[entity];
                        landValue3.m_LandValue = math.min(landValue3.m_LandValue, m_LandValueCap);
                        this.m_LandValues[entity] = landValue3;
                    }
                }
            }

            // Token: 0x0600557A RID: 21882 RVA: 0x0008E42B File Offset: 0x0008C62B
            void IJobChunk.Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                this.Execute(chunk, unfilteredChunkIndex, useEnabledMask, chunkEnabledMask);
            }

            // Token: 0x0400900D RID: 36877
            [ReadOnly]
            public EntityTypeHandle m_EntityType;

            // Token: 0x0400900E RID: 36878
            [ReadOnly]
            public ComponentTypeHandle<Game.Net.Node> m_NodeType;

            // Token: 0x0400900F RID: 36879
            [ReadOnly]
            public BufferTypeHandle<ConnectedEdge> m_ConnectedEdgeType;

            // Token: 0x04009010 RID: 36880
            [NativeDisableParallelForRestriction]
            public ComponentLookup<LandValue> m_LandValues;

            // Token: 0x04009011 RID: 36881
            [ReadOnly]
            public ComponentLookup<Curve> m_Curves;
        }

        // Token: 0x0200133B RID: 4923
        private struct TypeHandle
        {
            // Token: 0x0600557B RID: 21883 RVA: 0x003B1C78 File Offset: 0x003AFE78
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void __AssignHandles(ref SystemState state)
            {
                this.__Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
                this.__Game_Net_Edge_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Edge>(true);
                this.__Game_Net_Curve_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Curve>(true);
                this.__Game_Buildings_ConnectedBuilding_RO_BufferTypeHandle = state.GetBufferTypeHandle<ConnectedBuilding>(true);
                this.__Game_Prefabs_BuildingData_RO_ComponentLookup = state.GetComponentLookup<BuildingData>(true);
                this.__Game_Objects_Transform_RO_ComponentLookup = state.GetComponentLookup<Game.Objects.Transform>(true);
                this.__Game_Net_LandValue_RW_ComponentLookup = state.GetComponentLookup<LandValue>(false);
                this.__Game_Prefabs_PrefabRef_RO_ComponentLookup = state.GetComponentLookup<PrefabRef>(true);
                this.__Game_Buildings_PropertyRenter_RO_ComponentLookup = state.GetComponentLookup<PropertyRenter>(true);
                this.__Game_Buildings_Renter_RO_BufferLookup = state.GetBufferLookup<Renter>(true);
                this.__Game_Buildings_Abandoned_RO_ComponentLookup = state.GetComponentLookup<Abandoned>(true);
                this.__Game_Common_Destroyed_RO_ComponentLookup = state.GetComponentLookup<Destroyed>(true);
                this.__Game_Prefabs_ConsumptionData_RO_ComponentLookup = state.GetComponentLookup<ConsumptionData>(true);
                this.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup = state.GetComponentLookup<BuildingPropertyData>(true);
                this.__Game_Citizens_Household_RO_ComponentLookup = state.GetComponentLookup<Household>(true);
                this.__Game_Objects_Placeholder_RO_ComponentLookup = state.GetComponentLookup<Placeholder>(true);
                this.__Game_Objects_Attached_RO_ComponentLookup = state.GetComponentLookup<Attached>(true);
                this.__Game_Areas_SubArea_RO_BufferLookup = state.GetBufferLookup<Game.Areas.SubArea>(true);
                this.__Game_Areas_Lot_RO_ComponentLookup = state.GetComponentLookup<Game.Areas.Lot>(true);
                this.__Game_Areas_Geometry_RO_ComponentLookup = state.GetComponentLookup<Geometry>(true);
                this.__Game_Net_Node_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Net.Node>(true);
                this.__Game_Net_ConnectedEdge_RO_BufferTypeHandle = state.GetBufferTypeHandle<ConnectedEdge>(true);
                this.__Game_Net_Curve_RO_ComponentLookup = state.GetComponentLookup<Curve>(true);
            }

            // Token: 0x04009012 RID: 36882
            [ReadOnly]
            public EntityTypeHandle __Unity_Entities_Entity_TypeHandle;

            // Token: 0x04009013 RID: 36883
            [ReadOnly]
            public ComponentTypeHandle<Edge> __Game_Net_Edge_RO_ComponentTypeHandle;

            // Token: 0x04009014 RID: 36884
            [ReadOnly]
            public ComponentTypeHandle<Curve> __Game_Net_Curve_RO_ComponentTypeHandle;

            // Token: 0x04009015 RID: 36885
            [ReadOnly]
            public BufferTypeHandle<ConnectedBuilding> __Game_Buildings_ConnectedBuilding_RO_BufferTypeHandle;

            // Token: 0x04009016 RID: 36886
            [ReadOnly]
            public ComponentLookup<BuildingData> __Game_Prefabs_BuildingData_RO_ComponentLookup;

            [ReadOnly]
            public ComponentLookup<Game.Objects.Transform> __Game_Objects_Transform_RO_ComponentLookup;

            // Token: 0x04009017 RID: 36887
            public ComponentLookup<LandValue> __Game_Net_LandValue_RW_ComponentLookup;

            // Token: 0x04009018 RID: 36888
            [ReadOnly]
            public ComponentLookup<PrefabRef> __Game_Prefabs_PrefabRef_RO_ComponentLookup;

            // Token: 0x04009019 RID: 36889
            [ReadOnly]
            public ComponentLookup<PropertyRenter> __Game_Buildings_PropertyRenter_RO_ComponentLookup;

            // Token: 0x0400901A RID: 36890
            [ReadOnly]
            public BufferLookup<Renter> __Game_Buildings_Renter_RO_BufferLookup;

            // Token: 0x0400901B RID: 36891
            [ReadOnly]
            public ComponentLookup<Abandoned> __Game_Buildings_Abandoned_RO_ComponentLookup;

            // Token: 0x0400901C RID: 36892
            [ReadOnly]
            public ComponentLookup<Destroyed> __Game_Common_Destroyed_RO_ComponentLookup;

            // Token: 0x0400901D RID: 36893
            [ReadOnly]
            public ComponentLookup<ConsumptionData> __Game_Prefabs_ConsumptionData_RO_ComponentLookup;

            // Token: 0x0400901E RID: 36894
            [ReadOnly]
            public ComponentLookup<BuildingPropertyData> __Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;

            // Token: 0x0400901F RID: 36895
            [ReadOnly]
            public ComponentLookup<Household> __Game_Citizens_Household_RO_ComponentLookup;

            // Token: 0x04009020 RID: 36896
            [ReadOnly]
            public ComponentLookup<Placeholder> __Game_Objects_Placeholder_RO_ComponentLookup;

            // Token: 0x04009021 RID: 36897
            [ReadOnly]
            public ComponentLookup<Attached> __Game_Objects_Attached_RO_ComponentLookup;

            // Token: 0x04009022 RID: 36898
            [ReadOnly]
            public BufferLookup<Game.Areas.SubArea> __Game_Areas_SubArea_RO_BufferLookup;

            // Token: 0x04009023 RID: 36899
            [ReadOnly]
            public ComponentLookup<Game.Areas.Lot> __Game_Areas_Lot_RO_ComponentLookup;

            // Token: 0x04009024 RID: 36900
            [ReadOnly]
            public ComponentLookup<Geometry> __Game_Areas_Geometry_RO_ComponentLookup;

            // Token: 0x04009025 RID: 36901
            [ReadOnly]
            public ComponentTypeHandle<Game.Net.Node> __Game_Net_Node_RO_ComponentTypeHandle;

            // Token: 0x04009026 RID: 36902
            [ReadOnly]
            public BufferTypeHandle<ConnectedEdge> __Game_Net_ConnectedEdge_RO_BufferTypeHandle;

            // Token: 0x04009027 RID: 36903
            [ReadOnly]
            public ComponentLookup<Curve> __Game_Net_Curve_RO_ComponentLookup;
        }
    }
}
