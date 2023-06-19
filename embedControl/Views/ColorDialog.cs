using System;
using embedCONTROL.Services;

namespace embedCONTROL.Views
{
    public class ColorDialog : ContentPage
    {
        private static readonly Color[] DefaultColors = new Color[]
        {
            Colors.Black, Colors.White, Colors.Gray, Colors.DarkGray, Colors.LightGray, Colors.Azure, Colors.Aquamarine, Colors.Beige,
            Colors.Purple, Colors.MediumPurple, Colors.BlueViolet, Colors.Blue, Colors.LightBlue, Colors.CadetBlue, Colors.CornflowerBlue, Colors.AliceBlue,
            Colors.Green, Colors.GreenYellow, Colors.LightYellow, Colors.Red, Colors.Orange, Colors.OrangeRed, Colors.LightCoral, Colors.Bisque
        };

        public Color CurrentColor { get; set; }
        private readonly Frame _backgroundFrame;
        private readonly Action<Color> _colorChangeNotification;
        private readonly Slider _redSlider;
        private readonly Slider _greenSlider;
        private readonly Slider _blueSlider;
        private bool _settingValue;
        private int _row;
        private readonly Grid _displayGrid;

        public ColorDialog(Color initialColor, string colorName, Action<Color> changeNotification)
        {
            BackgroundColor = Colors.Transparent;
            CurrentColor = initialColor;
            _colorChangeNotification = changeNotification;
            Title = "Change " + colorName + " color";

            _displayGrid = new Grid
            {
                Padding = new Thickness(10),
                ColumnSpacing = 4,
                RowSpacing = 4
            };

            for (var i = 0; i < 8; i++)
            {
                _displayGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            }

            _backgroundFrame = new Frame
            {
                BackgroundColor = CurrentColor,
                BorderColor = Colors.Black
            };

            AddToGrid(_displayGrid, _backgroundFrame, IncrementRow(1), 0, 1, 8);

            AddToGrid(_displayGrid, new Label { Text = "Default Color Selections" }, IncrementRow(1), 0, 1, 8);

            for (var col = 0; col < 8; col++)
            {
                var btn = new Button { BackgroundColor = DefaultColors[col], HeightRequest = 32 };
                btn.Clicked += OnColorSelected;
                AddToGrid(_displayGrid, btn, _row, col);

                btn = new Button { BackgroundColor = DefaultColors[col + 8], HeightRequest = 32 };
                btn.Clicked += OnColorSelected;
                AddToGrid(_displayGrid, btn, _row + 1, col);

                btn = new Button { BackgroundColor = DefaultColors[col + 16], HeightRequest = 32 };
                btn.Clicked += OnColorSelected;
                AddToGrid(_displayGrid, btn, _row + 2, col);

            }

            IncrementRow(3);

            AddToGrid(_displayGrid, new Label { Text = "Custom Color Selection" }, IncrementRow(1), 0, 1, 8);


            _redSlider = new Slider(0, 255, CurrentColor.Red * 255.0);
            _greenSlider = new Slider(0, 255, CurrentColor.Green * 255.0);
            _blueSlider = new Slider(0, 255, CurrentColor.Blue * 255.0);

            _redSlider.ValueChanged += SliderValueChanged;
            _greenSlider.ValueChanged += SliderValueChanged;
            _blueSlider.ValueChanged += SliderValueChanged;

            AddToGrid(_displayGrid, _redSlider, _row, 2, 1, 6);
            AddToGrid(_displayGrid, new Label { Text = "Red" }, IncrementRow(1), 0, 1, 2);
            AddToGrid(_displayGrid, _greenSlider, _row, 2, 1, 6);
            AddToGrid(_displayGrid, new Label { Text = "Green" }, IncrementRow(1), 0, 1, 2);
            AddToGrid(_displayGrid, _blueSlider, _row, 2, 1, 6);
            AddToGrid(_displayGrid, new Label { Text = "Blue" }, IncrementRow(1), 0, 1, 2);

            var okBtn = new Button { Text = "Change Color" };
            okBtn.Clicked += OnColorChosen;
            AddToGrid(_displayGrid, okBtn, IncrementRow(1), 0, 1, 8);

            var cancelBtn = new Button { Text = "Keep Original Color" };
            cancelBtn.Clicked += OnCancel;
            AddToGrid(_displayGrid, cancelBtn, IncrementRow(1), 0, 1, 8);

            Content = _displayGrid;
        }

        private int IncrementRow(int number)
        {

            for (var i = 0; i < number; i++)
            {
                _displayGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            var oldRow = _row;
            _row += number;
            return oldRow;

        }

        private void SliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (_settingValue) return;

            CurrentColor = new Color(Convert.ToByte(_redSlider.Value), Convert.ToByte(_greenSlider.Value), Convert.ToByte(_blueSlider.Value));
            _backgroundFrame.BackgroundColor = CurrentColor;
        }

        private void OnCancel(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private void OnColorChosen(object sender, EventArgs e)
        {
            Navigation.PopAsync();
            _colorChangeNotification?.Invoke(CurrentColor);
        }

        private void OnColorSelected(object sender, EventArgs e)
        {
            CurrentColor = ((Button)sender).BackgroundColor;
            _backgroundFrame.BackgroundColor = CurrentColor;
            _settingValue = true;
            _redSlider.Value = CurrentColor.Red * 255.0;
            _greenSlider.Value = CurrentColor.Green * 255.0;
            _blueSlider.Value = CurrentColor.Blue * 255.0;
            _settingValue = false;
        }

        private void AddToGrid(Grid theGrid, View item, int row, int col, int rowSpan = 1, int colSpan = 1)
        {
            Grid.SetRow(item, row);
            Grid.SetColumn(item, col);
            Grid.SetColumnSpan(item, colSpan);
            Grid.SetRowSpan(item, rowSpan);
            theGrid.Children.Add(item);
        }
    }

}
