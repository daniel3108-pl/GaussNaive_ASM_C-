using System;
using System.Collections.Generic;
using System.Text;

namespace JA_Projekt
{
    public class MatrixModel
    {
        public double[][] Matrix;
        public int rows;
        public int cols;

        public MatrixModel(int cols = 4, int rows = 3)
        {
            this.rows = rows;
            this.cols = cols;
            this.Matrix = new double[rows][];
            for (int i = 0; i < rows; i++)
                this.Matrix[i] = new double[cols];
        }
        public MatrixModel(double[][] matr)
        {
            this.Matrix = new double[matr.Length][];
            this.rows = matr.Length;
            this.cols = matr[0].Length;
            for (int i = 0; i < this.rows; i++)
                this.Matrix[i] = new double[matr[i].Length];

            for (int i = 0; i < this.rows; i++)
                for (int j = 0; j < this.cols; j++)
                    this.Matrix[i][j] = matr[i][j];
        }
        public void setMatrix(double[][] matr)
        {
            this.Matrix = new double[matr.Length][];
            this.rows = matr.Length;
            this.cols = matr[0].Length;
            for (int i = 0; i < this.rows; i++)
                this.Matrix[i] = new double[matr[i].Length];

            for (int i = 0; i < this.rows; i++)
                for (int j = 0; j < this.cols; j++)
                    this.Matrix[i][j] = matr[i][j];
        }

        public double[][] getMatrix() => this.Matrix;
        public double getMatrixCell(int r, int c) => this.Matrix[r][c];
    }
}
