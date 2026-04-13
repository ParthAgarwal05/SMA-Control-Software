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

        // ── Status & Save button visibility ───────────────────────────
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
        private bool _graphStarted = false;
        private bool _graphRunning = false;

        // Per-actuator data (rebuilt when actuator count changes)
        private List<List<double>> _allValues = new();
        private int[] _startTick = Array.Empty<int>();
        private double[] _lastValue = Array.Empty<double>();

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
            Series = new SeriesCollection();
            SensorSelectors = new ObservableCollection<SensorSelector>();

            BuildSeriesFromActuators();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += OnTick;
            _timer.Start();
        }

        // ── Build / rebuild series to match current App.Actuators ─────

        // Set by SyncActuatorsWithDatabase() — defers rebuild until GraphView is on screen.
        // LiveCharts calls LineSeries.Erase() on any Series removal, which null-refs
        // when the chart canvas is not in the visual tree.
        public bool NeedsRebuild { get; private set; } = false;

        // Called by SyncActuatorsWithDatabase() — just marks dirty, never touches Series.
        public void RebuildSeries()
        {
            NeedsRebuild = true;
        }

        // Called by GraphView.Loaded — safe to mutate Series here because chart is on screen.
        public void ApplyRebuildIfNeeded()
        {
            if (!NeedsRebuild) return;
            NeedsRebuild = false;

            var colorMap = new Dictionary<string, Color>();
            for (int i = 0; i < SensorSelectors.Count; i++)
                colorMap[SensorSelectors[i].Name] = SensorSelectors[i].Color;

            ResetData(clearStatus: false);
            Series.Clear();
            SensorSelectors.Clear();

            BuildSeriesFromActuators(colorMap);
        }

        private void BuildSeriesFromActuators(Dictionary<string, Color>? colorMap = null)
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

            for (int i = 0; i < count; i++)
            {
                var actuator = App.Actuators[i];
                var name = $"Sensor {actuator.ChannelId}";

                // Reuse saved color if available, otherwise pick from palette
                var color = (colorMap != null && colorMap.TryGetValue(name, out var saved))
                            ? saved
                            : Palette[i % Palette.Length];

                Series.Add(new LineSeries
                {
                    Title = name,
                    Values = new ChartValues<double>(),
                    PointGeometry = null,
                    StrokeThickness = 2,
                    Stroke = new SolidColorBrush(color),
                    Fill = Brushes.Transparent,
                    Visibility = Visibility.Visible
                });

                var selector = new SensorSelector
                {
                    Name = name,
                    IsSelected = true,
                    Color = color
                };
                selector.PropertyChanged += OnSensorSelectorChanged;
                SensorSelectors.Add(selector);
            }
        }

        // ── Every second ─────────────────────────────────────────────
        private void OnTick(object? sender, EventArgs e)
        {
            int count = App.Actuators.Count;
            bool anyRunning = false;

            for (int i = 0; i < count; i++)
                if (App.Actuators[i].IsRunning) { anyRunning = true; break; }

            // Auto-reset when a new run starts after previous completed
            if (_graphStarted && !_graphRunning && anyRunning)
                ResetData();

            // Wait for first actuator to start
            if (!_graphStarted)
            {
                if (!anyRunning) return;
                _graphStarted = true;
                _graphRunning = true;
                StatusText = "Graph running…";
                ShowSaveButton = false;
            }

            // All actuators went idle → stop plotting
            if (_graphStarted && !anyRunning && _graphRunning)
            {
                _graphRunning = false;
                StatusText = "Graph stopped – all actuators idle.";
                ShowSaveButton = true;
                return;
            }

            if (!_graphRunning) return;

            // Advance tick and record one point per actuator
            _globalTick++;

            for (int i = 0; i < count; i++)
            {
                var actuator = App.Actuators[i];

                if (actuator.IsRunning && _startTick[i] < 0)
                    _startTick[i] = _globalTick;

                double plotValue;

                if (_startTick[i] < 0)
                    plotValue = 0;                   // not started yet → baseline
                else if (actuator.IsRunning)
                {
                    plotValue = actuator.CurrentDisplacement;
                    _lastValue[i] = plotValue;
                }
                else
                    plotValue = _lastValue[i];       // stopped → flat line

                _allValues[i].Add(plotValue);
                ((LineSeries)Series[i]).Values.Add(plotValue);
            }
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

            // Header row
            sb.Append("Time (s)");
            for (int i = 0; i < count; i++)
                sb.Append($",{SensorSelectors[i].Name}");
            sb.AppendLine();

            // Data rows
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

        // ── Reset data (keeps colors/selectors intact) ────────────────
        private void ResetData(bool clearStatus = true)
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

            if (clearStatus)
                StatusText = "Waiting for actuators…";
        }

        // ── Public reset (Reset button) ───────────────────────────────
        public void Reset() => ResetData();

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}