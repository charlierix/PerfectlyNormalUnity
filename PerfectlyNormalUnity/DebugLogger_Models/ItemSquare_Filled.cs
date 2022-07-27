﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PerfectlyNormalUnity.DebugLogger_Models
{
    public class ItemSquare_Filled : ItemBase
    {
        public Vector3 center { get; set; }

        public Vector3 normal { get; set; }

        public float size_x { get; set; }

        public float size_y { get; set; }
    }
}