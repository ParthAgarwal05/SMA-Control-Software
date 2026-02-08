using SMAControlApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SMAControlApp.Views
{
    /// <summary>
    /// Interaction logic for ClosedLoopView.xaml
    /// </summary>
    public partial class ClosedLoopView : UserControl
    {
        public ClosedLoopView()
        {
            InitializeComponent();
            DataContext = new ClosedLoopViewModel();
        }
        private void ApplyAll_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ClosedLoopViewModel;
            vm?.ApplyDisplacementToAll();
        }

    }
}
