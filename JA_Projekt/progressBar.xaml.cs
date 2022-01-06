using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JA_Projekt
{
    /// <summary>
    /// Logika interakcji dla klasy progressBar.xaml
    /// </summary>
    public partial class progressBar : Window
    {
        public progressBar()
        {
            InitializeComponent();
        }

        public void setTitle(string title)
        {
            this.Title = title;
        }
    }
}
