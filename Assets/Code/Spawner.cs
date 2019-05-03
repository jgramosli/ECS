using Assets.Code;
using Assets.Code.VerticesCopy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

public class Spawner : UnityEngine.MonoBehaviour
{
    public UnityEngine.Mesh ButterFlyMesh;
    public UnityEngine.Material ButterFlyMaterial;
    public UnityEngine.Mesh FloorMesh;
    public UnityEngine.Material FloorMaterial;
    public UnityEngine.Mesh MeshToDrawWithItems;
    public GameObject[] ECSItemsToSpawn;
    public GameObject[] hitsLarge;
    public GameObject[] hitsSmall;
    public UnityEngine.SkinnedMeshRenderer SkinToDrawWithItems;
    public Transform cameraTransform, cameraTransformSkinned;
    public int counter = 0;
    public int count = 100;

    StringBuilder _logs = new StringBuilder();
    EntityManager _entityManager;
    World _initialWorld;
    GravitateToTargetSystem _gravSystem;
    bool zoomedForVertices = false;
    TransformAccessArray _goTransforms;
    Unity.Mathematics.Random _rand;

    public float DefaultScale = 3f, SkinRendererScale = 0.75f;

    public void Start()
    {
        _entityManager = World.Active.EntityManager;
        _gravSystem = World.Active.GetOrCreateSystem<GravitateToTargetSystem>();
        _gravSystem.TargetHit += t => TargetHit(t);
        _initialWorld = World.Active;

        _rand = new Unity.Mathematics.Random();
        _rand.InitState();
        SetupWorld();

        //_entityManager.AddComponentData(butterFlyEntity, PhysicsStep.Default);//new PhysicsStep() { SimulationType = SimulationType.UnityPhysics, Gravity = 0.6f, SolverIterationCount = 2, ThreadCountHint = 16 });

        Debug.Log("Finished created " + count + " entities" +"\nLogs:\n" + _logs.ToString());
    }

    void TargetHit(float3 position)
    {
        if (zoomedForVertices)
            GameObject.Instantiate(hitsSmall[_rand.NextInt(0, hitsSmall.Length)], position, quaternion.identity);
        else
            GameObject.Instantiate(hitsLarge[_rand.NextInt(0, hitsLarge.Length)],position, quaternion.identity);
    }

    void SpawnViaPrebuiltGameObjects(float scale, bool removePhysics = false)
    {
        GameObject gameObject;

        if (ECSItemsToSpawn.Length == 0)
        {
            gameObject = Resources.Load<UnityEngine.GameObject>("ConvertGOToEntity");
        }
        else
        {
            Debug.Log("Using ECSItemToSpawn");
            gameObject = ECSItemsToSpawn[_rand.NextInt(0, ECSItemsToSpawn.Length)];
        }

        gameObject.transform.localScale = new Vector3(scale, scale, scale);
        var sourceEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(gameObject, _initialWorld);

        for (int i = 0; i < count; i++)
        {
            var entity = _entityManager.Instantiate(sourceEntity);
            
            var pos = RandFloat3ToX(50, 100, -25);
            _entityManager.SetComponentData(entity, new Translation { Value = pos }); //Too many in the center breaks things. new float3(100,50,-25) }); //Spawn them in the center
            _entityManager.AddComponentData(entity, new PositionComponent { position = pos, origionalPosition = pos, active = true });

            if (removePhysics)
            {
                _entityManager.RemoveComponent<PhysicsCollider>(entity);
            }
            counter++;

            _logs.AppendLine("Set position component to 0," + i + ",0");
        }
    }

    public void SpawnWithNoPhysics(int newCount)
    {
        count = newCount;
        SpawnWithPhysics(false);
    }

    public void Spawn()
    {
        SpawnWithPhysics(true);
    }

    void SpawnWithPhysics(bool physicsEnabled)
    {
        ECSHelper.EnableSystem<PositionWiggleSystem>(true);
        StartCoroutine(SpawnInternal(physicsEnabled));
    }

