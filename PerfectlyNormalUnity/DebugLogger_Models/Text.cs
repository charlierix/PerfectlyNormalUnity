using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PerfectlyNormalUnity.DebugLogger_Models
{
    public class Text
    {
        public string text { get; set; }

        public Color? color { get; set; }      // optional

        public float? fontsize_mult { get; set; }     // optional
    }
}
