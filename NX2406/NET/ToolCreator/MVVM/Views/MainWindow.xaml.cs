using System.Windows;
using ToolCreator.MVVM.ViewModels;

namespace ToolCreator.MVVM.Views {
    public partial class MainWindow : Window {

        public MainViewModel ViewModel { get; } = new MainViewModel();

        /// <summary>
        /// Initialisiert das ViewModel, den DataContext und registriert das OnRequestClose-Event.
        /// </summary>
        public MainWindow() {
            DataContext = this;
            ViewModel.OnRequestClose += (s, e) => Close();

            InitializeComponent();
        }
    }
}
