using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;

namespace Muti_Touch.Util
{
    /// <summary>
    /// 画图类
    /// </summary>
    public class Draw
    {
        /// <summary>
        /// 初始化对象
        /// </summary>
        /// <param name="broad">画布</param>
        public Draw(InkCanvas broad)
        {
            this.broad = broad;
        }

        InkCanvas broad;


        public Stroke Line(Point a, Point b)
        {
            StylusPointCollection points = new StylusPointCollection(new List<Point> { a, b });
            Stroke line = new Stroke(points);
            line.DrawingAttributes = broad.DefaultDrawingAttributes.Clone();
            return line;
        }

        /// <summary>
        /// 以矩形的两个对角坐标为根据画矩形
        /// </summary>
        /// <param name="a">一个顶点坐标</param>
        /// <param name="c">与a相对点的坐标</param>
        /// <returns></returns>
        public Stroke Rectangle(Point a, Point c)
        {
            Point b = new Point(c.X, a.Y),
            d = new Point(a.X, c.Y);
            StylusPointCollection points = new StylusPointCollection(new List<Point> { a, b, c, d, a });
            Stroke rectangle = new Stroke(points);
            rectangle.DrawingAttributes = broad.DefaultDrawingAttributes.Clone();
            return rectangle;
        }

        /// <summary>
        /// 绘制三角形
        /// </summary>
        /// <param name="a">点a</param>
        /// <param name="b">点b</param>
        /// <param name="c">点c</param>
        /// <returns></returns>
        public Stroke Triangle(Point a, Point b, Point c)
        {
            StylusPointCollection points = new StylusPointCollection(new List<Point>() { a, b, c, a });
            Stroke triangle = new Stroke(points);
            triangle.DrawingAttributes = broad.DefaultDrawingAttributes.Clone();
            return triangle;
        }

        /// <summary>
        /// 绘制多边形
        /// </summary>
        /// <param name="points">多边形的顶点</param>
        /// <returns></returns>
        public Stroke Polygon(StylusPointCollection points)
        {
            points.Add(points[0]);
            Stroke polygon = new Stroke(points);
            polygon.DrawingAttributes = broad.DefaultDrawingAttributes.Clone();
            return polygon;
        }

        public Stroke Circle(Point o, Point p)
        {
            StylusPointCollection points = new(GenerateEclipseGeometry(o, p));

            Stroke circle = new Stroke(points);
            circle.DrawingAttributes = broad.DefaultDrawingAttributes.Clone();

            return circle;
        }
        private List<System.Windows.Point> GenerateEclipseGeometry(System.Windows.Point st, System.Windows.Point ed)
        {
            double a = 0.5 * (ed.X - st.X);
            double b = 0.5 * (ed.Y - st.Y);
            List<System.Windows.Point> pointList = new List<System.Windows.Point>();
            for (double r = 0; r <= 2 * Math.PI; r = r + 0.01)
            {
                pointList.Add(new System.Windows.Point(0.5 * (st.X + ed.X) + a * Math.Cos(r), 0.5 * (st.Y + ed.Y) + b * Math.Sin(r)));
            }
            return pointList;
        }
    }

}
