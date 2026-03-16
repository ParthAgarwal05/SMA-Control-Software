using System.Collections.ObjectModel;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Wpf;
using SMAControlApp.Models;
using System.ComponentModel;
using System.Windows;

namespace SMAControlApp.ViewModels
{
    public class GraphViewModel
    {
        public SeriesCollection Series { get; set; }
        public ObservableCollection<SensorSelector> SensorSelectors { get; set; }

        private DispatcherTimer _timer;

        public GraphViewModel()
        {
            Series = new SeriesCollection();
            SensorSelectors = new ObservableCollection<SensorSelector>();

            foreach (var actuator in App.Actuators)
            {
                Series.Add(new LineSeries
                {
                    Title = $"Sensor {actuator.ChannelId}",
                    Values = new ChartValues<double> { 0 }, // start at 0
                    PointGeometry = null
                });

                var selector = new SensorSelector
                {
                    Name = $"Sensor {actuator.ChannelId}",
                    IsSelected = true
                };

                // listen to checkbox changes
                selector.PropertyChanged += SensorSelectionChanged;

                SensorSelectors.Add(selector);
            }

            _timer = new DispatcherTimer();
            _timer.Interval = System.TimeSpan.FromSeconds(1);
            _timer.Tick += UpdateGraph;
            _timer.Start();
        }

        // called when checkbox changes
        private void SensorSelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(SensorSelector.IsSelected))
                return;

            UpdateVisibility();
        }

        // controls which graphs are visible
        private void UpdateVisibility()
        {
            for (int i = 0; i < Series.Count; i++)
            {
                var series = (LineSeries)Series[i];

                series.Visibility = SensorSelectors[i].IsSelected
                    ? Visibility.Visible
                    : Visibility.Hidden;
            }
        }

        private void UpdateGraph(object sender, System.EventArgs e)
        {
            bool anyRunning = false;

            for (int i = 0; i < App.Actuators.Count; i++)
            {
                var actuator = App.Actuators[i];
                var series = (LineSeries)Series[i];

                // always update backend data
                series.Values.Add(actuator.CurrentDisplacement);

                if (series.Values.Count > 50)
                    series.Values.RemoveAt(0);

                if (actuator.IsRunning)
                    anyRunning = true;
            }

            if (!anyRunning)
                _timer.Stop();
        }
    }
}