using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

namespace Assets.Code
{
    public class PositionComponentSystem : JobComponentSystem
    {
        JobHandle _jobHandle, _positionUpdateJob;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (!_jobHandle.IsCompleted)
            {
                Debug.Log("PositionComponentSystem Job IsCompleted:" + _jobHandle.IsCompleted);

                return _jobHandle;
            }

            var newJob = new PositionComponentJob(Time.time, 50);

            _jobHandle = newJob.Schedule(this, inputDeps);

            //_jobHandle.Complete();
            
            //Debug.Log("PositionUpdateSystem Update Done");
            return _jobHandle;
        }
    }
}
