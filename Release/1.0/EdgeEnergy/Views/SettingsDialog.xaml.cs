using System.Windows;
using System.Windows.Media;
using System.ComponentModel;


namespace EdgeEnergy.CutterDashboard.Views
{
    public partial class SettingsDialog : System.Windows.Window
    {

        // Data for the dialog that supports notification for data binding
        class DialogData : INotifyPropertyChanged
        {
            string _ftpHost;
            public string FtpHost
            {
                get { return _ftpHost; }
                set { _ftpHost = value; Notify("FtpHost"); }
            }

            // INotifyPropertyChanged Members
            public event PropertyChangedEventHandler PropertyChanged;
            void Notify(string prop) { if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); } }
        }

        readonly DialogData _data = new DialogData();

        public string FtpHost
        {
            get { return _data.FtpHost; }
            set { _data.FtpHost = value; }
        }

        public SettingsDialog()
        {
            InitializeComponent();

            // Allow binding to the data to keep UI bindings up to date
            DataContext = _data;

            okButton.Click += OkButtonClick;
        }

        void OkButtonClick(object sender, RoutedEventArgs e)
        {
            // the return from ShowDialog will be true
            DialogResult = true;

            // no need to explicitly call this method
            // when DialogResult transitions to non-null
            //Close();
        }

        // Don't need this with IsCancel set
        //void cancelButton_Click(object sender, RoutedEventArgs e) {
        //  // the return from ShowDialog will be false
        //  Close();
        //}


    }
}