using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PerfectlyNormalUnity.DebugLogger_Models
{
    [Serializable]
    public abstract class ItemBase
    {
        // All of these properties are optional

        public Category category;

        //public Color? color;      // unity's json serializer can't handle a lot of types
        public string color;        // AARRGGBB or null

        public float? size_mult;

        public string tooltip;
    }
}
