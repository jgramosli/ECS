using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Burst;

//[BurstCompile]
//public struct GravitateToTargetJob : IJobForEach<Translation, PositionComponent, PhysicsVelocity, PhysicsCollider>
//{

//    public void Execute(ref Translation translation, ref PositionComponent positionComponent, ref PhysicsVelocity physicsVelocity, ref PhysicsCollider physicsCollider)
//    {
//        var DirectionNormalized = (translation.Value - positionComponent.position) * 0.3f;
//        physicsVelocity.Linear = DirectionNormalized;
//        //Keep the direction of the velocity towards the position Component
//    }
//}
