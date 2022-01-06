using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Logika interakcji dla klasy resultWindow.xaml
    /// </summary>
    /// Klasa okienka z wynikiem obliczania ukladu rownan
    public partial class resultWindow : Window
    {
        // Konstruktor
        public resultWindow()
        {
            InitializeComponent();
        }
        // Konstruktor przyjmujaca header, string tablicy z obliczonymi wartosciami niewiadomy i footer
        public resultWindow(String header = "", String resultString = "",  String footer = "")
        {
            InitializeComponent();
            this.result.Text = resultString;
            this.header.Text = header;
            this.footer.Text = footer;
        }
        // Metoda ustawiajaca wartosci pol tekstowych w widoku jak w konstruktorze powyzej
        public void setTextBlockes(String header, String resultString, String footer)
        {
            this.result.Text = resultString;
            this.header.Text = header;
            this.footer.Text = footer;
        }

        // Przyznanie rodzicowi okna (czyli glownemu oknu z tabela) focus'a
        void _Closing(object sender, CancelEventArgs e)
        {
            this.Owner.Activate();
        }


    }
}
