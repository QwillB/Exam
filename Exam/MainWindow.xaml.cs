using System.Windows;
using System.Windows.Input;

namespace Exam
{
    public partial class MainWindow : Window
    {
        private bool _isDragging = false;
        private Point _dragStartPoint;
        private Vector _offset;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isDragging = true;
                _dragStartPoint = e.GetPosition(this); // Получаем начальную позицию
                _offset = new Vector(Mouse.GetPosition(this).X, Mouse.GetPosition(this).Y); // Определяем смещение
                Mouse.Capture(this); // Захватываем мышь
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point mousePos = e.GetPosition(this);

                this.Left += mousePos.X - _dragStartPoint.X;
                this.Top += mousePos.Y - _dragStartPoint.Y;
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {

            if (e.ButtonState == MouseButtonState.Released)
            {
                _isDragging = false;
                Mouse.Capture(null);
            }
        }
    }
}