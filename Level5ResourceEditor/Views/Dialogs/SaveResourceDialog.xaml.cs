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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Level5ResourceEditor.ViewModels.Dialogs;

namespace Level5ResourceEditor.Views.Dialogs
{
    /// <summary>
    /// Logique d'interaction pour SaveResourceDialog.xaml
    /// </summary>
    public partial class SaveResourceDialog : UserControl
    {
        public SaveResourceDialog()
        {
            InitializeComponent();
        }

        public SaveResourceDialogViewModel ViewModel
        {
            get => DataContext as SaveResourceDialogViewModel;
            set => DataContext = value;
        }
    }
}
