using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Assets.Code
{
    public static class ECSHelper
    {
        public static void EnableSystem<T>(bool enabled) where T : ComponentSystemBase
        {
            var lerpSystem = World.Active.GetOrCreateSystem<T>();
            lerpSystem.Enabled = enabled;
        }
    }
}
