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
using System.Runtime.InteropServices;
using System.Threading;

namespace JA_Projekt
{
    // Klasa Modelu Odpowiadajacego za logike programu i wywolywania odpowiednich bibliotek dll
    // oraz konwertowanie typow danych miedzy bibliotekami
    public class GaussEliminationAppModel
    {
        // Pole tabeli danych, ktora jest zrodlem danych dla tabeli z interfejsu GUI
        // przechowuje macierz ukladu rownan
        private DataTable matrixTab;

        // Konstructor domyslny
        public GaussEliminationAppModel()
        { }

        // Getter dla tabeli danych maciezry
        public DataTable getMatrixTab() => this.matrixTab;

        // Metoda, ktora wypelnia zerami tabele na podstawie podanych z picker'a w GUI ilosci wierszy macierzy (ilosci szukanych)
        public void fillMatrixTable(int pickerVal)
        {
            this.matrixTab = new DataTable("EquationMatrix");
            int varNum = pickerVal;
            int matrixCol = varNum + 1;
            List<string> headers = new List<string>();

            for (int i = 0; i < matrixCol; i++)
            {
                string header = i < varNum ? $"x{i + 1}" : "V";
                headers.Add(header);

                DataColumn column = new DataColumn();
                column.DataType = System.Type.GetType("System.Double");
                column.ColumnName = header;
                column.ReadOnly = false;
                this.matrixTab.Columns.Add(column);
            }
            for (int j = 0; j < varNum; j++)
            {
                DataRow row = this.matrixTab.NewRow();

                foreach (string s in headers)
                    row[s] = 0;

                this.matrixTab.Rows.Add(row);
            }
        }

        // Funkcja czytajaca z pliku CSV zawartosc i zapisuje ja do tabeli danych modelu
        // parsowanie CSV jest uzywajac metody parseCSV
        public bool readCSVToMatrix(string fpath)
        {
            DataTable? matrixCSV = this.parseCSV(fpath);
            if (matrixCSV == null)
                return false;

            this.matrixTab = matrixCSV;
            return true;
        }

        // Metoda parsuje plik CSV podany jako sciezka w argumencie
        // Zwraca tabelę typu DataTable lub null jesli parsowanie sie nie powiodlo
        // z powodu niepoprawnego typu danych lub braku headera w csv
        private DataTable? parseCSV(string fpath)
        {
            if (!File.Exists(fpath))
                return null;

            DataTable df = new DataTable();

            int rowCount = 0;
            using (StreamReader sr = new StreamReader(fpath))
            {
                string[] headers = sr.ReadLine().Split(",");

                foreach (string h in headers)
                    df.Columns.Add(h.Trim());

                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(",");
                    DataRow dr = df.NewRow();
                    int i = 0;
                    foreach (string h in headers)
                    {
                        try
                        {
                            dr[h.Trim()] = double.Parse(rows[i++].Trim());
                        }
                        catch (FormatException e)
                        {
                            sr.Close();
                            return null;
                        }
                    }
                    df.Rows.Add(dr);
                    rowCount++;
                }
            }
            if (rowCount != df.Columns.Count - 1)
                return null;
            return df;
        }

        // Konwertuje dane z DataTable do tablicy double[][]
        public double[][] matrixTabToArray()
        {
            List<double[]> tempList = new List<double[]>();

            for (int i = 0; i < this.matrixTab.Rows.Count; i++)
            {
                // Pobiera pojedynczy wiersz tabeli
                // i parsuje kazdy jego element na double zwracajac tablice doubli
                // ktora dodaje do listy tempList
                double[] arr = this.matrixTab.Rows[i].ItemArray
                    .Select(x => double.Parse(x.ToString())).ToArray();
                tempList.Add(arr);
            }
            return tempList.ToArray();
        }

