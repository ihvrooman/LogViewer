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
    /// Interaction logic for AddDatabaseView.xaml
    /// </summary>
    public partial class AddDatabaseView : Window
    {
        public AddDatabaseViewModel ViewModel
        {
            get
            {
                return (AddDatabaseViewModel)DataContext;
            }
        }

        public AddDatabaseView()
        {
            InitializeComponent();
            DataContext = new AddDatabaseViewModel();
            AddDatabaseTextbox.Focus();
        }

        private void AddDatabaseTextbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                AddDatabaseButton.Focus();
            }
        }
    }
}
