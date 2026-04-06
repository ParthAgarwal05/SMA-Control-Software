using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Wpf;
using SMAControlApp.Models;

namespace SMAControlApp.ViewModels
{
    public class GraphViewModel : INotifyPropertyChanged
    {
        // ── Public bindings ───────────────────────────────────────────
        public SeriesCollection Series { get; set; }
        public ObservableCollection<SensorSelector> SensorSelectors { get; set; }

        // ── Status & Save button ──────────────────────────────────────
        private string _statusText = "Waiting for actuators…";
        public string StatusText
        {
            get => _statusText;
            private set { _statusText = value; OnPropertyChanged(nameof(StatusText)); }
        }

        private bool _showSaveButton = false;
        public bool ShowSaveButton
        {
            get => _showSaveButton;
            private set { _showSaveButton = value; OnPropertyChanged(nameof(ShowSaveButton)); }
        }

        // ── Internals ─────────────────────────────────────────────────
        private readonly DispatcherTimer _timer;
        private int _globalTick = 0;
        private bool _graphStarted = false;  // true once any actuator has ever run
        private bool _graphRunning = false;  // true while actively plotting

        // Full history per actuator (no cap)
        private readonly List<List<double>> _allValues;
        private readonly int[] _startTick;
        private readonly double[] _lastValue;

        private static readonly Color[] Palette =
        {
            Colors.DeepSkyBlue,  Colors.OrangeRed,    Colors.LimeGreen,
            Colors.Gold,         Colors.MediumOrchid, Colors.Cyan,
            Colors.HotPink,      Colors.Coral,        Colors.Aquamarine,
            Colors.DodgerBlue,   Colors.Tomato,       Colors.Violet
        };

        // ── Constructor ───────────────────────────────────────────────
        public GraphViewModel()
        {
            int count = App.Actuators.Count;

            _startTick = new int[count];
            _lastValue = new double[count];
            _allValues = new List<List<double>>();

            for (int i = 0; i < count; i++)
            {
                _startTick[i] = -1;
                _lastValue[i] = 0;
                _allValues.Add(new List<double>());
            }

            Series = new SeriesCollection();
            SensorSelectors = new ObservableCollection<SensorSelector>();

            for (int i = 0; i < count; i++)
            {
                var actuator = App.Actuators[i];
                var color = Palette[i % Palette.Length];

                Series.Add(new LineSeries
                {
                    Title = $"Sensor {actuator.ChannelId}",
                    Values = new ChartValues<double>(),
                    PointGeometry = null,
                    StrokeThickness = 2,
                    Stroke = new SolidColorBrush(color),
                    Fill = Brushes.Transparent,
                    Visibility = Visibility.Visible
                });

                var selector = new SensorSelector
                {
                    Name = $"Sensor {actuator.ChannelId}",
                    IsSelected = true,
                    Color = color
                };
                selector.PropertyChanged += OnSensorSelectorChanged;
                SensorSelectors.Add(selector);
            }

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += OnTick;
            _timer.Start();
        }

        // ── Every second ─────────────────────────────────────────────
        private void OnTick(object? sender, EventArgs e)
        {
            int count = App.Actuators.Count;
            bool anyRunning = false;

            for (int i = 0; i < count; i++)
                if (App.Actuators[i].IsRunning) { anyRunning = true; break; }

            // ── Auto-reset when a new run starts after a previous one finished ──
            // Condition: graph was run before (_graphStarted), it has since stopped
            // (_graphRunning = false), but now something is running again.
            if (_graphStarted && !_graphRunning && anyRunning)
                ResetData();

            // ── Wait for first actuator ───────────────────────────────
            if (!_graphStarted)
            {
                if (!anyRunning) return;
                _graphStarted = true;
                _graphRunning = true;
                StatusText = "Graph running…";
                ShowSaveButton = false;
            }

            // ── All actuators went idle → stop plotting ───────────────
            if (_graphStarted && !anyRunning && _graphRunning)
            {
                _graphRunning = false;
                StatusText = "Graph stopped – all actuators idle.";
                ShowSaveButton = true;
                return;
            }

            if (!_graphRunning) return;

            // ── Advance tick and record values ────────────────────────
            _globalTick++;

            for (int i = 0; i < count; i++)
            {
                var actuator = App.Actuators[i];

                if (actuator.IsRunning && _startTick[i] < 0)
                    _startTick[i] = _globalTick;

                double plotValue;

                if (_startTick[i] < 0)
                {
                    // Not started yet → baseline 0 on shared time axis
                    plotValue = 0;
                }
                else if (actuator.IsRunning)
                {
                    plotValue = actuator.CurrentDisplacement;
                    _lastValue[i] = plotValue;
                }
                else
                {
                    // Stopped → flat line
                    plotValue = _lastValue[i];
                }

                _allValues[i].Add(plotValue);
                ((LineSeries)Series[i]).Values.Add(plotValue);
            }
        }

        // ── Clears plot data only (keeps colors/selectors intact) ────
        private void ResetData()
        {
            _globalTick = 0;
            _graphStarted = false;
            _graphRunning = false;
            ShowSaveButton = false;

            for (int i = 0; i < _startTick.Length; i++)
            {
                _startTick[i] = -1;
                _lastValue[i] = 0;
                _allValues[i].Clear();
            }

            foreach (var s in Series)
                ((LineSeries)s).Values.Clear();

            StatusText = "Waiting for actuators…";
        }

        // ── Checkbox / color ──────────────────────────────────────────
        private void OnSensorSelectorChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SensorSelector.IsSelected))
                for (int i = 0; i < Series.Count; i++)
                    ((LineSeries)Series[i]).Visibility = SensorSelectors[i].IsSelected
                        ? Visibility.Visible : Visibility.Hidden;

            if (e.PropertyName == nameof(SensorSelector.Color))
                for (int i = 0; i < Series.Count; i++)
                    ((LineSeries)Series[i]).Stroke = SensorSelectors[i].Brush;
        }

        // ── Save to CSV ───────────────────────────────────────────────
        public void SaveToCsv()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Save Graph Data",
                Filter = "CSV files (*.csv)|*.csv",
                FileName = "GraphData.csv"
            };

            if (dlg.ShowDialog() != true) return;

            int count = App.Actuators.Count;
            int maxTicks = 0;
            for (int i = 0; i < count; i++)
                if (_allValues[i].Count > maxTicks)
                    maxTicks = _allValues[i].Count;

            var sb = new StringBuilder();

            // Header
            sb.Append("Time (s)");
            for (int i = 0; i < count; i++)
                sb.Append($",{SensorSelectors[i].Name}");
            sb.AppendLine();

            // Rows
            for (int t = 0; t < maxTicks; t++)
            {
                sb.Append(t + 1);
                for (int i = 0; i < count; i++)
                {
                    double val = t < _allValues[i].Count ? _allValues[i][t] : 0;
                    sb.Append($",{val}");
                }
                sb.AppendLine();
            }

            File.WriteAllText(dlg.FileName, sb.ToString());
            StatusText = $"Saved → {Path.GetFileName(dlg.FileName)}";
        }

        // ── Manual reset (Reset button) ───────────────────────────────
        public void Reset() => ResetData();

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}