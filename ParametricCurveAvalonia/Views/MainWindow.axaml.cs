using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using System;
using System.Globalization;

namespace ParametricCurveAvalonia
{
    public partial class MainWindow : Window
    {
        private float R = 1.0f;
        private float r = 0.5f;
        private float tMin = 0.0f;
        private float tMax = (float)(2 * Math.PI);
        private const int steps = 1000;
        
        private readonly IPen axisPen = new Pen(Brushes.Black, 2);
        private readonly IPen gridPen = new Pen(Brushes.LightGray, 1);
        private readonly IPen curvePen = new Pen(Brushes.Red, 2);
        
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            // Додамо обробник події для завантаження вікна
            this.Opened += (s, e) => DrawPlot();
        }
        
        private void InitializeComponent()
        {
            Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
        }
        
        private async void OnPlotButtonClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                if (float.TryParse(RValue.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float newR) &&
                    float.TryParse(rValue.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float newr) &&
                    float.TryParse(tMinValue.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float newTMin) &&
                    float.TryParse(tMaxValue.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float newTMax))
                {
                    R = newR;
                    r = newr;
                    tMin = newTMin;
                    tMax = newTMax;
                    DrawPlot();
                }
                else
                {
                    var dialog = new Window()
                    {
                        Title = "Помилка",
                        Content = new TextBlock { 
                            Text = "Будь ласка, введіть коректні числові значення.", 
                            Margin = new Thickness(20),
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                        },
                        SizeToContent = SizeToContent.WidthAndHeight,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        CanResize = false
                    };
                    
                    await dialog.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                var dialog = new Window()
                {
                    Title = "Помилка",
                    Content = new TextBlock { 
                        Text = $"Сталася помилка: {ex.Message}", 
                        Margin = new Thickness(20),
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    },
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    CanResize = false
                };
                
                await dialog.ShowDialog(this);
            }
        }
        
        private void DrawPlot()
        {
            var canvas = this.FindControl<Canvas>("PlotCanvas");
            if (canvas == null) return;
            
            canvas.Children.Clear();
            
            double width = canvas.Bounds.Width;
            double height = canvas.Bounds.Height;
            
            if (width <= 0 || height <= 0) return;
            
            // Визначаємо масштаб та центр
            double scale = Math.Min(width, height) / 6.0;
            Point center = new Point(width / 2, height / 2);
            
            // Малюємо сітку та осі
            DrawGrid(canvas, width, height, center, scale);
            
            // Малюємо криву
            DrawParametricCurve(canvas, center, scale);
        }
        
        private void DrawGrid(Canvas canvas, double width, double height, Point center, double scale)
{
    // Вертикальні лінії сітки (ліворуч від осі Y)
    for (double x = -4; x <= 0; x += 0.5)
    {
        double screenX = center.X + x * scale;
        var line = new Avalonia.Controls.Shapes.Line
        {
            StartPoint = new Point(screenX, 0),
            EndPoint = new Point(screenX, height),
            Stroke = gridPen.Brush,
            StrokeThickness = gridPen.Thickness
        };
        canvas.Children.Add(line);
    }

    // Вертикальні лінії сітки (праворуч від осі Y, але тільки до X=4)
    for (double x = 0.5; x <= 4; x += 0.5)
    {
        double screenX = center.X + x * scale;
        var line = new Avalonia.Controls.Shapes.Line
        {
            StartPoint = new Point(screenX, 0),
            EndPoint = new Point(screenX, height),
            Stroke = gridPen.Brush,
            StrokeThickness = gridPen.Thickness
        };
        canvas.Children.Add(line);
    }

    // Горизонтальні лінії сітки
    for (double y = -10; y <= 10; y += 0.5)
    {
        double screenY = center.Y - y * scale;
        var line = new Avalonia.Controls.Shapes.Line
        {
            StartPoint = new Point(center.X - 4 * scale, screenY), // Починаємо з X=-4
            EndPoint = new Point(center.X + 4 * scale, screenY),  // Закінчуємо на X=4
            Stroke = gridPen.Brush,
            StrokeThickness = gridPen.Thickness
        };
        canvas.Children.Add(line);
    }

    // Вісь X (від X=-4 до X=4)
    var xAxis = new Avalonia.Controls.Shapes.Line
    {
        StartPoint = new Point(center.X - 4 * scale, center.Y),
        EndPoint = new Point(center.X + 4 * scale, center.Y),
        Stroke = axisPen.Brush,
        StrokeThickness = axisPen.Thickness
    };
    canvas.Children.Add(xAxis);

    // Вісь Y (повна висота)
    var yAxis = new Avalonia.Controls.Shapes.Line
    {
        StartPoint = new Point(center.X, 0),
        EndPoint = new Point(center.X, height),
        Stroke = axisPen.Brush,
        StrokeThickness = axisPen.Thickness
    };
    canvas.Children.Add(yAxis);

    // Позначки на осі X (від X=-4 до X=4)
    for (double x = -4; x <= 4; x += 1.0)
    {
        double screenX = center.X + x * scale;
        var tick = new Avalonia.Controls.Shapes.Line
        {
            StartPoint = new Point(screenX, center.Y - 5),
            EndPoint = new Point(screenX, center.Y + 5),
            Stroke = axisPen.Brush,
            StrokeThickness = axisPen.Thickness
        };
        canvas.Children.Add(tick);
        
        var label = new TextBlock
        {
            Text = x.ToString("0.0"),
            FontSize = 10,
            Foreground = Brushes.Black,
            [Canvas.LeftProperty] = screenX - 10,
            [Canvas.TopProperty] = center.Y + 10
        };
        canvas.Children.Add(label);
    }

    // Позначки на осі Y
    for (double y = -5; y <= 5; y += 1.0)
    {
        double screenY = center.Y - y * scale;
        var tick = new Avalonia.Controls.Shapes.Line
        {
            StartPoint = new Point(center.X - 5, screenY),
            EndPoint = new Point(center.X + 5, screenY),
            Stroke = axisPen.Brush,
            StrokeThickness = axisPen.Thickness
        };
        canvas.Children.Add(tick);
        
        var label = new TextBlock
        {
            Text = y.ToString("0.0"),
            FontSize = 10,
            Foreground = Brushes.Black,
            [Canvas.LeftProperty] = center.X + 10,
            [Canvas.TopProperty] = screenY - 8
        };
        canvas.Children.Add(label);
    }

    // Підписи осей
    var xLabel = new TextBlock
    {
        Text = "X",
        FontSize = 12,
        Foreground = Brushes.Black,
        [Canvas.LeftProperty] = center.X + 4 * scale - 20,
        [Canvas.TopProperty] = center.Y + 10
    };
    canvas.Children.Add(xLabel);
    
    var yLabel = new TextBlock
    {
        Text = "Y",
        FontSize = 12,
        Foreground = Brushes.Black,
        [Canvas.LeftProperty] = center.X + 10,
        [Canvas.TopProperty] = 10
    };
    canvas.Children.Add(yLabel);
}
        
        private void DrawParametricCurve(Canvas canvas, Point center, double scale)
{
    if (Math.Abs(R) < 0.0001f) return; // Уникаємо ділення на нуль
    
    float m = r / R;
    Point? prevPoint = null;
    bool shouldStopDrawing = false;
    
    for (int i = 0; i <= steps && !shouldStopDrawing; i++)
    {
        float t = tMin + (tMax - tMin) * i / steps;
        
        // Обчислюємо координати за параметричними рівняннями
        float x = (R + m * R) * (float)Math.Cos(m * t) - m * (float)Math.Cos(t + m * t);
        float y = (R + m * R) * (float)Math.Sin(m * t) - m * (float)Math.Sin(t + m * t);
        
        // Перевіряємо, чи x не перевищує 4
        if (x > 4.0f)
        {
            shouldStopDrawing = true;
            continue;
        }
        
        // Перетворюємо в координати екрана
        Point currentPoint = new Point(
            center.X + x * scale,
            center.Y - y * scale);
        
        // Малюємо лінію від попередньої точки до поточної
        if (prevPoint != null)
        {
            var line = new Avalonia.Controls.Shapes.Line
            {
                StartPoint = prevPoint.Value,
                EndPoint = currentPoint,
                Stroke = curvePen.Brush,
                StrokeThickness = curvePen.Thickness
            };
            canvas.Children.Add(line);
        }
        
        prevPoint = currentPoint;
    }
}
    }
}