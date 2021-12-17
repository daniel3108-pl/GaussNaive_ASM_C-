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
    // Main view class that displays user interface 
    // based on MainWindow.XAML file
    public partial class MainWindowViewModel : Window
    {
        // viewModel Object field
        private EquationMatrixModel model;
        // Default Constructor of MainWindow Class
        public MainWindowViewModel()
        {
            InitializeComponent();
            this.model = new EquationMatrixModel();

            // Initializing DataGrid to hold matrix for equations
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
        // Method that runs equation computing method based on DDL Library and displays its result 
        // with time that the computation took.
        private void computeEquation_Click(object sender, RoutedEventArgs e)
        {

            if (this.csddl.IsChecked == true)
            {
                this.showSubWindow(
                        "Result of equation using C++ DLL",
                        this.model.returnEquationResults("CPP", (int) this.Threads.Value));
            }
            else if (this.asmddl.IsChecked == true)
            {
                this.showSubWindow(
                       "Result of equation using MASM DLL",
                       this.model.returnEquationResults("MASM", (int) this.Threads.Value));
            }
        }
        // Method that fills data table in grid based on number of variables in the vars slider
        private void applyVar_Click(object sender, RoutedEventArgs e)
        {
            this.model.fillMatrixTable((int)this.xPicker.Value);
            this.matrix.ItemsSource = this.model.getMatrixTab().DefaultView;
            this.matrix.DataContext = this.model.getMatrixTab().DefaultView;
            this.Threads.Maximum = this.model.getMatrixTab().Rows.Count - 1;
        }
        // A method that displays file open dialog and runs viewModel methods to parse and
        // save csv file content to DataTable in the grid.
        private void readCSV_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog fdlg = new Microsoft.Win32.OpenFileDialog();
            fdlg.FileName = "";
            fdlg.DefaultExt = ".csv";
            fdlg.Filter = "Comma Separated Values (.csv)|*.csv";

            if (fdlg.ShowDialog() == true)
            {
                if (!this.model.readCSVToMatrix(fdlg.FileName))
                    this.showSubWindow("Error!",
                                       "Incorrect file!\nChoose proper file again!");
                else
                {
                    this.matrix.ItemsSource = this.model.getMatrixTab().DefaultView;
                    this.matrix.DataContext = this.model.getMatrixTab().DefaultView;
                    this.Threads.Maximum = this.model.getMatrixTab().Rows.Count - 1;
                }
            }
            else
                this.showSubWindow("Error!", "Something went wrong with file dialog!");

        }
        // Method that shows sub window with specified header, body and footer
        private void showSubWindow(String head = "", String body = "", String foot = "")
        {
            resultWindow resWin = new resultWindow();
            resWin.setTextBlockes(head, body, foot);
            resWin.Show();
        }
    }


}
