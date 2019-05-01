using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace Assets.Code
{
    public class PositionWiggleSystem : JobComponentSystem
    {
        JobHandle _jobHandle, _positionUpdateJob;

        protected override void OnCreate()
        {
            base.OnCreate();
            Enabled = false;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (!_jobHandle.IsCompleted)
            {
                Debug.Log("PositionWiggleSystem Job IsCompleted:" + _jobHandle.IsCompleted);

                return _jobHandle;
            }

            var newJob = new PositionWiggleJob(Time.time, 50, 5f);

            _jobHandle = newJob.Schedule(this, inputDeps);

            //_jobHandle.Complete();
            
            //Debug.Log("PositionUpdateSystem Update Done");
            return _jobHandle;
        }
    }
}
