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
    public UnityEngine.SkinnedMeshRenderer SkinToDrawWithItems;
    public Transform cameraTransform, cameraTransformSkinned;
    public int counter = 0;
    public int count = 100;

    StringBuilder _logs = new StringBuilder();
    EntityManager _entityManager;
    World _initialWorld;

    TransformAccessArray _goTransforms;
    Unity.Mathematics.Random _rand;
    float _defaultSize = 3f, _skinSize = 0.5f;

    public void Start()
    {
        _entityManager = World.Active.EntityManager;
        _initialWorld = World.Active;

        _rand = new Unity.Mathematics.Random();
        _rand.InitState();

        SetupWorld();

        //_entityManager.AddComponentData(butterFlyEntity, PhysicsStep.Default);//new PhysicsStep() { SimulationType = SimulationType.UnityPhysics, Gravity = 0.6f, SolverIterationCount = 2, ThreadCountHint = 16 });

        Spawn();

        Debug.Log("Finished created " + count + " entities" +"\nLogs:\n" + _logs.ToString());
    }

    public void SpawnViaPrebuiltGameObjects(float scale, bool removePhysics = false)
    {
        var gameObject = Resources.Load<UnityEngine.GameObject>("ConvertGOToEntity");
        gameObject.transform.localScale = new Vector3(scale,scale,scale);

        Entity sourceEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(gameObject, _initialWorld);

        for (int i = 0; i < count; i++)
        {
            var entity = _entityManager.Instantiate(sourceEntity);
            
            var pos = RandFloat3ToX(50, 100, -25);

            _entityManager.AddComponentData(entity, new PositionComponent { position = pos, origionalPosition = pos });
            _entityManager.SetComponentData(entity, new Translation { Value = pos }); //Too many in the center breaks things. new float3(100,50,-25) }); //Spawn them in the center

            //var localScale = _entityManager.GetComponentData<NonUniformScale>(entity);
            //localScale.Value = new float3(scale, scale, scale);
            //_entityManager.SetComponentData(entity, localScale);

            if (removePhysics)
            {
                _entityManager.RemoveComponent<PhysicsCollider>(entity);
            }
            counter++;

            _logs.AppendLine("Set position component to 0," + i + ",0");
        }
    }

    public void Spawn()
    {
        var gravitateToTargetSystem = World.Active.GetOrCreateSystem<GravitateToTargetSystem>();
        gravitateToTargetSystem.SetExplosionMultiplier(200,100);

        //SpawnViaArcheTypesAndConstruct();

        SpawnViaPrebuiltGameObjects(_defaultSize);
    }

    public void SetLightDirection()
    {
        var light = GameObject.Find("Directional Light");
        light.transform.position = Camera.main.transform.position;
        light.transform.rotation = Quaternion.Euler(Vector3.right * 120);
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
        
        SetLightDirection();
        counter = 0;
        count = 100;
    }

    public void SpawnInSkin(bool useLerp)
    {
        Clear();
        var verticesCopysystem = World.Active.GetOrCreateSystem<VerticesCopySystem>();
        verticesCopysystem.SetVertices(SkinToDrawWithItems,UnityEngine.Animations.Axis.None,0);
        verticesCopysystem.Enabled = true;
        count = verticesCopysystem.GetPointsCount();

        var gravitateToTargetSystem = World.Active.GetOrCreateSystem<GravitateToTargetSystem>();
        gravitateToTargetSystem.SetExplosionMultiplier(50, 75);

        if (useLerp)
            SpawnViaPrebuiltGameObjects(_skinSize, true);
        else
            SpawnViaPrebuiltGameObjects(_skinSize);

        Camera.main.transform.position = cameraTransformSkinned.position;
        Camera.main.transform.rotation = cameraTransformSkinned.rotation;
        SetLightDirection();

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

    //Hack to do the same thing that a job does in a script
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

    public void ResetPositions()
    {
        var queryResponse = _entityManager.CreateEntityQuery(new ComponentType[] { typeof(PositionComponent) });
        var positionComponents = queryResponse.ToComponentDataArray<PositionComponent>(Allocator.TempJob);
        var entities = queryResponse.ToEntityArray(Allocator.TempJob);

        for (int i = 0; i < positionComponents.Length;i++)
        {
            var pos = RandFloat3ToX(50, 100, -25);
            _entityManager.SetComponentData(entities[i], new PositionComponent { position = pos, origionalPosition = pos });
        }

        entities.Dispose();
        queryResponse.Dispose();
        positionComponents.Dispose(); 
    }

    float3 RandFloat3ToX(float xOffSet, float max, float zOffSet)
    {
        return _rand.NextFloat3(new float3(xOffSet, 0, zOffSet), new float3(max + xOffSet, max, max + zOffSet));
    }

    void SpawnViaArcheTypesAndConstruct()
    {
        var archeType = _entityManager.CreateArchetype(
            typeof(PositionComponent),
            typeof(LocalToWorld),
            typeof(Translation),
            typeof(Rotation),
            typeof(RenderMesh)
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
            _entityManager.SetComponentData(entity, new PositionComponent { position = pos, origionalPosition = pos });
            _entityManager.AddComponentData(entity, physicsCollider);
            _entityManager.AddComponentData(entity, new PhysicsVelocity()
            {
                Linear = float3.zero,
                Angular = float3.zero
            });

            //var mass = 1.0f;
            ////Unity.Physics.Collider* colliderPtr = (Unity.Physics.Collider*)collider.GetUnsafePtr();
            //_entityManager.AddComponentData(butterFlyEntity, new PhysicsMass() { PhysicsMass.CreateDynamic(colliderPtr->MassProperties, mass));// colliderPtr->MassProperties, mass));

            _entityManager.AddComponentData(entity, new PhysicsDamping()
            {
                Linear = 0.01f,
                Angular = 0.05f
            });

            _logs.AppendLine("Instantiated a butterFlyEntity in the entityManager.");
        }
    }
}

