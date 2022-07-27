using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PerfectlyNormalUnity.DebugLogger_Models
{
    public class LogFrame
    {
        public string name { get; set; }       // optional

        public Color? back_color { get; set; }     // optional

        public ItemBase[] items { get; set; }

        public Text[] text { get; set; }
    }
}
