using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Code.VerticesCopy
{
    [BurstCompile]
    public struct VerticesCopyJob : IJobForEachWithEntity<PositionComponent>
    {
        [ReadOnly] NativeArray<float3> _vertices;

        public VerticesCopyJob(NativeArray<float3> vertices)
        {
            _vertices = vertices;
        }

        public void Execute(Entity entity, int index, ref PositionComponent positionComponent)
        {
            positionComponent.position = _vertices[index];
        }
    }
}
