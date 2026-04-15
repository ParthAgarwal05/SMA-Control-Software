using System.ComponentModel;
using System.Windows.Media;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace SMAControlApp.Models
{
    public class SensorSelector : INotifyPropertyChanged
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public string Name { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        // ── NEW: per-sensor color ─────────────────────────────────────
        private Color _color = Colors.DeepSkyBlue;
        [NotMapped]
        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(Color)));
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(Brush)));
            }
        }

        // Convenience brush for XAML binding
        public SolidColorBrush Brush => new SolidColorBrush(_color);

        public event PropertyChangedEventHandler PropertyChanged;
    }
}