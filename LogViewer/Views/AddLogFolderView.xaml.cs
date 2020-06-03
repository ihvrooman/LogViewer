using LogViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LogViewer.Views
{
    /// <summary>
    /// Interaction logic for AddLogFolderView.xaml
    /// </summary>
    public partial class AddLogFolderView : Window
    {
        public AddLogFolderViewModel ViewModel
        {
            get
            {
                return (AddLogFolderViewModel)DataContext;
            }
        }

        public AddLogFolderView()
        {
            InitializeComponent();
            DataContext = new AddLogFolderViewModel();
            AddLogFolderTextbox.Focus();
        }

        private void AddLogFolderTextbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                AddLogFolderButton.Focus();
            }
        }
    }
}
