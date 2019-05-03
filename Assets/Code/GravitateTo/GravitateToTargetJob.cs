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
[BurstCompile]
public struct GravitateToTargetJob : IJobForEach<Translation, PositionComponent, PhysicsVelocity, PhysicsCollider>
{
    float _fakeTimer;
    float _multiplier;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timer"></param>
    /// <param name="multiplier">0.1, increases movement of speed and acceleration.</param>
    public GravitateToTargetJob(float timer, float multiplier)
    {
        _fakeTimer = timer;
        _multiplier = multiplier;
    }

    public void Execute(ref Translation translation, ref PositionComponent positionComponent, ref PhysicsVelocity physicsVelocity, ref PhysicsCollider physicsCollider)
    {
        if(positionComponent.active == false)
        {
            return;
        }

        var direction = (positionComponent.position - translation.Value);
        float internalDistanceMultiplier;
        var distance = math.distance(positionComponent.position, translation.Value);

        if (distance > 20f)
        {
            internalDistanceMultiplier = 100f;
        }
        else
        {
            internalDistanceMultiplier = distance * 5;
        }
        //Debug.Log("multiplier:" + multiplier);

        float3 directionNormalized = math.normalize(direction) * internalDistanceMultiplier * _multiplier;

        physicsVelocity.Linear = math.lerp(physicsVelocity.Linear,directionNormalized,0.3f * _multiplier);
        //Keep the direction of the velocity towards the position Component target

        //Debug.Log("physicsVelocity.Linear:" + physicsVelocity.Linear);
    }
}
