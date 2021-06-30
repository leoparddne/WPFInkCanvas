using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Muti_Touch
{
    /// <summary>
    /// 预览装饰器
    /// </summary>
    public class PreviewAdorner : Adorner
    {
        //所装饰的对象
        UIElement adornedElement;

        /// <summary>
        /// 渲染时回调
        /// </summary>
        Action<DrawingContext> renderCallback;

        public PreviewAdorner(UIElement adornedElement, Action<DrawingContext> renderCallback = null) : base(adornedElement)
        {
            this.adornedElement = adornedElement;
            this.renderCallback = renderCallback;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            renderCallback?.Invoke(drawingContext);
            base.OnRender(drawingContext);
        }

        public void InjectRenderCallback(Action<DrawingContext> renderCallback)
        {
            this.renderCallback = renderCallback;
        }

        private void ClearAdorner()
        {
            var layer = AdornerLayer.GetAdornerLayer(adornedElement);
            var arr = layer.GetAdorners(adornedElement);//获取该控件上所有装饰器，返回一个数组

            if (arr != null)
            {
                for (int i = arr.Length - 1; i >= 0; i--)
                {
                    if (arr[i] is PreviewAdorner)
                    {
                        layer.Remove(arr[i]);
                    }
                }
            }
        }

        public PreviewAdorner ReDraw()
        {
            ClearAdorner();

            var layer = AdornerLayer.GetAdornerLayer(adornedElement);

            //重新将自身加入
            //触发重绘
            layer?.Add(this);
            return this;
        }
    }
}
