using SafeMotionOnOperation.MVVM.ViewModels;
using System.Windows;

namespace SafeMotionOnOperation.MVVM.Views {
    public partial class MainWindow : Window {

        public MainViewModel ViewModel { get; } = new MainViewModel();

        public MainWindow() {
            DataContext = this;
            ViewModel.OnRequestClose += (s, e) => Close();

            InitializeComponent();
        }
    }
}
