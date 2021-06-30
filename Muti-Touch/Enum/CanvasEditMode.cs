using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Muti_Touch.Enum
{
    public enum CanvasEditMode
    {
        /// <summary>
        /// 选择笔迹
        /// </summary>
        SELECT,

        /// <summary>
        /// 移动画布
        /// </summary>
        MOVE,

        /// <summary>
        /// 绘制笔迹
        /// </summary>
        WRITE,

        /// <summary>
        /// 绘制形状
        /// </summary>
        WRITE_SHARP,

        /// <summary>
        /// 缩放画布
        /// </summary>
        ZOOM,

        /// <summary>
        /// 空
        /// </summary>
        NONE
    }

}
