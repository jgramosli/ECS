using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public class GravitateToTargetSystem : JobComponentSystem
{
    JobHandle _jobHandle;
    EntityManager _entityManager;

    protected override void OnCreate()
    {
        base.OnCreate();
        Debug.Log("Created GravitateToTargetSystem");
        _entityManager = World.Active.EntityManager;
        Enabled = false;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {

        if (!_jobHandle.IsCompleted)
        {
            Debug.Log("GravitateToTargetSystem Job Is Still IsCompleted:" + _jobHandle.IsCompleted);

            return _jobHandle;
        }

        //Add a little explosion to all the entities
        if (Input.GetMouseButtonDown(1))
        {
            var screenPoint = Input.mousePosition;
            screenPoint.z = 75.0f; //distance of the plane from the camera
            var posVector = Camera.main.ScreenToWorldPoint(screenPoint);
            var pos = new float3(posVector.x,posVector.y, posVector.z);

            var query = new EntityQueryDesc() { All = new ComponentType[] { typeof(PositionComponent), typeof(Translation), typeof(PhysicsVelocity) } };
            var response = GetEntityQuery(query);
            var entities = response.ToEntityArray(Unity.Collections.Allocator.TempJob);
            var translations = response.ToComponentDataArray<Translation>(Unity.Collections.Allocator.TempJob);
            var velocities = response.ToComponentDataArray<PhysicsVelocity>(Unity.Collections.Allocator.TempJob);

            for (int i = 0; i < translations.Length; i++)
            {
                var physicsVelocity = velocities[i];
                var distance = math.distance(pos, translations[i].Value);
                //Only effect those within a certain distance
                if (distance > 40)
                    continue;

                var linearVelocity = math.normalize(translations[i].Value - pos) * distance * 10.0f;
                physicsVelocity.Linear = linearVelocity;
                _entityManager.SetComponentData(entities[i], physicsVelocity);
            }

            translations.Dispose();
            velocities.Dispose();
            entities.Dispose();
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (Camera.main.ScreenToViewportPoint(Input.mousePosition).y > 0.85)
            {
                //Debug.Log("Too high to register");
            }
            else
            {
                var query = new EntityQueryDesc() { All = new ComponentType[] { typeof(PositionComponent) } };
                var response = GetEntityQuery(query);
                var entities = response.ToEntityArray(Unity.Collections.Allocator.TempJob);
                var posComps = response.ToComponentDataArray<PositionComponent>(Unity.Collections.Allocator.TempJob);

                var screenPoint = Input.mousePosition;
                screenPoint.z = 75.0f; //distance of the plane from the camera
                var pos = Camera.main.ScreenToWorldPoint(screenPoint);

                for (int i = 0; i < posComps.Length; i++)
                {
                    posComps[i] = new PositionComponent() { position = pos, origionalPosition = pos };
                    _entityManager.SetComponentData(entities[i], posComps[i]);
                }
                Debug.Log("Processed " + posComps.Length + " Position Components set to " + pos);

                posComps.Dispose();
                entities.Dispose();
            }
        }

        var newJob = new GravitateToTargetJob();

        _jobHandle = newJob.Schedule(this, inputDeps);

        //_jobHandle.Complete();

        return _jobHandle;
    }
}
