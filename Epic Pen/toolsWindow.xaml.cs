using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Epic_Pen
{
    /// <summary>
    /// Interaction logic for toolsWindow.xaml
    /// </summary>
    public partial class ToolsWindow : Window
    {

        enum DrawingMode {NONE, PEN, HIGHLIGHT, ERASE, RECT, ARROW, CIRCLE };
        DrawingMode mode = DrawingMode.NONE;

        InkCanvas inkCanvas;
        public ToolsWindow()
        {
            InitializeComponent();
        }

        public void setInkCanvas(InkCanvas _inkCanvas)
        { 
            inkCanvas = _inkCanvas;
            SelectInkColor(Color.FromRgb(50, 220, 50));
            inkCanvas.StrokeCollected += OnStrokeCollected;
        }

        public void OnStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            System.Windows.Ink.Stroke s = e.Stroke;
            if (mode == DrawingMode.RECT)
                StrokeRectConvert(s);
            else if (mode == DrawingMode.CIRCLE)
                StrokeCircleConvert(s);
            else if (mode == DrawingMode.ARROW)
                StrokeArrowConvert(s);
            
            //otherwise we do nothing as the stroke is added "as is".
        }

        private void StrokeRectConvert(System.Windows.Ink.Stroke stroke)
        {
            StylusPointCollection ptsRect = new StylusPointCollection();
            StylusPointCollection pts = stroke.StylusPoints;

            double minX = double.MaxValue;
            double minY = minX;
            double maxX = double.MinValue;
            double maxY = maxX;
            //find bounding area
            foreach (StylusPoint pt in pts)
            {
                if (pt.X > maxX)
                    maxX = pt.X;
                if (pt.X < minX)
                    minX = pt.X;

                if (pt.Y > maxY)
                    maxY = pt.Y;
                if (pt.Y < minY)
                    minY = pt.Y;
            }
            //stroke four corners of the rect
            ptsRect.Add(new StylusPoint(minX, minY));
            ptsRect.Add(new StylusPoint(minX, maxY));
            ptsRect.Add(new StylusPoint(maxX, maxY));
            ptsRect.Add(new StylusPoint(maxX, minY));
            ptsRect.Add(new StylusPoint(minX, minY));
            stroke.StylusPoints = ptsRect;

            //no smoothing
            stroke.DrawingAttributes.FitToCurve = false;
        }

        private void StrokeArrowConvert(System.Windows.Ink.Stroke stroke)
        {
            //Console.WriteLine("Stroke Completed");
            double toRadians = Math.PI / 180.0;
            StylusPointCollection ptsRect = new StylusPointCollection();
            StylusPointCollection pts = stroke.StylusPoints;

            StylusPoint pt1 = pts[pts.Count - 1];
            StylusPoint pt2 = pts[0];

            ptsRect.Add(pt1);
            ptsRect.Add(pt2);
            //compute arrow head
            double arrowAngle = 30.0 * toRadians;
            double deltaX = pt2.X - pt1.X;
            double deltaY = pt2.Y - pt1.Y;
            double theta = Math.Atan2(deltaY, deltaX); //radians
            double x1 = Math.Cos(theta + arrowAngle);
            double x2 = Math.Cos(theta - arrowAngle);
            double y1 = Math.Sin(theta + arrowAngle);
            double y2 = Math.Sin(theta - arrowAngle);
            double mag = 10.0; //arrorhead line length

            ptsRect.Add(new StylusPoint(pt2.X - mag * x1, pt2.Y - mag * y1));
            ptsRect.Add(new StylusPoint(pt2.X, pt2.Y));
            ptsRect.Add(new StylusPoint(pt2.X - mag * x2, pt2.Y - mag * y2));
            stroke.StylusPoints = ptsRect;
            stroke.DrawingAttributes.FitToCurve = false;
        }

        private void StrokeCircleConvert(System.Windows.Ink.Stroke stroke)
        {
            //Console.WriteLine("Stroke Completed");
            double toRadians = Math.PI / 180.0;
            StylusPointCollection ptsRect = new StylusPointCollection();
            StylusPointCollection pts = stroke.StylusPoints;

            //rather that try to redraw the circle, consider a diagonal line as indicator of circle center and diameter
            StylusPoint ptStart = pts[0];
            StylusPoint ptEnd = pts[pts.Count - 1];
            StylusPoint ptCenter = new StylusPoint((ptStart.X + ptEnd.X) / 2.0, (ptStart.Y + ptEnd.Y) / 2.0);

            double deltaX = ptEnd.X - ptCenter.X;
            double deltaY = ptEnd.Y - ptCenter.Y;
            double radius = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            for (double theta = 0; theta <= 360.0; theta += 10.0)
            {
                double angle = theta * toRadians;
                ptsRect.Add(new StylusPoint(ptCenter.X + radius * Math.Cos(angle), ptCenter.Y + radius * Math.Sin(angle)));
            }
            stroke.StylusPoints = ptsRect;
            stroke.DrawingAttributes.FitToCurve = false;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Color c = ((SolidColorBrush)((Border)sender).Background).Color;
            SelectInkColor(c);
        }

        private void SelectInkColor(Color color)
        {
            selectedColourBorder.Background = new SolidColorBrush(color);
            inkCanvas.DefaultDrawingAttributes.Color = color;
        }

        private new void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //System.Media.SystemSounds.Asterisk.Play();
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        public event EventHandler CloseButtonClick;

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            onCloseButtonClick();
        }

        void onCloseButtonClick()
        {
            if (CloseButtonClick != null)
                CloseButtonClick.Invoke(new object(), new EventArgs());
        }

        private void resetAllToolBackgrounds()
        {
            foreach (Button i in toolStackPanel.Children)
                i.Style = defaultButtonStyle;
        }

        public void cursorButton_Click(object sender, RoutedEventArgs e)
        {
            resetAllToolBackgrounds();
            cursorButton.Style = (Style)FindResource("highlightedButtonStyle");
        }
        public void penButton_Click(object sender, RoutedEventArgs e)
        {
            inkCanvas.Cursor = Cursors.Pen;
            inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            inkCanvas.DefaultDrawingAttributes.IsHighlighter = false;
            inkCanvas.DefaultDrawingAttributes.FitToCurve = true;
            
            setBrushSize();
            resetAllToolBackgrounds();
            penButton.Style = (Style)FindResource("highlightedButtonStyle");
            mode = DrawingMode.PEN;
        }

        public void highlighterButton_Click(object sender, RoutedEventArgs e)
        {
            inkCanvas.Cursor = Cursors.Pen;
            inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            inkCanvas.DefaultDrawingAttributes.IsHighlighter = true;
            setBrushSize();
            resetAllToolBackgrounds();
            highlighterButton.Style = (Style)FindResource("highlightedButtonStyle");
            mode = DrawingMode.HIGHLIGHT;
        }
        
        public void eraserButton_Click(object sender, RoutedEventArgs e)
        {
            inkCanvas.Cursor = Cursors.Cross;
            inkCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
            setBrushSize();
            resetAllToolBackgrounds();
            eraserButton.Style = (Style)FindResource("highlightedButtonStyle");
            mode = DrawingMode.ERASE;
        }
        
        public void eraseAllButton_Click(object sender, RoutedEventArgs e)
        {
            inkCanvas.Strokes.Clear();
        }

        public void rectButton_Click(object sender, RoutedEventArgs e)
        {
            inkCanvas.Cursor = Cursors.Pen;
            inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            inkCanvas.DefaultDrawingAttributes.IsHighlighter = false;
            inkCanvas.DefaultDrawingAttributes.FitToCurve = true;

            setBrushSize();
            resetAllToolBackgrounds();
            rectButton.Style = (Style)FindResource("highlightedButtonStyle");
            mode = DrawingMode.RECT;
        }

        public void arrowButton_Click(object sender, RoutedEventArgs e)
        {
            inkCanvas.Cursor = Cursors.Pen;
            inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            setBrushSize();
            resetAllToolBackgrounds();
            arrowButton.Style = (Style)FindResource("highlightedButtonStyle");
            mode = DrawingMode.ARROW;
        }

        public void circleButton_Click(object sender, RoutedEventArgs e)
        {
            inkCanvas.Cursor = Cursors.Pen;
            inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            setBrushSize();
            resetAllToolBackgrounds();
            circleButton.Style = (Style)FindResource("highlightedButtonStyle");
            mode = DrawingMode.CIRCLE;
        }

        double penSize=3;
        private void penSizeButton_MouseDown(object sender, RoutedEventArgs e)
        {
            penSize = ((Ellipse)((Button)sender).Content).Width;
            setBrushSize();

            foreach (Button i in brushSizeStackPanel.Children)
                i.Style = defaultButtonStyle;
            ((Button)sender).Style = (Style)FindResource("highlightedButtonStyle");   
        }

        private void setBrushSize()
        {
            if (inkCanvas.Cursor == Cursors.Cross)
            {
                inkCanvas.DefaultDrawingAttributes.Width = penSize * 5;
                inkCanvas.DefaultDrawingAttributes.Height = penSize * 5;
            }
            else
            {
                inkCanvas.DefaultDrawingAttributes.Width = penSize;
                inkCanvas.DefaultDrawingAttributes.Height = penSize;
            }
        }

        private void clickThroughCheckBox_Checked(object sender, RoutedEventArgs e)
        {

            if ((bool)hideInkCheckBox.IsChecked)
            {
                //toolsDockPanel.Height = 0;
                DoubleAnimation doubleAnimation = new DoubleAnimation();
                doubleAnimation.From = toolsDockPanelDefaultHeight;
                doubleAnimation.To = 0;
                doubleAnimation.Duration = new Duration(new TimeSpan(0,0,0,0,200));
                ExponentialEase expoEase = new ExponentialEase();
                expoEase.Exponent = 7;
                doubleAnimation.EasingFunction = expoEase;
                //Storyboard.SetTargetName(doubleAnimation, toolsDockPanel.Name);
                Storyboard.SetTarget(doubleAnimation, toolsDockPanel);
                Rectangle rect = new Rectangle();
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(DockPanel.HeightProperty));
                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(doubleAnimation);
                storyboard.Begin();
            }
            else
            {
                //toolsDockPanel.Height = double.NaN;
                DoubleAnimation doubleAnimation = new DoubleAnimation();
                doubleAnimation.From = 0;
                doubleAnimation.To = toolsDockPanelDefaultHeight;
                doubleAnimation.Duration = new Duration(new TimeSpan(0, 0, 0,0, 200));
                ExponentialEase expoEase = new ExponentialEase();
                expoEase.Exponent = 7;
                doubleAnimation.EasingFunction = expoEase;
                //Storyboard.SetTargetName(doubleAnimation, toolsDockPanel.Name);
                Storyboard.SetTarget(doubleAnimation, toolsDockPanel);
                Rectangle rect = new Rectangle();
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(DockPanel.HeightProperty));
                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(doubleAnimation);
                storyboard.Begin();
            }

        }
        Style defaultButtonStyle;
        double toolsDockPanelDefaultHeight;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            toolsDockPanel.Height = toolsDockPanel.ActualHeight;
            toolsDockPanelDefaultHeight = toolsDockPanel.Height;
            Height = ActualHeight;
            SizeToContent = System.Windows.SizeToContent.Manual;
            defaultButtonStyle = eraseAllButton.Style;
        }

    }
}
