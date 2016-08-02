using System.Windows;

namespace EdgeEnergy.CutterDashboard.Views
{
    /// <summary>
    /// Interaction logic for Splash.xaml
    /// </summary>
    public partial class Splash : Window
    {
        //private static readonly Splash splash = new Splash();

        // To refresh the UI immediately
        private delegate void RefreshDelegate();
        private static void Refresh(DependencyObject obj)
        {
            obj.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render,
                (RefreshDelegate)delegate { });
        }

        public Splash()
        {
            InitializeComponent();
        }

        public void BeginDisplay()
        {
            //ShowDialog();
            Show();
            Loading(string.Empty);
        }

        public void EndDisplay()
        {
            Hide();
            //splash.Close();
        }

        public void Loading(string test)
        {
            StatusLabel.Content = test;
            Refresh(StatusLabel);
        }

    }
}
