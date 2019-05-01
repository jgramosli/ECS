using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Jobs;

namespace Assets.Code.VerticesCopy
{
    public class VerticesCopySystem : JobComponentSystem
    {
        
        JobHandle _jobHandle;
        SkinnedMeshRenderer _skinnedMeshRenderer;
        int skipCount = 1; //How many vertices we skip when creating the 
        Axis _axisToTwistOn;
        float _twistAmount;

        public void SetVertices(SkinnedMeshRenderer skinnedMeshRenderer, Axis axis, float twistAmount = 0)
        {
            _axisToTwistOn = axis;
            _skinnedMeshRenderer = skinnedMeshRenderer;
            _twistAmount = twistAmount;
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

            Vector3 center = new Vector3(0, 0, 0);
            Quaternion newRotation = new Quaternion();
            newRotation.eulerAngles = new Vector3(
                _axisToTwistOn == Axis.X ? _twistAmount : 0,
                _axisToTwistOn == Axis.Y ? _twistAmount : 0,
                _axisToTwistOn == Axis.Z ? _twistAmount : 0);

            for (int i = 0; i + 2 < naVertices.Length; i++)
            {
                var vert = vertices[i * skipCount];

                if(_axisToTwistOn != Axis.None)
                {
                    //Debug.Log("Setting rotation for " + _axisToTwistOn.ToString() + " axis");
                    vert = newRotation * (vert - center) + center;
                }

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
