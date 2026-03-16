using SMAControlApp.ViewModels;
using System.Windows.Controls;

namespace SMAControlApp.Views
{
    public partial class GraphView : UserControl
    {
        public GraphView()
        {
            InitializeComponent();
            DataContext = new GraphViewModel();
        }
    }
}