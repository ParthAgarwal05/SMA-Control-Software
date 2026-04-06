using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace SMAControlApp.Models
{
    public class User : INotifyPropertyChanged
    {
        private string _userName;
        private string _passwordHash;

        [Key]
        public int UserId { get; set; }

        [Required]
        public string UserName
        {
            get => _userName;
            set { _userName = value; OnPropertyChanged(); }
        }

        [Required]
        public string PasswordHash
        {
            get => _passwordHash;
            set { _passwordHash = value; OnPropertyChanged(); }
        }

        // Navigation Properties
        public virtual ICollection<ActuatorChannel> Actuators { get; set; } = new List<ActuatorChannel>();
        public virtual Configuration Config { get; set; }
        public virtual ICollection<SensorSelector> SensorSelectors { get; set; } = new List<SensorSelector>();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}