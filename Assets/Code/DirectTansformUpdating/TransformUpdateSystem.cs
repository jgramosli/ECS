using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

namespace Assets.Code
{
    public class TransformUpdateSystem : ComponentSystem
    {
        GameObject gameObject = null;
        int count = 100;
        TransformAccessArray _transformAccesses;
        JobHandle _jobHandle;
        protected override void OnUpdate()
        {
            //Disable for now
            this.Enabled = false;
            return; 

            if(!_jobHandle.IsCompleted)
            {
                return;
            }

            if (gameObject == null)
            {
                Debug.Log("Loading Capsule");
                gameObject = Resources.Load<GameObject>("Capsule");
                var random = new System.Random();
                var transforms = new Transform[count];
                for (int i = 0; i < count; i++)
                {
                    var instantiedGameObject = GameObject.Instantiate(gameObject);
                    instantiedGameObject.transform.localPosition = new Vector3(random.Next(0,50), random.Next(0, 50), random.Next(0, 50));
                    Debug.Log("instantiedGameObject: " + (instantiedGameObject == null ? "Null" : "Not Null"));
                    transforms[i] = instantiedGameObject.transform;
                }
                _transformAccesses = new TransformAccessArray(transforms);
            }

            var newJob = new ParalellTransformUpdaterJob(Time.time);
            _jobHandle = newJob.Schedule(_transformAccesses);
        }
    }
}
