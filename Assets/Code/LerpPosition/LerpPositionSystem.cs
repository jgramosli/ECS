using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace Assets.Code
{
    /// <summary>
    /// Lerps the position to the target using the LerpPositionSystem
    /// Also note it uses the [UpdateAfter(typeof(GenericSystem))]  Attribute, this tells the Job System to run this system after Generic System every update
    /// </summary>
    [UpdateAfter(typeof(GenericSystem))]
    public class LerpPositionSystem : JobComponentSystem
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
                Debug.Log("LerpPositionSystem Job IsCompleted:" + _jobHandle.IsCompleted);

                return _jobHandle;
            }

            var newJob = new LerpPositionJob(0.2f);

            _jobHandle = newJob.Schedule(this, inputDeps);

            return _jobHandle;
        }
    }
}
