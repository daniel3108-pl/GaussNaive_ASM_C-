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

namespace JA_Projekt
{
    // A Controller class that takes care of logic of the interface and holds data of this
    // program to use for computations.
    public class EquationMatrixModel
    {
        private MatrixModel matrixArray;
        private DataTable matrixTab;

        public EquationMatrixModel()
        {
            this.matrixArray = new MatrixModel();
        }

        public MatrixModel getMatrixArray() => this.matrixArray;
        public DataTable getMatrixTab() => this.matrixTab;


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

        // Konwertuje DataTable obiekt do tablicy double[][]
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
        public void prepareMatrixModel()
        {
            this.matrixArray.setMatrix(this.matrixTabToArray());
        }

        [System.Runtime.InteropServices.DllImport(@"C:\Users\Daniel\source\repos\JA_Projekt\x64\Debug\GaussEliminationCPP.dll", 
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gaussElimWithThreading(double[] matrix, int rows, int cols, int threadsNum);

        [System.Runtime.InteropServices.DllImport(@"C:\Users\Daniel\source\repos\JA_Projekt\x64\Debug\GaussEliminationASM.dll")]
        public static extern void gaussEliminationMASM(double[] matrix, int rows, int cols, double[] results);

        public String returnEquationResults(String ddlLibrary, int threads)
        {
            this.prepareMatrixModel();

            Stopwatch stopwatch = new Stopwatch();
            String resultOutputLabel = "";
            double[] results = new double[this.matrixArray.rows];
            double[] matrix1D = this.matrixTo1DArray(this.matrixArray.Matrix);

            if (ddlLibrary.ToUpper() == "CPP")
            {
                stopwatch.Start();
                IntPtr resultPtr = gaussElimWithThreading(matrix1D, this.matrixArray.rows, this.matrixArray.cols, threads);
                stopwatch.Stop();
                try
                {
                    Marshal.Copy(resultPtr, results, 0, this.matrixArray.rows);
                }
                catch (ArgumentNullException e)
                {
                    return $"Error!\n{e.Message}";
                }
            }
            else if (ddlLibrary.ToUpper() == "MASM")
            {
                int size = this.matrixArray.rows * this.matrixArray.cols;
                results = new double[this.matrixArray.rows];
                int cols = this.matrixArray.cols;
                int rows =this.matrixArray.rows;

                gaussEliminationMASM(matrix1D, rows, cols, results);

            }
            else
                return "ERROR!! Something went wrong. \nPlease restart this application.";

            int i = 1;
            foreach (double item in results)
                resultOutputLabel += $"x{i++} = {item}\n";

            resultOutputLabel += $"\n\nExceution time: {stopwatch.ElapsedMilliseconds} ms\n";
            return resultOutputLabel;
        }
    }

    class CSVDataFormatException : Exception
    {
        public CSVDataFormatException() { }
        public CSVDataFormatException(string message) : base(message) { }
    }
}
