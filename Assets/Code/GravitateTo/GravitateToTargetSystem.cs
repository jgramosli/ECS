using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class GravitateToTargetSystem : JobComponentSystem
{
    public System.Action<float3> TargetHit;
    JobHandle _jobHandle;
    EntityManager _entityManager;
    float _explosionMultiplier = 20;
    float _movementMultiplier = 1;
    float _explosionDestructionDistance = 5;
    float _explosionMaxDistance = 40;
    BuildPhysicsWorld _physWorld;
    Spawner _spawner;

    protected override void OnCreate()
    {
        base.OnCreate();
        Debug.Log("Created GravitateToTargetSystem");
        _entityManager = World.Active.EntityManager;
        _physWorld = World.Active.GetOrCreateSystem<BuildPhysicsWorld>();
        _spawner = GameObject.FindObjectOfType<Spawner>();
        Enabled = false;
    }

    public void SetPhysicsScales(float explosionMultiplier, float movementMultiplier, float explosionDestructionDistance, float explosionMaxDistance)
    {
        _explosionMultiplier = explosionMultiplier;
        _movementMultiplier = movementMultiplier;
        _explosionMaxDistance = explosionMaxDistance;
        _explosionDestructionDistance = explosionDestructionDistance;
    }

    public Entity RaycastEntity(float3 RayFrom, float3 Direction)
    {
        var physicsWorldSystem = World.Active.GetExistingSystem<BuildPhysicsWorld>();
        var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
        RaycastInput input = new RaycastInput()
        {
            Ray = new Unity.Physics.Ray()
            {
                Origin = RayFrom,
                Direction = Direction
            },
            Filter = new CollisionFilter()
            {
                CategoryBits = ~0u, // all 1s, so all layers, collide with everything 
                MaskBits = ~0u,
                GroupIndex = 0
            }
        };

        Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();
        bool haveHit = collisionWorld.CastRay(input, out hit);
        if (haveHit)
        {
            // see hit.Position 
            // see hit.SurfaceNormal
            Entity e = physicsWorldSystem.PhysicsWorld.Bodies[hit.RigidBodyIndex].Entity;
            return e;
        }
        return Entity.Null;
    }

    public float3 RaycastHitLocation(float3 RayFrom, float3 Direction)
    {
        var physicsWorldSystem = World.Active.GetExistingSystem<BuildPhysicsWorld>();
        var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
        RaycastInput input = new RaycastInput()
        {
            Ray = new Unity.Physics.Ray()
            {
                Origin = RayFrom,
                Direction = Direction
            },
            Filter = new CollisionFilter()
            {
                CategoryBits = ~0u, // all 1s, so all layers, collide with everything 
                MaskBits = ~0u,
                GroupIndex = 0
            }
        };

        Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();
        bool haveHit = collisionWorld.CastRay(input, out hit);
        if (haveHit)
        {
            return hit.Position;
        }
        return float3.zero;
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
            var ray = Camera.main.ScreenPointToRay(screenPoint);
            Debug.DrawRay(ray.origin, ray.direction * 2000, Color.red,10);

            var pos = RaycastHitLocation(ray.origin, ray.direction * 2000);
            if(pos.x == 0 && pos.y == 0 && pos.z == 0)
            {
                Debug.Log("Didnt get a hit...");
                return inputDeps;
            }
            else
            {
                TargetHit?.Invoke(pos);
            }

            var query = new EntityQueryDesc() { All = new ComponentType[] { typeof(PositionComponent), typeof(Translation), typeof(PhysicsVelocity) } };
            var response = GetEntityQuery(query);
            var entities = response.ToEntityArray(Unity.Collections.Allocator.TempJob);
            var translations = response.ToComponentDataArray<Translation>(Unity.Collections.Allocator.TempJob);
            var velocities = response.ToComponentDataArray<PhysicsVelocity>(Unity.Collections.Allocator.TempJob);

            var removalCount = 0;
            for (int i = 0; i < translations.Length; i++)
            {
                var physicsVelocity = velocities[i];
                var distance = math.distance(pos, translations[i].Value);
                //Only effect those within a certain distance
                if (distance > _explosionMaxDistance)
                    continue;

                if(distance < _explosionDestructionDistance)
                {
                    removalCount++;
                    //We've got a hit, deactivate it
                    _entityManager.SetComponentData(entities[i], new PositionComponent() { active = false });
                    _spawner.counter--;
                }
                var linearVelocity = math.normalize(translations[i].Value - pos) * distance * _explosionMultiplier;
                physicsVelocity.Linear = linearVelocity;
                _entityManager.SetComponentData(entities[i], physicsVelocity);
            }

            Debug.Log("Removed " + removalCount + " Entities");
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
                screenPoint.z = 150.0f; //distance of the plane from the camera
                var pos = Camera.main.ScreenToWorldPoint(screenPoint);

                for (int i = 0; i < posComps.Length; i++)
                {
                    posComps[i] = new PositionComponent() { position = pos, origionalPosition = pos, active = true };
                    _entityManager.SetComponentData(entities[i], posComps[i]);
                }
                Debug.Log("Processed " + posComps.Length + " Position Components set to " + pos);

                posComps.Dispose();
                entities.Dispose();
            }
        }

        var newJob = new GravitateToTargetJob(Time.time, _movementMultiplier);

        _jobHandle = newJob.Schedule(this, inputDeps);

        _jobHandle.Complete();

        return _jobHandle;
    }
}


