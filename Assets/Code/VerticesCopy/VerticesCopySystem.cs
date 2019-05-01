using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Assets.Code.VerticesCopy
{
    public class VerticesCopySystem : JobComponentSystem
    {
        
        JobHandle _jobHandle;
        SkinnedMeshRenderer _skinnedMeshRenderer;
        int skipCount = 1; //How many vertices we skip when creating the 

        public void SetVertices(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            _skinnedMeshRenderer = skinnedMeshRenderer;
        }

        public int GetPointsCount()
        {
            Mesh mesh = new Mesh();
            _skinnedMeshRenderer.BakeMesh(mesh);

            return mesh.vertices.Length / skipCount;
        }


        protected override void OnCreate()
        {
            Enabled = false;
            base.OnCreate();
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (!_jobHandle.IsCompleted)
            {
                Debug.Log("VertexesCopySystem Job IsCompleted:" + _jobHandle.IsCompleted);

                return _jobHandle;
            }

            Mesh mesh = new Mesh();
            _skinnedMeshRenderer.BakeMesh(mesh);
            var vertices = mesh.vertices;

            NativeArray<float3> naVertices = new NativeArray<float3>((vertices.Length / skipCount), Allocator.TempJob);

            for (int i = 0; i + 2 < naVertices.Length; i++)
            {
                var vert = vertices[i * skipCount];
                naVertices[i] = new float3(vert.x, vert.y, vert.z);
                //Debug.Log("Positions[i]:i" + i + " p:"+ + positions[i]);
            }

            var newJob = new VerticesCopyJob(naVertices);
            _jobHandle = newJob.Schedule(this, inputDeps);

            _jobHandle.Complete();
            naVertices.Dispose();

            return _jobHandle;
        }
    }
}