        // Konwertuje 2 wymiarowa tablice double'i do jedno wymiarowej o rozmairze [ rows * columns ]
        public double[] matrixTo1DArray(double[][] matrix)
        {
            int row = matrix.Length;
            int col = matrix[0].Length;
            double[] result = new double[row * col];

            for (int i = 0; i < row; i++)
                for (int j = 0; j < col; j++)
                    result[i * col + j] = matrix[i][j];

            return result;
        }

        // Funkcja z biblioteki DLL C++ oblcizajaca wielowatkowo wartosci ukladu rownan
        [System.Runtime.InteropServices.DllImport(@"C:\Users\Daniel\source\repos\JA_Projekt\x64\Debug\GaussEliminationCPP.dll",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gaussElimWithThreading(double[] matrix, int rows, int cols, int threadsNum);

        // Procedura z biblioteki DLL MASM x64 doporwadzajaca Macierz do postaci schodkowej
        //[System.Runtime.InteropServices.DllImport(@"C:\Users\Daniel\source\repos\JA_Projekt\x64\Debug\GaussEliminationASM.dll")]
        //public static extern void gaussEliminationMASM(double[] matrix, int rows, int cols);

        [System.Runtime.InteropServices.DllImport(@"C:\Users\Daniel\source\repos\JA_Projekt\x64\Debug\GaussEliminationASM.dll")]
        public static extern double gaussEliminationMASM(double[] matrix, int rowStartIndex, int rows, int idx);


        // Procedura z biblioteki DLL MASM x64 zajmujaca sie zamiana 
        [System.Runtime.InteropServices.DllImport(@"C:\Users\Daniel\source\repos\JA_Projekt\x64\Debug\GaussEliminationASM.dll")]
        public static extern int gaussPivotingMASM(double[] matrix, int rows, int cols, int i);

        // Procedura z biblioteki DLL MASM x64 obliczajaca z postaci schodkowej maciezry wartosci niewiadomych z ukladu rownan
        [System.Runtime.InteropServices.DllImport(@"C:\Users\Daniel\source\repos\JA_Projekt\x64\Debug\GaussEliminationASM.dll")]
        public static extern void gaussBackSubstMASM(double[] matrix, int rows, int cols, double[] results);

        // Funkcja wywolujaca odpowiednia biblioteke na podstawie podanej ilosci watkow oraz wybranego rodzaju DLL'a
        // oraz zwracajaca Stringa z cialem do wyswietlenia w okienku z wynikiem programu
        public String returnEquationResults(String ddlLibrary, int threads)
        {
            var matrix2D = this.matrixTabToArray();
            if (matrix2D[0][0] == 0)
                return "ERROR!\nCannot run app when first element is 0";

            // Tworzenie obiektu stopwatcha obliczajacego czas wykonywania dll'a
            Stopwatch stopwatch = new Stopwatch();
            String resultOutputLabel = "";

            int cols = this.matrixTab.Columns.Count;
            int rows = this.matrixTab.Rows.Count;
            double[] results = new double[this.matrixTab.Rows.Count];
            double[] matrix1D = this.matrixTo1DArray(matrix2D);

            // Wywolywanie dla dll'a c++
            if (ddlLibrary.ToUpper() == "CPP")
            {
                try
                {
                    stopwatch.Start();
                    // Zwrocenie wskaznika na tablice z wartosciami obliczonych niewiadomych
                    IntPtr resultPtr = gaussElimWithThreading(matrix1D, rows, cols, threads);
                    stopwatch.Stop();
                    // Kopiowanie zawarosci zwroconej tablicy pod wskaznikiem do c#'owej tablicy double'i
                    Marshal.Copy(resultPtr, results, 0, rows);
                }
                catch (ArgumentNullException e)
                {
                    return $"Error!\nThis matrix was not possible to compute.\nIt is probably a singular matrix!\nPlease input proper matrix";
                }
                catch (DivideByZeroException e)
                {
                    return $"Error!\nThis matrix is not suitable for Gasussian Elim.\nWas trying to divide by 0.";
                }
        
            }
            // wywolanie funkcji dla dll'a MASM'owego
            else if (ddlLibrary.ToUpper() == "MASM")
            {
                // Glowna petla przechodzaca przez wszystkie - ostatni wiersze w macierzy ukladu rownan
                for(int i = 0; i < rows - 1; i++)
                {
                    // Wywolanie funkcji MASM, ktora sprawdza czy i-ty element i-tego wiersza w macierzy == 0
                    // jesli tak to szuka pierwszego od tylu wiersza gdzie i-ty jego element nie jest 0 i zamienia te wiersze miejscami
                    int success = gaussPivotingMASM(matrix1D, rows, cols, i);
                    if (success == 0)
                        return "Error!\nThis matrix was not possible to compute.\nIt is probably a singular matrix!\nPlease input proper matrix";

                    // Przygotowanie danych do watkow
                    int j = i + 1, t = 0;
                    // Ilosc watkow do przetworzenia (kazda iteracja petli z i moze miec max rows - i - 1 watkow)
                    int threadsToMake = (threads <= rows - j) ? threads : rows - j;
                    // Pozostala roznica przewidzianych watkow do inicjalizaji gdy rows - i - 1 % threads nie jest 0 
                    //(nie przysta liczba watkow a przysta wierszy lub vice versa)
                    int diff = (rows - j) % threadsToMake;

                    // Vector watkow
                    //std::vector<std::thread> threads(threadsToMake);
                    Thread[] threadList = new Thread[threadsToMake];

                    // Petla uruchamiania watkow do przetwarzania nastepnych rows - 1 - i wierszy
                    while (j < rows)
                    {
                        // Zapisywanie zmiennych lokalnych do kazdego watku, zeby nie bylo konfliktow
                        int jcols_loc = j * cols;
                        int loc_cols = cols;
                        int loc_i = i;
                        var matrix = matrix1D;

                        // Uruchomienie watkow z argumentami, macierzy, poczatkowego indeksu wiersza do przetworzenia
                        // wspolczynnika a_ii oraz indeksu wiersza w glownej petli
                        threadList[t] = new Thread(() => {
                            // Wywoluje funkcje ktora kazdy podany wiersz od i + do rows odejmuje od odpowiadajcego elementu z i-tego wiersza
                            // * matrix[j][i]/matrix[i][i]
                            gaussEliminationMASM(matrix, jcols_loc, loc_cols, loc_i);
                        });
                        // rozpoczecie watku
                        threadList[t].Start();

                        // gdy ilosc pozostalych wierszy jest rowna roznicy watkow do zrobienia a pozostalych wierszy
                        // ustawiana ilosc kolejnych watkow do uruchomienia na ta roznice
                        if (rows - j == diff)
                            threadsToMake = diff;
                        t++;
                        // jesli ilosc uruchomionych watkow jest rowna przewidzianym do wykonania
                        // nastepuje petla z join'ami watkow, ktora blokuje zakonczenie petli while
                        // przed ukonczeniem wszystkich watkow
                        if (t == threadsToMake)
                        {
                            // blokada konca petli przed wykonaniem kazdego watkow
                            for (int s = 0; s < threadsToMake; s++)
                                threadList[s].Join();
                            t = 0;
                        }
                        j++;
                    }
                }
                gaussBackSubstMASM(matrix1D, rows, cols, results);
            }
            else
                return "ERROR!! Something went wrong. \nPlease restart this application.";

            // Utworzenie tekstu, ktory ma byc wyswietlany w nowym okienku z wynikiem
            // na podstawie obliczonych niewiadomych ukladu
            int k = 1;
            foreach (double item in results)
                resultOutputLabel += $"x{k++} = {item}\n";

            resultOutputLabel += $"\n\nExceution time: {stopwatch.ElapsedMilliseconds} ms\n";
            return resultOutputLabel;
        }
    }

    
}