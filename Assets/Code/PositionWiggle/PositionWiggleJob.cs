using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Entities;
using System;
using Unity.Mathematics;

/// <summary>
/// When running, it makes entities with Position Component wiggle a bit
/// </summary>
[BurstCompile]
public struct PositionWiggleJob : IJobForEachWithEntity<PositionComponent>
{
    float _movementMultiplier;
    float fakeTimer;
    Unity.Mathematics.Random _rand;

    public PositionWiggleJob(float timer, int count, float movementMultiplier)
    {
        _rand = new Unity.Mathematics.Random();
        _rand.InitState();
        _movementMultiplier = movementMultiplier;
        fakeTimer = timer;
    }

    public void Execute(Entity entity, int index, ref PositionComponent positionComponent)
    {
        fakeTimer += 1f; //Increment this value so we can get some nice wave movement, can't access any timers so this will have to do.
        var movement = Mathf.PingPong(fakeTimer, 3f) / 3f * _movementMultiplier - (_movementMultiplier / 2);
        movement *= 3;//give it some movement boost
        positionComponent.position = new float3(positionComponent.origionalPosition.x + movement, positionComponent.origionalPosition.y + movement, positionComponent.origionalPosition.z + movement);
    }
}
