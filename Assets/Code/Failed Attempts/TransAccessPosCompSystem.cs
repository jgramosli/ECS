using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;


namespace Assets.Code
{
    public class TransAccessPosCompSystem : JobComponentSystem
    {
        JobHandle _jobHandle;

        protected override void OnCreate()
        {
            base.OnCreate();
            Enabled = false;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (!_jobHandle.IsCompleted)
            {
                Debug.Log("TransAccessPosCompSystem Job IsCompleted:" + _jobHandle.IsCompleted);
                return _jobHandle;
            }

            var entityManager = World.Active.EntityManager;

            var query = new EntityQueryDesc
            { 
                //None = new ComponentType[] { typeof(Frozen) },
                All = new ComponentType[] { typeof(PositionComponent), typeof(LocalToWorld) },
            };

            EntityQuery m_Group = GetEntityQuery(query);

            var positionComponents = m_Group.ToComponentDataArray<PositionComponent>(Allocator.TempJob);
            Debug.Log("Found m_Group.ToComponentDataArray<PositionComponent>" + positionComponents.Length);
            var transformAccesses = m_Group.GetTransformAccessArray();
            var newJob = new TransAccessPosCompJob(positionComponents);
            
            _jobHandle = newJob.Schedule(transformAccesses);

            _jobHandle.Complete();

            Debug.Log("TransAccessPosCompJob count:" + newJob.counter);
            return _jobHandle;
        }
    }
}