using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// Generic Job showing some of the more common Component Data Types used.
/// </summary>
[BurstCompile]
public struct GenericJob : IJobForEach<Translation, PositionComponent, LocalToWorld, Rotation>
{

    public void Execute(ref Translation translation, ref PositionComponent positionComponent, ref LocalToWorld localToWorld, ref Rotation rotation)
    {
        translation.Value = positionComponent.position;
    }


    //An older version just running with IJobParallelFor, and basically you can specify the arrays how many times it will run for, it will then split the job across many threads.

    //public int counter;
    //NativeArray<PositionComponent> _positionComponents;
    //NativeArray<Translation> _translations;
    //NativeArray<LocalToWorld> _localToWorlds;
    //NativeArray<Entity> _entities;

    //public LocalToWorLocalToWorldPositionComponentJob(NativeArray<Entity> entities, NativeArray<PositionComponent> positionComponents, NativeArray<Translation> translations, NativeArray<LocalToWorld> localToWorlds)
    //{
    //    _entities = entities;
    //    _translations = translations;
    //    _positionComponents = positionComponents;
    //    _localToWorlds = localToWorlds;
    //    counter = 0;
    //}

    //public void Execute(int index)
    //{
    //    var translation = _translations[index];
    //    translation.Value = _positionComponents[index].position;

    //    //Remember to write stuff back to the arrays
    //    {
    //        _translations[index] = translation;
    //    }
    //    counter++;
    //}
}
