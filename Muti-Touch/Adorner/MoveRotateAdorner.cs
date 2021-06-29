using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace Muti_Touch
{
    public class MoveRotateAdorner : Adorner
    {
        /// <summary>
        /// 控件中心点
        /// (旋转使用)
        /// INFO 如果后续需要切换中心点定位只需要调整此变量即可 
        /// </summary>
        private Point Center;
        Rect bound;

        Rect scaleRound;
        Point lastResizePosition;

        StrokeCollection selectionStrokes = null;
        EllipseGeometry rotateGeometry;
        EllipseGeometry closeGeometry;

        //所装饰的对象
        UIElement adornedElement;


        //上一次移动的角度
        private double lastAngle;

        /// <summary>
        /// 锚点列表
        /// </summary>
        List<MoveRotateAdornerConfig> anchors = new List<MoveRotateAdornerConfig>();


        private MoveRotateAdornerConfig selectAnchor = null;


        public MoveRotateAdorner(UIElement adornedElement, StrokeCollection selectionStrokes = null) : base(adornedElement)
        {
            this.adornedElement = adornedElement;
            if ((selectionStrokes?.Count ?? 0) > 0)
            {
                this.selectionStrokes = selectionStrokes;
                bound = selectionStrokes.GetBounds();
                scaleRound = bound;
            }
        }
        bool needRotate = false;
        bool resize = false;
        bool isMove = false;
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            var mousePosition = e.MouseDevice.GetPosition(this);
            //TODO control mode
            if (rotateGeometry.Bounds.Contains(mousePosition))
            {
                //重置
                lastAngle = 0;
                needRotate = true;
                this.CaptureMouse();

                //ReDraw();
            }
            else
            if (closeGeometry.Bounds.Contains(mousePosition))
            {
                this.ClearAdorner();
            }
            else
            {
                isMove = true;
                lastResizePosition = mousePosition;
            }

            foreach (var anchor in anchors)
            {
                if (anchor.Rect.Contains(mousePosition))
                {
                    selectAnchor = anchor;
                    resize = true;
                    lastResizePosition = mousePosition;
                    this.CaptureMouse();
                    break;
                }
            }

            base.OnMouseLeftButtonDown(e);
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            var position = e.GetPosition(this);

            if (needRotate)
            {
                var middlePoint = new Point(bound.Left + bound.Width / 2, bound.Top + bound.Height / 2);
                position.X -= middlePoint.X;
                position.Y -= middlePoint.Y;


                //计算角度
                var x = -position.X;// bound.Width/2;
                //翻转y轴坐标系
                var y = position.Y;// bound.Height/2;
                var angle = Math.Atan2(x, y) * (180 / Math.PI);

                Rotate(angle - lastAngle);
                lastAngle = angle;

                this.Visibility = Visibility.Hidden;
                this.Visibility = Visibility.Visible;

                Trace.WriteLine($"x:{x},y:{y},angle:{angle}");

                //var tempBound = selectionStrokes.GetBounds();
                ReDraw(this);
            }
            if (resize)
            {
                if (selectAnchor == null)
                {
                    return;
                }
                //TODO
                //Trace.WriteLine($"position x:{position.X},position y:{position.Y},startPosition x:{startPosition.X},startPosition y:{startPosition.Y}");
                double scaleX = 1;
                double scaleY = 1;
                //计算缩放比例
                if (selectAnchor.XAdd != null && this.ActualWidth != 0 && (position.X != lastResizePosition.X))
                {
                    scaleX = (position.X - lastResizePosition.X) / scaleRound.Width;
                    //翻转坐标系
                    scaleX = selectAnchor.XAdd.Value ? scaleX : -scaleX;
                    //加上原始比例
                    scaleX++;
                }
                if (selectAnchor.YAdd != null && this.ActualHeight != 0 && (position.Y != lastResizePosition.Y))
                {
                    scaleY = (position.Y - lastResizePosition.Y) / scaleRound.Height;
                    scaleY = selectAnchor.YAdd.Value ? scaleY : -scaleY;
                    scaleY++;
                }

                //禁止翻转
                if (scaleX <= 0 || scaleY <= 0)
                {
                    return;
                }

                //scaleX = 1.5;
                //scaleY = 1;
                if (selectAnchor.DiagonPoint != null)
                {
                    lastResizePosition = position;
                    ReSize(scaleX, scaleY, selectAnchor.DiagonPoint.Value.X, selectAnchor.DiagonPoint.Value.Y);
                }
            }
            if (isMove)
            {
                //TODO
                var tranX = (position.X - lastResizePosition.X);
                var tranY = (position.Y - lastResizePosition.Y);

                lastResizePosition = position;

                Move(tranX, tranY);
            }
            base.OnMouseMove(e);
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (selectionStrokes == null)
            {
                return;
            }
            needRotate = false;
            resize = false;

            isMove = false;

            this.ReleaseMouseCapture();

            bound = selectionStrokes.GetBounds();

            base.OnMouseUp(e);
        }
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
        }

        //protected override Size MeasureOverride(Size constraint)
        //{
        //    if(selectionStrokes!=null)
        //    var bound = selectionStrokes.GetBounds();
        //    //return base.MeasureOverride(constraint);
        //    return base.MeasureOverride(bound.Size);

        //}
        protected override Size ArrangeOverride(Size finalSize)
        {
            return base.ArrangeOverride(finalSize);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            //if(selectionStrokes != null)
            //{
            //    bound= new Rect(AdornedElement.RenderSize)
            //}
            Rect adornedElementRect = (selectionStrokes == null) ? new Rect(AdornedElement.RenderSize) : bound;

            Center.X = adornedElementRect.Width / 2;
            Center.Y = adornedElementRect.Height / 2;


            SolidColorBrush renderBrush = new SolidColorBrush(Colors.Green);
            renderBrush.Opacity = 0.2;

            Pen renderPen = new Pen(new SolidColorBrush(Colors.Navy), 1.5);
            drawingContext.DrawRectangle(renderBrush, renderPen, adornedElementRect);


            //锚点尺寸、宽高一致
            const double rectSize = 5;
            //绘制锚点
            adornedElementRect = DrawChangeSizeAnchor(drawingContext, adornedElementRect, rectSize);

            //绘制边框
            DrawBorder(drawingContext, adornedElementRect);

            const double ellipseSize = 10;
            DrawCloseAndRotate(drawingContext, adornedElementRect, ellipseSize, ellipseSize);

            base.OnRender(drawingContext);
        }

        #region render
        /// <summary>
        /// 绘制关闭、选中按钮
        /// </summary>
        /// <param name="drawingContext"></param>
        /// <param name="adornedElementRect"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void DrawCloseAndRotate(DrawingContext drawingContext, Rect adornedElementRect, double width, double height)
        {
            const double margin = 10;
            closeGeometry = DrawEllipse(drawingContext, adornedElementRect.Left + Center.X, adornedElementRect.Top - margin, width, height, Colors.Red);
            rotateGeometry = DrawEllipse(drawingContext, adornedElementRect.Left + Center.X, adornedElementRect.Top + adornedElementRect.Height + margin, width, height, Colors.LightBlue);

        }

        private EllipseGeometry DrawEllipse(DrawingContext drawingContext, double x, double y, double width, double height, Color? brushColor = null, string img = null)
        {
            if (brushColor == null)
            {
                brushColor = Colors.Green;
            }
            SolidColorBrush renderBrush = new SolidColorBrush((Color)brushColor);

            Pen renderPen = new Pen(new SolidColorBrush(Colors.Transparent), 1.5);

            var ellGeometry = new EllipseGeometry(new Point(x, y), width, height);
            drawingContext.DrawGeometry(renderBrush, renderPen, ellGeometry);

            return ellGeometry;
        }


        /// <summary>
        /// 绘制调整大小的六个锚点
        /// </summary>
        /// <param name="drawingContext"></param>
        /// <param name="adornedElementRect"></param>
        /// <param name="rectSize"></param>
        /// <returns></returns>
        private Rect DrawChangeSizeAnchor(DrawingContext drawingContext, Rect adornedElementRect, double rectSize)
        {
            ///绘制四个角落顶点
            DrawRect(drawingContext, adornedElementRect.Right, adornedElementRect.Bottom, rectSize, rectSize, true, true, adornedElementRect.TopLeft);
            DrawRect(drawingContext, adornedElementRect.Right, adornedElementRect.Top, rectSize, rectSize, true, false, adornedElementRect.BottomLeft);
            DrawRect(drawingContext, adornedElementRect.Left, adornedElementRect.Top, rectSize, rectSize, false, false, adornedElementRect.BottomRight);
            DrawRect(drawingContext, adornedElementRect.Left, adornedElementRect.Bottom, rectSize, rectSize, false, true, adornedElementRect.TopRight);

            //水平居中锚点
            DrawRect(drawingContext, adornedElementRect.Left, adornedElementRect.Top + Center.Y, rectSize, rectSize, false, null, new Point(adornedElementRect.Left + adornedElementRect.Width, adornedElementRect.Top + Center.Y));
            DrawRect(drawingContext, adornedElementRect.Left + adornedElementRect.Width, adornedElementRect.Top + Center.Y, rectSize, rectSize, true, null, new Point(adornedElementRect.Left, adornedElementRect.Top + Center.Y));

            //垂直居中锚点
            DrawRect(drawingContext, adornedElementRect.Left + Center.X, adornedElementRect.Top, rectSize, rectSize, null, false, new Point(adornedElementRect.Left + Center.X, adornedElementRect.Top + adornedElementRect.Height));
            DrawRect(drawingContext, adornedElementRect.Left + Center.X, adornedElementRect.Top + adornedElementRect.Height, rectSize, rectSize, null, true, new Point(adornedElementRect.Left + Center.X, adornedElementRect.Top));


            //居中的定位锚点
            DrawRect(drawingContext, adornedElementRect.Left + Center.X, adornedElementRect.Top + Center.Y, rectSize, rectSize, null, null);

            return adornedElementRect;
        }

        /// <summary>
        /// 绘制矩形
        /// </summary>
        /// <param name="drawingContext"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="xAdd">x轴正方向为放大</param>
        /// <param name="yAdd">y轴正方向为放大</param>
        /// <param name="diagonPoint">对角</param>
        private void DrawRect(DrawingContext drawingContext, double x, double y, double width, double height, bool? xAdd = null, bool? yAdd = null, Point? diagonPoint = null)
        {
            SolidColorBrush renderBrush = new SolidColorBrush(Colors.Green);
            renderBrush.Opacity = 0.2;

            Pen renderPen = new Pen(new SolidColorBrush(Colors.Navy), 1.5);
            Rect rect = new Rect(x - width / 2, y - height / 2, width, height);

            if (!anchors.Exists(f => f.Rect == rect))
            {
                anchors.Add(new MoveRotateAdornerConfig
                {
                    Rect = rect,
                    XAdd = xAdd,
                    YAdd = yAdd,
                    DiagonPoint = diagonPoint
                });
            }

            drawingContext.DrawRectangle(renderBrush, renderPen, rect);
        }

        /// <summary>
        /// 绘制边框
        /// </summary>
        /// <param name="drawingContext"></param>
        /// <param name="adornedElementRect"></param>
        private void DrawBorder(DrawingContext drawingContext, Rect adornedElementRect)
        {
            DrawLine(drawingContext, adornedElementRect.TopLeft, adornedElementRect.TopRight);
            DrawLine(drawingContext, adornedElementRect.TopRight, adornedElementRect.BottomRight);
            DrawLine(drawingContext, adornedElementRect.BottomRight, adornedElementRect.BottomLeft);
            DrawLine(drawingContext, adornedElementRect.BottomLeft, adornedElementRect.TopLeft);
        }

        /// <summary>
        /// 绘制直线
        /// </summary>
        /// <param name="drawingContext"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void DrawLine(DrawingContext drawingContext, Point start, Point end)
        {
            //虚线
            DoubleCollection dc = new DoubleCollection();
            dc.Add(2);
            DashStyle dashstyle = new DashStyle();
            dashstyle.Dashes = dc;
            //dashstyle.Offset = 0;

            var borderColor = (Color)ColorConverter.ConvertFromString("#9C9FAA");

            Pen renderPen = new Pen(new SolidColorBrush(borderColor), 1.5);
            renderPen.DashStyle = dashstyle;
            renderPen.DashCap = PenLineCap.Round;

            drawingContext.DrawLine(renderPen, start, end);
        }

        #endregion

        private void Rotate(double angle)
        {
            if (selectionStrokes.Count == 0)
            {
                return;
            }
            //var bound = selectionStrokes.GetBounds();

            Matrix matrix = new Matrix();
            matrix.RotateAt(angle, bound.Left + bound.Width / 2, bound.Top + bound.Height / 2);
            selectionStrokes.Transform(matrix, false);


            bound = selectionStrokes.GetBounds();

        }

        //double lastScaleX = 1;
        //double lastScaleY = 1;
        /// <summary>
        /// 缩放
        /// </summary>
        /// <param name="scaleX">缩放比例</param>
        /// <param name="scaleY">缩放比例</param>
        /// <param name="middleX">缩放中心点</param>
        /// <param name="middleY">缩放中心点</param>
        private void ReSize(double scaleX, double scaleY, double middleX, double middleY)
        {
            if ((selectionStrokes?.Count ?? 0) == 0)
            {
                return;
            }

            Matrix matrix = new Matrix();

            matrix.ScaleAt(scaleX, scaleY, middleX, middleY);

            Trace.WriteLine($"scale x:{ scaleX}, scale y:{ scaleY}");
            selectionStrokes.Transform(matrix, false);
            scaleRound = selectionStrokes.GetBounds();

            bound = selectionStrokes.GetBounds();
            ReDraw(this);
        }
        public void Move(double x, double y)
        {
            if ((selectionStrokes?.Count ?? 0) == 0)
            {
                return;
            }

            Matrix matrix = new Matrix();

            matrix.OffsetX = x;
            matrix.OffsetY = y;

            selectionStrokes.Transform(matrix, false);
            scaleRound = selectionStrokes.GetBounds();

            bound = selectionStrokes.GetBounds();
            ReDraw(this);
        }

        public void ReDraw(MoveRotateAdorner adorner = null)
        {
            ClearAdorner();
            ReDrawAdorner(adorner);
        }

        public void ClearAdorner()
        {
            var layer = AdornerLayer.GetAdornerLayer(adornedElement);
            var arr = layer.GetAdorners(adornedElement);//获取该控件上所有装饰器，返回一个数组

            if (arr != null)
            {
                for (int i = arr.Length - 1; i >= 0; i--)
                {
                    layer.Remove(arr[i]);
                }
            }
        }
        public MoveRotateAdorner ReDrawAdorner(MoveRotateAdorner adorner = null)
        {
            var layer = AdornerLayer.GetAdornerLayer(adornedElement);

            adorner = adorner ?? new MoveRotateAdorner(adornedElement, selectionStrokes);
            layer.Add(adorner);
            return adorner;
        }
    }
}
