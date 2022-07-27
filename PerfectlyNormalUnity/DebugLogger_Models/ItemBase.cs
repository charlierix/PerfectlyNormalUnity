using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PerfectlyNormalUnity.DebugLogger_Models
{
    public abstract class ItemBase
    {
        // All of these properties are optional

        public Category category { get; set; }

        public Color? color { get; set; }

        public double? size_mult { get; set; }

        public string tooltip { get; set; }
    }
}
