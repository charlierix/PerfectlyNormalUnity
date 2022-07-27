using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerfectlyNormalUnity.DebugLogger_Models
{
    public class LogScene
    {
        public Category[] categories { get; set; }

        public LogFrame[] frames { get; set; }

        public Text[] text { get; set; }
    }
}
