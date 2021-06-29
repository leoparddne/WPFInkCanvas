using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Muti_Touch
{
    public class MoveRotateAdornerConfig
    {
        public Rect Rect { get; set; }
        public bool? XAdd { get; set; }
        public bool? YAdd { get; set; }
        public Point? DiagonPoint { get; set; }
    }
}
