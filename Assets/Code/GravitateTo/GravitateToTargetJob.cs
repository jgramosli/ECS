using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;

/// <summary>
/// When activated will make this archetype gravitate slowly to the PositionComponent target using PhysicsVelocity.Linear to propel it
/// </summary>
//[BurstCompile]
public struct GravitateToTargetJob : IJobForEach<Translation, PositionComponent, PhysicsVelocity, PhysicsCollider>
{
    float fakeTimer;
    public GravitateToTargetJob(float timer)
    {
        fakeTimer = timer;
    }

    public void Execute(ref Translation translation, ref PositionComponent positionComponent, ref PhysicsVelocity physicsVelocity, ref PhysicsCollider physicsCollider)
    {
        var direction = (positionComponent.position - translation.Value);
        var multiplier = 10f;
        var distance = math.distance(positionComponent.position, translation.Value);

        if (distance > 30f)
        {
            multiplier = 100f;
        }
        else
        {
            multiplier = distance * 5;
        }
        //Debug.Log("multiplier:" + multiplier);

        float3 directionNormalized = math.normalize(direction) * multiplier;

        physicsVelocity.Linear = math.lerp(physicsVelocity.Linear,directionNormalized,0.3f);
        //Keep the direction of the velocity towards the position Component target

        //Debug.Log("physicsVelocity.Linear:" + physicsVelocity.Linear);
    }
}
