using Muti_Touch.Enum;
using Muti_Touch.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Muti_Touch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Point> paths = new List<Point>();
        LassoAdorner lasso;

        //绘制模式
        CanvasEditMode BlackMode = CanvasEditMode.WRITE;

        //绘制形状预览
        private PreviewAdorner previewAdorner;
        public MainWindow()
        {
            InitializeComponent();

            lasso = new LassoAdorner(writeBorad);
            previewAdorner = new PreviewAdorner(writeBorad);
        }
        private void image_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            //e.ManipulationContainer = touchPad;
            e.Mode = ManipulationModes.All;
        }
        private void image_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)e.Source;
            element.Opacity = 0.5;

            Matrix matrix = ((MatrixTransform)element.RenderTransform).Matrix;

            var deltaManipulation = e.DeltaManipulation;

            Point center = new Point(element.ActualWidth / 2, element.ActualHeight / 2);
            center = matrix.Transform(center);

            matrix.ScaleAt(deltaManipulation.Scale.X, deltaManipulation.Scale.Y, center.X, center.Y);

            matrix.RotateAt(e.DeltaManipulation.Rotation, center.X, center.Y);

            matrix.Translate(e.DeltaManipulation.Translation.X, e.DeltaManipulation.Translation.Y);

            ((MatrixTransform)element.RenderTransform).Matrix = matrix;
        }
        private void image_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)e.Source;
            element.Opacity = 1;
        }

        private void writeBorad_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //Trace.WriteLine("zoom");
            Point point = e.GetPosition(writeBorad);
            var delta = e.Delta * 0.001;
            DowheelZoom(null, point, delta);
        }
        private void DowheelZoom(System.Windows.Media.TransformGroup group, Point point, double delta)
        {
            //按住ctrl缩放
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                //缩放越界屏蔽即可
                if ((scale.ScaleX + delta) <= 1)
                {
                    //return;
                    scale.ScaleX = 1;
                    scale.ScaleY = 1;
                }
                else if (scale.ScaleX + delta > 5)
                {
                    scale.ScaleX = 5;
                    scale.ScaleY = 5;
                }
                else
                {
                    scale.ScaleX += delta;
                    scale.ScaleY += delta;


                }
                //根据鼠标缩放
                //scale.CenterX = point.X;
                //scale.CenterY = point.Y;
                //scale.CenterX = point.X;
                //scale.CenterY = point.Y;
                //scale.CenterX = point.X / scale.ScaleX;
                //scale.CenterY = point.Y / scale.ScaleY;

                ////trans.X = writeBorad.ActualWidth * scale.ScaleX * (point.X / writeBorad.ActualWidth);
                ////trans.Y = writeBorad.ActualHeight * scale.ScaleY * (point.Y / writeBorad.ActualHeight);
                //得到如下优化
                var targetX = point.X * (scale.ScaleX - 1);
                var targetY = point.Y * (scale.ScaleX - 1);

                trans.X = -targetX;
                trans.Y = -targetY;

                scaleSize = scale.ScaleX;

                adorner?.ReDraw();
            }
        }

        //空格按下
        bool pressBlank;
        //是否移入
        bool movein = false;
        //是否已加载
        bool loadhand = false;
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                pressBlank = true;
                if (loadhand)
                {
                    //Trace.WriteLine("loadhand");
                }
                else
                {
                    //Trace.WriteLine("pressBlank:" + pressBlank);
                }
            }
            if (movein && (!loadhand))
            {
                loadhand = true;
                //Trace.WriteLine("need load cursor");
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            pressBlank = false;
            //Trace.WriteLine(pressBlank);
            loadhand = false;
        }

        double scaleSize = 1;
        //绘制形状
        private void DrawSharp(MouseEventArgs e)
        {
            if (!mouseDown)
            {
                return;
            }
            SolidColorBrush renderBrush = new SolidColorBrush(Colors.Green);
            //renderBrush.Opacity = 0.5;

            Pen renderPen = new Pen(new SolidColorBrush(Colors.Yellow), 2);
            Point mousePoint = e.GetPosition(canvasGrid);

            previewAdorner.InjectRenderCallback((DrawingContext context) =>
            {
                //var tri = d.Triangle(startPoint, mousePoint, new Point(mousePoint.X-300, mousePoint.Y));
                context.DrawRectangle(renderBrush, renderPen, new Rect(startPoint, mousePoint));
            });

            previewAdorner.ReDraw();


        }
        private void writeBorad_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                //TODO 需要计算当前的模式而不是通过复杂的判断
                if (CanvasEditMode.WRITE_SHARP == BlackMode)
                {
                    DrawSharp(e);
                }
                else
                {
                    paths.Add(e.GetPosition(writeBorad));
                    if (writeBorad.EditingMode == InkCanvasEditingMode.None)
                    {
                        lasso.AddPoint(e.GetPosition(writeBorad));
                    }
                }
            }

            

            movein = true;
            if (pressBlank && (!loadhand) && movein)
            {
                //Trace.WriteLine("load hand");
                loadhand = true;
            }

            //同时按下ctrl和空格处理
            if ((!pressBlank) || (!mouseDown))//Keyboard.IsKeyDown(Key.Space))
            {
                return;
            }

            Point mousePoint = e.GetPosition(canvasGrid);
            //Trace.WriteLine($"{mousePoint.X - startPoint.X}:{mousePoint.Y - startPoint.Y}");

            //writeBorad.EditingMode = System.Windows.Controls.InkCanvasEditingMode.None;

            var targetX = tranX + mousePoint.X - startPoint.X;
            var targetY = tranY + mousePoint.Y - startPoint.Y;

            ////Trace.WriteLine($"{targetX}:{targetY}");

            //边界检测

            //缩放后的尺寸
            var scaleWidth = writeBorad.ActualWidth * scaleSize;
            var scaleHeight = writeBorad.ActualHeight * scaleSize;
            //Trace.WriteLine($"{scaleWidth}:{scaleHeight}");

            var gridWidth = canvasGrid.ActualWidth;
            var gridHeight = canvasGrid.ActualHeight;

            //x轴最小x坐标
            var minX = -(scaleWidth - gridWidth);

            //y轴最小坐标
            var minY = -(scaleHeight - gridHeight);
            //Trace.WriteLine($"{minX}:{minX}");


            //x轴左侧坐标越界
            if (targetX > 0)
            {
                ////拖拽前在可拖动区域
                //if(trans.X<0)
                targetX = 0;
            }
            if (targetX < minX)
            {
                targetX = minX;
            }
            if (targetY > 0)
            {
                targetY = 0;
            }
            if (targetY < minY)
            {
                targetY = minY;
            }
            //Trace.WriteLine($"{targetX}:{targetY}");

            trans.X = targetX;
            trans.Y = targetY;

            adorner?.ReDraw();
        }
        private void writeBorad_MouseLeave(object sender, MouseEventArgs e)
        {
            movein = false;
        }
        //鼠标按下
        bool mouseDown;
        Point startPoint;
        double tranX;
        double tranY;

        InkCanvasEditingMode oldMode;
        private void writeBorad_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (writeBorad.EditingMode == InkCanvasEditingMode.None)
            {
                paths.Clear();
                writeBorad.CaptureMouse();
            }
            if (pressBlank)
            {
                oldMode = writeBorad.EditingMode;
                writeBorad.CaptureMouse();
                writeBorad.EditingMode = InkCanvasEditingMode.None;
            }

            startPoint = e.GetPosition(canvasGrid);// writeBorad.TranslatePoint(new Point(), canvasGrid);
            //Trace.WriteLine($"{startPoint.X}:{startPoint.Y}");

            mouseDown = true;
            tranX = trans.X;
            tranY = trans.Y;
        }

        private void writeBorad_MouseUp(object sender, MouseButtonEventArgs e)
        {
            lasso.ClearPoint();
            mouseDown = false;

            if (BlackMode == CanvasEditMode.WRITE_SHARP)
            {
                Draw d = new Draw(writeBorad);
                Point mousePoint = e.GetPosition(canvasGrid);

                //三角形
                //var stroke = d.Triangle(startPoint, mousePoint, new Point(mousePoint.X-300, mousePoint.Y));

                //矩形
                var stroke = d.Rectangle(startPoint, mousePoint);

                //椭圆
                //var stroke = d.Circle(new StylusPoint(startPoint.X, startPoint.Y), new StylusPoint(mousePoint.X, mousePoint.Y));
                //var stroke = d.Circle(startPoint, mousePoint);


                //var stroke = d.Line(startPoint, mousePoint);


                //writeBorad.Strokes.Clear();
                writeBorad.Strokes.Add(stroke);
                //清空
                previewAdorner.InjectRenderCallback(null);
                previewAdorner.ReDraw();

                return;
            }

            if (writeBorad.EditingMode == InkCanvasEditingMode.None)
            {
                writeBorad.ReleaseMouseCapture();
                writeBorad.EditingMode = oldMode;
                oldMode = InkCanvasEditingMode.None;
                Trace.WriteLine(paths.Count);

                var selectionStrokes = writeBorad.GetSelectedStrokes();


                foreach (var item in writeBorad.Strokes)
                {
                    var result = item.HitTest(paths, 1);
                    Trace.WriteLine(result);
                    if (result)
                    {
                        selectionStrokes.Add(item);
                    }
                }
                if (paths.Count == 0)
                {
                    return;
                }
                adorner = new MoveRotateAdorner(writeBorad, selectionStrokes, () =>
                 {
                     paths.Clear();
                 });

                if (selectionStrokes.Count == 0)
                {
                    adorner?.ClearAdorner();
                }
                else
                {
                    adorner?.ReDraw();
                }
            }

        }

        bool add = true;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (add)
            {
                add = false;
                var layer = AdornerLayer.GetAdornerLayer(addAdorner);
                layer.Add(new MoveRotateAdorner(addAdorner));
            }
            else
            {
                add = true;
                var layer = AdornerLayer.GetAdornerLayer(addAdorner);
                var arr = layer.GetAdorners(addAdorner);//获取该控件上所有装饰器，返回一个数组
                if (arr != null)
                {
                    for (int i = arr.Length - 1; i >= 0; i--)
                    {
                        layer.Remove(arr[i]);
                    }
                }
            }
        }

        private void canvasModeChange_Click(object sender, RoutedEventArgs e)
        {
            if (writeBorad.EditingMode == InkCanvasEditingMode.Ink)
            {
                writeBorad.EditingMode = InkCanvasEditingMode.Select;
            }
            else
            {
                writeBorad.EditingMode = InkCanvasEditingMode.Ink;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //重置画布选中笔迹后的装饰器
        }
        MoveRotateAdorner adorner;
        private void writeBorad_SelectionChanged(object sender, System.EventArgs e)
        {
            //writeBorad.EditingMode = InkCanvasEditingMode.None;

            //var selectionStrokes = writeBorad.GetSelectedStrokes();

            //adorner = new MoveRotateAdorner(writeBorad, selectionStrokes);

            //if (selectionStrokes.Count == 0)
            //{
            //    adorner?.ClearAdorner();
            //}
            //else
            //{
            //    adorner?.ReDraw();
            //}
        }

        private void btnRotate_Click(object sender, RoutedEventArgs e)
        {
            writeBorad.EditingMode = InkCanvasEditingMode.None;
        }

        private void writeBorad_SelectionMoved(object sender, System.EventArgs e)
        {
            adorner?.ReDraw();
        }
        private void writeBorad_SelectionResized(object sender, System.EventArgs e)
        {
            adorner?.ReDraw();
        }



        private void writeBorad_MouseMove_1(object sender, MouseEventArgs e)
        {

        }

        private void writeBorad_EditingModeChanged(object sender, RoutedEventArgs e)
        {
            switch (writeBorad.EditingMode)
            {
                case InkCanvasEditingMode.None:
                    break;
                case InkCanvasEditingMode.Ink:
                    BlackMode = CanvasEditMode.WRITE;
                    break;
                case InkCanvasEditingMode.GestureOnly:
                    break;
                case InkCanvasEditingMode.InkAndGesture:
                    break;
                case InkCanvasEditingMode.Select:
                    break;
                case InkCanvasEditingMode.EraseByPoint:
                    break;
                case InkCanvasEditingMode.EraseByStroke:
                    break;
                default:
                    break;
            }
        }

        private void btnEll_Click(object sender, RoutedEventArgs e)
        {
            BlackMode = CanvasEditMode.WRITE_SHARP;
            writeBorad.EditingMode = InkCanvasEditingMode.None;
        }
    }
}
