using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct PositionComponent : IComponentData
{
    public float3 position;
    public float3 origionalPosition;
    public bool active;
}

