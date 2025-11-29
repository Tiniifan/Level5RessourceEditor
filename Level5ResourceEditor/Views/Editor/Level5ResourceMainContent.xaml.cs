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
        private Border _activeTab;

        public Level5ResourceMainContent()
        {
            InitializeComponent();
            SetActiveTab(Scene3DTab);
        }

        private void TabClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border tab)
            {
                SetActiveTab(tab);
            }
        }

        private void SetActiveTab(Border selectedTab)
        {
            ResetTabStyles();
            _activeTab = selectedTab;

            // Style du tab actif
            selectedTab.Background = (Brush)FindResource("Theme.Accent.Brush");
            selectedTab.Opacity = 1.0;

            // Affichage des contenus
            if (selectedTab == Scene3DTab)
            {
                ShowScene3DContent();
            }
            else if (selectedTab == Scene2DTab)
            {
                ShowScene2DContent();
            }
        }

        private void ResetTabStyles()
        {
            Scene3DTab.Background = (Brush)FindResource("Theme.Control.BackgroundBrush");
            Scene3DTab.Opacity = 0.8;

            Scene2DTab.Background = (Brush)FindResource("Theme.Control.BackgroundBrush");
            Scene2DTab.Opacity = 0.8;
        }

        private void ShowScene3DContent()
        {
            Scene3DContent.Visibility = Visibility.Visible;
            Scene2DContent.Visibility = Visibility.Collapsed;
        }

        private void ShowScene2DContent()
        {
            Scene3DContent.Visibility = Visibility.Collapsed;
            Scene2DContent.Visibility = Visibility.Visible;
        }
    }
}
