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
            var system = World.Active.GetOrCreateSystem<T>();
            system.Enabled = enabled;
        }
    }
}
