using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Muti_Touch
{
    //TODO 蚁行线
    public class LassoAdorner : Adorner
    {
        //所装饰的对象
        UIElement adornedElement;

        //绘制的点
        private List<Point> points = new List<Point>();
        public LassoAdorner(UIElement adornedElement) : base(adornedElement)
        {
            this.adornedElement = adornedElement;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Pen renderPen = new Pen(new SolidColorBrush(Colors.Yellow), 2);
            if (points.Count > 1)
            {
                var last = points[0];
                for (int i = 1; i < points.Count; i++)
                {
                    drawingContext.DrawLine(renderPen, last, points[i]);
                    //Trace.WriteLine($"OnRender:{last.X},{last.Y}-{points[i].X},{points[i].Y}");

                    last = points[i];
                }
            }


            base.OnRender(drawingContext);
        }

        public void AddPoint(Point p)
        {
            points.Add(p);
            //Trace.WriteLine("add point");
            ClearAdorner();
            ReDraw(this);
        }

        public void ClearAdorner()
        {
            //Trace.WriteLine("clear");

            var layer = AdornerLayer.GetAdornerLayer(adornedElement);
            var arr = layer.GetAdorners(adornedElement);//获取该控件上所有装饰器，返回一个数组

            if (arr != null)
            {
                for (int i = arr.Length - 1; i >= 0; i--)
                {
                    if (arr[i] is LassoAdorner)
                    {
                        layer.Remove(arr[i]);
                    }
                }
            }
        }
        public void ClearPoint()
        {
            points.Clear();
        }
        public LassoAdorner ReDraw(LassoAdorner adorner)
        {
            Trace.WriteLine("redraw");
            var layer = AdornerLayer.GetAdornerLayer(adornedElement);

            adorner = adorner;
            layer?.Add(adorner);
            return adorner;
        }
        //public void ReDrawAdorner(MoveRotateAdorner adorner = null)
        //{
        //    ClearAdorner();
        //    ReDrawAdorner(adorner);
        //}
    }

}