    IEnumerator SpawnInternal(bool physicsEnabled)
    {
        zoomedForVertices = false;
        Debug.Log("SpawnInternal Fired");
        var gravitateToTargetSystem = World.Active.GetOrCreateSystem<GravitateToTargetSystem>();
        gravitateToTargetSystem.SetPhysicsScales(70,0.5f,20,30);

        //Not bothering constructing complex objects with ECS via code yet
        //SpawnViaArcheTypesAndConstruct();


        //Left all of this commented out on purpose, it seems because I was not calling jobHandle.Complete in GravitateToTargetSystem, 
        //changing the entity list while that job is running seems to break things for ECS as we try to add our objects.  Avoiding this issue for now by putting in a .complete wait
        //gravitateToTargetSystem.Enabled = false;
        Debug.Log("SpawnInternal Waiting for a frame framecount:" + Time.frameCount);
        //yield return new WaitForFixedUpdate();
        //yield return new WaitForFixedUpdate();
        //yield return new WaitForFixedUpdate();
        //yield return new WaitForSeconds(0.1f); //Wait a frame so that ECS can do it its thing? Otherwise the new entities created while GravitateToTargetSystem is running, are not displayed.
        Debug.Log("SpawnInternal After Wait framecount:" + Time.frameCount);
        //SpawnViaPrebuiltGameObjects(DefaultScale);
        //yield return new WaitForFixedUpdate();
        //yield return new WaitForFixedUpdate();
        //yield return new WaitForFixedUpdate();
        //yield return new WaitForSeconds(0.1f); //Wait a frame so that ECS can do it its thing? Otherwise the new entities created while GravitateToTargetSystem is running, are not displayed.
        // gravitateToTargetSystem.Enabled = true;



        SpawnViaPrebuiltGameObjects(DefaultScale);
        //Debug.Log("SpawnInternal Done:" + Time.frameCount);

        yield break;
    }

    public void SetLightDirection(bool forward)
    {
        var light = GameObject.Find("Directional Light");
        light.transform.position = Camera.main.transform.position;
        if (forward)
        {
            light.transform.rotation = Quaternion.Euler(Vector3.right * 60);
        }
        else
        {
            light.transform.rotation = Quaternion.Euler(Vector3.right * 120);
        }
    }

    public void Clear()
    {
        ECSHelper.EnableSystem<VerticesCopySystem>(false); //Should only be running while we've got something to copy to

        var queryDes = new EntityQueryDesc { All = new ComponentType[] { typeof(PositionComponent) } };
        var query = World.Active.EntityManager.CreateEntityQuery(queryDes);
        World.Active.EntityManager.DestroyEntity(query);
        query.Dispose();

        Camera.main.transform.position = cameraTransform.position;
        Camera.main.transform.rotation = cameraTransform.rotation;
        
        SetLightDirection(true);
        counter = 0;
        count = 100;
    }

    public void SpawnWithVertices(bool useLerp)
    {
        zoomedForVertices = true;
        Clear();
        var verticesCopysystem = World.Active.GetOrCreateSystem<VerticesCopySystem>();
        verticesCopysystem.SetVertices(SkinToDrawWithItems,UnityEngine.Animations.Axis.None,0);
        verticesCopysystem.Enabled = true;
        count = verticesCopysystem.GetPointsCount();

        var gravitateToTargetSystem = World.Active.GetOrCreateSystem<GravitateToTargetSystem>();
        gravitateToTargetSystem.SetPhysicsScales(75, 2f, 3,15);

        if (useLerp)
            SpawnViaPrebuiltGameObjects(SkinRendererScale, true);
        else
            SpawnViaPrebuiltGameObjects(SkinRendererScale);

        Camera.main.transform.position = cameraTransformSkinned.position;
        Camera.main.transform.rotation = cameraTransformSkinned.rotation;
        SetLightDirection(false);

        ECSHelper.EnableSystem<PositionWiggleSystem>(false);
        ECSHelper.EnableSystem<LerpPositionSystem>(useLerp);
        ECSHelper.EnableSystem<GravitateToTargetSystem>(!useLerp);
    }

    Vector3[] TrimCloseVertices(Vector3[] vertices, int depth, int totalDepth, float distance)
    {
        List<Vector3> _vector3s = new List<Vector3>(vertices);
        Debug.Log("_vector3s.Count:" + _vector3s.Count);

        for (int i = _vector3s.Count; i > 0; i--)
        {
            Debug.Log("Distance:" + Vector3.Distance(_vector3s[i], _vector3s[_vector3s.Count - i]));
            if (Vector3.Distance(_vector3s[i], _vector3s[_vector3s.Count-i]) < distance)
            {
                if (_vector3s.Contains(_vector3s[i]))
                {
                    Debug.Log("Removed entry");
                    _vector3s.Remove(_vector3s[i]);
                }
            }
        }

        return _vector3s.ToArray();
        //depth++;
        //if (depth > totalDepth)
        //    return;
    }

