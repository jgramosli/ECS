using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

//public class GravitateToTargetSystem : JobComponentSystem
//{
//    protected override JobHandle OnUpdate(JobHandle inputDeps)
//    {
//        var newJob = new GravitateToTargetJob();

//        var jobHandle = newJob.Schedule(this, inputDeps);

//        jobHandle.Complete();

//        return jobHandle;
//    }
//}
