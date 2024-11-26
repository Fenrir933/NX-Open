using ProgramOrder.MVVM.ViewModels;
using System.Windows;

namespace ProgramOrder.MVVM.Views {
    public partial class MainWindow : Window {

        public MainViewModel ViewModel { get; }

        /// <summary>
        /// Init the ViewModel, Datacontext and register the ViewModels OnRequestClose Event.
        /// </summary>
        public MainWindow() {
            ViewModel = new MainViewModel();
            DataContext = this;
            ViewModel.OnRequestClose += (s, e) => Close();

            InitializeComponent();
        }
    }
}
