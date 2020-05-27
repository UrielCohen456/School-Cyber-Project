using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for GameView.xaml
    /// </summary>
    public partial class GameView : UserControl
    {
        public GameView()
        {
            InitializeComponent();
            canvas.DefaultDrawingAttributes.Width = 10;
            canvas.DefaultDrawingAttributes.Height = 10;
        }

        private void ChangeColor(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var color = (Color)ColorConverter.ConvertFromString(btn.Background.ToString());

            canvas.DefaultDrawingAttributes.Color = color;
        }

        private void SwitchToPen(object sender, RoutedEventArgs e)
        {
            canvas.EditingMode = InkCanvasEditingMode.Ink;
        }

        private void SwitchToEraser(object sender, RoutedEventArgs e)
        {
            canvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
        }

        private void SwitchPenSize(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var size = int.Parse(btn.Tag.ToString());

            canvas.DefaultDrawingAttributes.Width = size * 5;
            canvas.DefaultDrawingAttributes.Height = size * 5;
        }

        private void ClearBoard(object sender, RoutedEventArgs e)
        {
            canvas.Strokes.Clear();
        }

    }
}
