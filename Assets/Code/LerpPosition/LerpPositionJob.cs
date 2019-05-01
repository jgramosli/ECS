using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public struct LerpPositionJob : IJobForEachWithEntity<Translation, PositionComponent>
{
    float _lerpSpeed;

    public LerpPositionJob(float lerpSpeed)
    {
        _lerpSpeed = lerpSpeed;
    }

    public void Execute(Entity entity, int index, ref Translation translation, ref PositionComponent positionComponent)
    {
        translation.Value = math.lerp(translation.Value, positionComponent.position, _lerpSpeed);
    }
}