/*
 *     public EntityQuery m_Follower;
    EntityCommandBufferSystem m_ECB;
    public PointDistanceInput pointDistanceInput;
    public RaycastInput raycastInput;
    public BuildPhysicsWorld phyWorld;
    EntityManager em;
    public NativeList<DistanceHit> DistanceHits;
    public ComponentDataFromEntity<Tag> tag;
    protected override void OnCreateManager()
    {
 
        m_ECB = World.GetOrCreateSystem<EntityCommandBufferSystem>();
        em = World.EntityManager;
        m_Follower = GetEntityQuery(
           typeof(FollowNear)
       );
        phyWorld = World.Active.GetOrCreateSystem<BuildPhysicsWorld>();
        DistanceHits = new NativeList<DistanceHit>(Allocator.Persistent);
 
    }
    unsafe protected override void OnUpdate()
    {
        DistanceHits.Clear();
        Entities.ForEach((Entity entity,
                     ref FollowNear followNear,
                     ref CircleCollision collision,
                     ref Translation translation,
                     ref Rotation rotation                  
                     )
                     =>
      {
          pointDistanceInput = new PointDistanceInput
          {
              Position = translation.Value,
              MaxDistance = 10,
              Filter = GameSettings._instance.filter((1 << 1), 1 << 0, 0)
          };
          bool isTargetInDistance = phyWorld.PhysicsWorld.CalculateDistance(pointDistanceInput, out DistanceHit distHit);  
 
          raycastInput = new RaycastInput
          {
              Ray = new Ray(translation.Value, -distHit.SurfaceNormal),
 
              Filter = GameSettings._instance.filter(GameSettings._instance.layer_enemy,
                                                     GameSettings._instance.layer_player | GameSettings._instance.layer_structure,
                                                     0)
          };
          bool isTargetInSight = phyWorld.PhysicsWorld.CastRay(raycastInput, out RaycastHit sightHit);
          Entity e_sight = phyWorld.PhysicsWorld.Bodies[sightHit.RigidBodyIndex].Entity;
 
      bool isPlayer =
          phyWorld.PhysicsWorld.GetCollisionFilter(sightHit.RigidBodyIndex).MaskBits == GameSettings._instance.layer_player ? true : false;
 
          if (isTargetInDistance)
          {
              DistanceHits.Add(distHit);
 
              if (isPlayer)
              {
                  Entity e_dist = phyWorld.PhysicsWorld.Bodies[distHit.RigidBodyIndex].Entity;
                  translation = new Translation { Value = translation.Value - (distHit.SurfaceNormal * 0.5f * UnityEngine.Time.deltaTime) };              
 
              }
          }
 
      });
    }
*/