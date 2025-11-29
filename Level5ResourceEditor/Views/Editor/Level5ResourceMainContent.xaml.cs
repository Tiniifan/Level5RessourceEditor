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

namespace Level5ResourceEditor.Views.Editor
{
    /// <summary>
    /// Logique d'interaction pour Level5ResourceMainContent.xaml
    /// </summary>
    public partial class Level5ResourceMainContent : UserControl
    {
        public Level5ResourceMainContent()
        {
            InitializeComponent();
            SelectTab(Scene3DTab);
        }

        private void TabClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                SelectTab(border);
            }
        }

        private void SelectTab(Border selectedTab)
        {
            // Reset all tabs
            Scene3DTab.Style = (Style)FindResource("TabItemStyle");
            Scene2DTab.Style = (Style)FindResource("TabItemStyle");

            // Set selected tab style
            selectedTab.Style = (Style)FindResource("TabItemSelectedStyle");

            // Show corresponding content
            if (selectedTab == Scene3DTab)
            {
                Scene3DContent.Visibility = Visibility.Visible;
                Scene2DContent.Visibility = Visibility.Collapsed;
            }
            else if (selectedTab == Scene2DTab)
            {
                Scene3DContent.Visibility = Visibility.Collapsed;
                Scene2DContent.Visibility = Visibility.Visible;
            }
        }
    }
}
