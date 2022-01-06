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
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

/*
 * Projekt Języki Asemblerowe
 * Temat: Obliczanie układów równań metodą Eliminacji Gaussa
 * 
 * Autor: Daniel Świetlik
 * Kierunek: Informatyka Katowice
 * Grupa: 2
 * Prowadzący: Krzysztof Hanzel
 * 
 * 
 */
namespace JA_Projekt
{
    // Glowna klasa modelu widoku wyswietlajaca interfejs uzytkownika
    public partial class MainWindowViewModel : Window
    {
        // Obiekt modelu aplikacji
        private GaussEliminationAppModel model;
        // Konstruktor domyslny 
        public MainWindowViewModel()
        {
            InitializeComponent();
            this.model = new GaussEliminationAppModel();

            // Inicjowanie tabeli w widoku i jej wlasciwosci
            this.model.fillMatrixTable((int)this.xPicker.Value);
            this.matrix.ItemsSource = this.model.getMatrixTab().DefaultView;
            this.matrix.DataContext = this.model.getMatrixTab().DefaultView;
            this.matrix.ColumnWidth = 50;
            this.matrix.CanUserAddRows = false;
            this.matrix.CanUserDeleteRows = false;
            this.matrix.CanUserResizeRows = false;
            this.matrix.CanUserSortColumns = false;
            this.matrix.CanUserReorderColumns = false;
            this.matrix.CanUserResizeColumns = false;
            this.matrix.RowHeight = 24;

            this.Threads.Maximum = this.model.getMatrixTab().Rows.Count - 1;

        }
        // Wywolanie w modelu obliczenia wartosci na podstawie odpowiedniej biblioteki DLL  wybranej w radioButtonie przez uzytkownika
        private async void computeEquation_Click(object sender, RoutedEventArgs e)
        {
            var prog = new progressBar();
            prog.setTitle("Computing gauss equation");
            await this.showProgresBar(prog);
            await this.computeEquationWorker();
            prog.Close();
        }
        private async Task showProgresBar(progressBar prog)
        {
            var progres = prog;
            progres.Owner = this;
            progres.Show();
        }

        private async Task computeEquationWorker()
        {
            var model = this.model;
            var threads = (int)this.Threads.Value;
            if (this.csddl.IsChecked == true)
            {
                this.showSubWindow(
                        "Result of equation using C++ DLL",
                        await Task.Run(() => {
                            return model.returnEquationResults("CPP", threads);
                        })
                 );
            }
            else if (this.asmddl.IsChecked == true)
            {
                this.showSubWindow(
                       "Result of equation using MASM DLL",
                       await Task.Run(() => {
                           return model.returnEquationResults("MASM", threads);
                       }));
            }
        }

        // Metoda ktora zmienia rozmiar tabeli na podany przez uzytkownika w sliderze
        private void applyVar_Click(object sender, RoutedEventArgs e)
        {
            this.model.fillMatrixTable((int)this.xPicker.Value);
            this.matrix.ItemsSource = this.model.getMatrixTab().DefaultView;
            this.matrix.DataContext = this.model.getMatrixTab().DefaultView;
            this.Threads.Maximum = this.model.getMatrixTab().Rows.Count - 1;
        }
        // Metoda wywolujaca okienko wyboru pliku csv, ktory ma byc zapisywany do tabeli jako macierz ukladow rownan
        private async void readCSV_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog fdlg = new Microsoft.Win32.OpenFileDialog();
            fdlg.FileName = "";
            fdlg.DefaultExt = ".csv";
            fdlg.Filter = "Comma Separated Values (.csv)|*.csv";

            if (fdlg.ShowDialog() == true)
            {
                var prog = new progressBar();
                prog.setTitle("Loading CSV file");
                await readCSVWorker(fdlg.FileName);
                await showProgresBar(prog);
                prog.Close();
            }
            else
                this.showSubWindow("Error!", "Something went wrong with file dialog!");

        }
        private async Task readCSVWorker(string fpath)
        {
            if (! await Task.Run( () => this.model.readCSVToMatrix(fpath) ))
                this.showSubWindow("Error!",
                                   "Incorrect file!\nChoose proper file again!");
            else
            {
                this.matrix.ItemsSource = this.model.getMatrixTab().DefaultView;
                this.matrix.DataContext = this.model.getMatrixTab().DefaultView;
                this.Threads.Maximum = this.model.getMatrixTab().Rows.Count - 1;
            }
        }
        // Metoda wywolujaca nowe okno z wynikiem obliczen ukladu rownan
        private void showSubWindow(String head = "", String body = "", String foot = "")
        {
            resultWindow resWin = new resultWindow();
            resWin.Owner = this;
            resWin.setTextBlockes(head, body, foot);
            resWin.Show();
        }
    }


}
