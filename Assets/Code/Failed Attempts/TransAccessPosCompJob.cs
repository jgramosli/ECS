using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.Jobs;

namespace Assets.Code
{
    [BurstCompile]
    public struct TransAccessPosCompJob : IJobParallelForTransform
    {
        NativeArray<PositionComponent> _positionComponents;
        public int counter;

        public TransAccessPosCompJob(NativeArray<PositionComponent> positionComponents)
        {
            _positionComponents = positionComponents;
            positionComponents.Dispose();
            counter = 0;
        }

        public void Execute(int index, TransformAccess transform)
        {
            transform.localPosition = _positionComponents[index].position;
        }
    }
}
