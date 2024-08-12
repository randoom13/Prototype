using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;

namespace Prototype.Main.ViewModels
{
    internal class ProgressViewModel : BindableBase
    {
        private void CalculatePercentage()
        {
            Percentage = Maximum == 0 ? string.Empty
                : (Current / Maximum).ToString("P", CultureInfo.InvariantCulture);
        }

        public string Percentage { private set; get; } = "";


        private double _minimum = 0;
        public double Minimum
        {
            get => _minimum;
            set
            {
                _minimum = value;
                RaisePropertyChanged(nameof(Minimum));
            }
        }
        private double _current;
        public double Current
        {
            get => _current;
            set
            {
                _current = value;
                CalculatePercentage();
                RaisePropertyChanged(nameof(Current));
                RaisePropertyChanged(nameof(Percentage));
            }
        }

        private double _maximum;
        public double Maximum
        {
            get => _maximum;
            set
            {
                _maximum = value;
                RaisePropertyChanged(nameof(Maximum));
            }
        }

        private string _description = "";
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                RaisePropertyChanged(nameof(Description));
            }
        }

        public void Reset() 
        {
            Maximum = 0; 
            Minimum = 0;
            Current = 0;
            Description = "";
        }
    }
}
