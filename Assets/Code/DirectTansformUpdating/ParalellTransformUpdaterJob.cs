using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.Jobs;

[BurstCompile]
public struct ParalellTransformUpdaterJob : IJobParallelForTransform
{
    float fakeTimer;

    public ParalellTransformUpdaterJob(float timer)
    {
        fakeTimer = timer;
    }

    public void Execute(int index, TransformAccess transform)
    {
        fakeTimer += 0.05f; //Increment this value so we can get some nice wave movement, can't access any timers so this will have to do.
        var movement = Mathf.PingPong(fakeTimer, 0.7f) - 0.35f;
        transform.localPosition = new Vector3(transform.localPosition.x + movement, transform.localPosition.y + movement, transform.localPosition.z + movement);
    }
}
