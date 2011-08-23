using System.Windows;

namespace StyleConfigDialog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class RectangleButtonLayoutDialog : Window
    {
        private readonly IClientDialogAdapter dialogAdapter;
        
        public RectangleButtonLayoutDialog(IClientDialogAdapter caller)
        {
            InitializeComponent();
            dialogAdapter = caller;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            dialogAdapter.DropDown = (bool) this.dropDownListRadio.IsChecked;
            dialogAdapter.PanelLayout = (bool) this.panelLayoutRadio.IsChecked;

            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.dropDownListRadio.IsChecked = dialogAdapter.DropDown;
            this.panelLayoutRadio.IsChecked = dialogAdapter.PanelLayout;    
        }

        private void dropDownListRadio_Checked(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
