using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Entities;
using System;

[BurstCompile]
public struct PositionComponentJob : IJobForEachWithEntity<PositionComponent>
{
    float fakeTimer;

    public PositionComponentJob(float timer, int count)
    {
        fakeTimer = timer;
    }

    public void Execute(Entity entity, int index, ref PositionComponent positionComponent)
    {
        fakeTimer += 0.05f; //Increment this value so we can get some nice wave movement, can't access any timers so this will have to do.
        var movement = Mathf.PingPong(fakeTimer, 0.7f) - 0.35f;
        positionComponent.position = new Vector3(positionComponent.position.x + movement, positionComponent.position.y + movement, positionComponent.position.z + movement);
    }
}
