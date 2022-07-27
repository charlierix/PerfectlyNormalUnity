using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PerfectlyNormalUnity.DebugLogger_Models
{
    public class Category
    {
        public string name { get; set; }

        public Color? color { get; set; }      // optional

        public float? size_mult { get; set; }      // optional
    }
}