    void SetupWorld()
    {
        var floorArcheType = _entityManager.CreateArchetype(
            typeof(Translation),
            typeof(Rotation),
            typeof(RenderMesh),
            typeof(PhysicsCollider)
        );

        var floorEntity = _entityManager.CreateEntity(floorArcheType);
        
        var collider = Unity.Physics.BoxCollider.Create(float3.zero, Quaternion.identity, new float3(10.0f,1.0f,10.0f), 0.01f);
        _entityManager.SetComponentData(floorEntity, new PhysicsCollider() { Value = collider });
        _entityManager.SetComponentData(floorEntity, new Translation() { Value = new float3(0,-5,0) });
        _entityManager.SetSharedComponentData(floorEntity, new RenderMesh() { mesh = FloorMesh, material = FloorMaterial });
    }

    /// <summary>
    /// Reset all back to original grid of random positions
    /// </summary>
    public void ResetPositions()
    {
        var queryResponse = _entityManager.CreateEntityQuery(new ComponentType[] { typeof(PositionComponent) });
        var positionComponents = queryResponse.ToComponentDataArray<PositionComponent>(Allocator.TempJob);
        var entities = queryResponse.ToEntityArray(Allocator.TempJob);

        for (int i = 0; i < positionComponents.Length;i++)
        {
            var pos = RandFloat3ToX(50, 100, -25);
            _entityManager.SetComponentData(entities[i], new PositionComponent { position = pos, origionalPosition = pos, active = true });
        }

        entities.Dispose();
        queryResponse.Dispose();
        positionComponents.Dispose(); 
    }

    /// <summary>
    /// Generate a random float3
    /// </summary>
    /// <param name="xOffSet"></param>
    /// <param name="max"></param>
    /// <param name="zOffSet"></param>
    /// <returns></returns>
    float3 RandFloat3ToX(float xOffSet, float max, float zOffSet)
    {
        return _rand.NextFloat3(new float3(xOffSet, 0, zOffSet), new float3(max + xOffSet, max, max + zOffSet));
    }

    /// <summary>
    /// Create entities without the need for game objects. Construct it purely with ECS Components
    /// </summary>
    void SpawnViaArcheTypesAndConstruct()
    {
        var archeType = _entityManager.CreateArchetype(
            typeof(PositionComponent),
            typeof(LocalToWorld),
            typeof(Translation),
            typeof(Rotation),
            typeof(RenderMesh),
            typeof(PhysicsVelocity),
            typeof(PhysicsDamping),
            typeof(PhysicsMass),
            typeof(PhysicsCollider)
        );

        for (int i = 0; i < count; i++)
        {
            var entity = _entityManager.CreateEntity(archeType);

            _entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = ButterFlyMesh,
                material = ButterFlyMaterial
            });
            var collider = Unity.Physics.BoxCollider.Create(float3.zero, Quaternion.identity, new float3(1.0f), 0.01f);

            var physicsCollider = new PhysicsCollider { Value = collider };
            var pos = RandFloat3ToX(-50, 100, 0);
            _entityManager.SetComponentData(entity, new PositionComponent { position = pos, origionalPosition = pos, active = true });
            _entityManager.SetComponentData(entity, physicsCollider);

            //_entityManager.SetComponentData(entity, new PhysicsVelocity()
            //{
            //    Linear = float3.zero,
            //    Angular = float3.zero
            //});

            ////var mass = 1.0f;
            //////Unity.Physics.Collider* colliderPtr = (Unity.Physics.Collider*)collider.GetUnsafePtr();
            ////_entityManager.AddComponentData(butterFlyEntity, new PhysicsMass() { PhysicsMass.CreateDynamic(colliderPtr->MassProperties, mass));// colliderPtr->MassProperties, mass));

            //_entityManager.SetComponentData(entity, new PhysicsDamping()
            //{
            //    Linear = 0.01f,
            //    Angular = 0.05f
            //});

            _logs.AppendLine("Instantiated a butterFlyEntity in the entityManager.");
        }

        //Code to do the same thing that a job does in a script, can run this anywhere in Unity too, should work in CoRoutines
        void SetInPositionListWithoutECS(NativeArray<float3> positions)
        {
            var queryResponse = _entityManager.CreateEntityQuery(new ComponentType[] { typeof(PositionComponent), typeof(Translation) });
            var positionComponents = queryResponse.ToComponentDataArray<PositionComponent>(Allocator.TempJob);
            var translations = queryResponse.ToComponentDataArray<Translation>(Allocator.TempJob);
            var entities = queryResponse.ToEntityArray(Allocator.TempJob);

            for (int i = 0; i < positionComponents.Length; i++)
            {
                _entityManager.SetComponentData(entities[i], new Translation { Value = positions[i] });
            }

            entities.Dispose();
            queryResponse.Dispose();
            positionComponents.Dispose();
            translations.Dispose();
        }
    }
}

