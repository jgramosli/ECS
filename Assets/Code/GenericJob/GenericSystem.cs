using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Unity.Burst;

namespace Assets.Code
{
    public class GenericSystem : JobComponentSystem
    {
        JobHandle _jobHandle;

        protected override void OnCreate()
        {
            Enabled = false;
            //init something, happens once each life
            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            //cleanup something, happens once each life
            base.OnDestroy();
        }

        //With the Start and Stop Running you can manage Systems when they get Enabled set to false/true.
        protected override void OnStartRunning()
        {
            //Enabled has been set to true, init something
            base.OnStartRunning();
        }

        protected override void OnStopRunning()
        {
            //Enabled has been set to false, cleanup something
            base.OnStopRunning();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (!_jobHandle.IsCompleted)
            {
                Debug.Log("ECSBlittablesSystem Job IsCompleted:" + _jobHandle.IsCompleted);
                return _jobHandle;
            }

            //Use the below code for manually querying through the Entity Manager
            //var entityManager = World.Active.EntityManager;

            //var query = new EntityQueryDesc
            //{
            //    All = new ComponentType[] { typeof(PositionComponent), typeof(Translation), typeof(LocalToWorld) },
            //};

            //EntityQuery m_Group = GetEntityQuery(query);
            //m_Group.ToComponentArray<Translation>();

            var newJob = new GenericJob();

            _jobHandle = newJob.Schedule(this, inputDeps);

            //_jobHandle.Complete();

            return _jobHandle;
        }

    }
}