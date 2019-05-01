using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Unity.Burst;

namespace Assets.Code
{

    public class ECSBlittablesSystem : JobComponentSystem
    {
        JobHandle _jobHandle;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            Enabled = false;
            return inputDeps;

            if (!_jobHandle.IsCompleted)
            {
                Debug.Log("ECSBlittablesSystem Job IsCompleted:" + _jobHandle.IsCompleted);
                return _jobHandle;
            }

            //Use the below code
            //var entityManager = World.Active.EntityManager;

            //var query = new EntityQueryDesc
            //{
            //    All = new ComponentType[] { typeof(PositionComponent), typeof(Translation), typeof(LocalToWorld) },
            //};

            //EntityQuery m_Group = GetEntityQuery(query);
            //m_Group.ToComponentArray<Translation>(); //EG

            var newJob = new ECSBlittablesBlittablesJob();

            _jobHandle = newJob.Schedule(this, inputDeps);

            //_jobHandle.Complete();

            return _jobHandle;
        }

    }
}