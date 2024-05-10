using System.Windows;
using WorkpieceCreator.MVVM.ViewModels;

namespace WorkpieceCreator.MVVM.Views {
    public partial class MainWindow : Window {

        public MainViewModel ViewModel { get; }

        /// <summary>
        /// Init the MainViewModel, Datacontext and register the ViewModels OnRequestClose Event.
        /// </summary>
        public MainWindow() {
            ViewModel = new MainViewModel();
            DataContext = this;
            ViewModel.OnRequestClose += (s, e) => Close();

            InitializeComponent();
        }
    }
}
