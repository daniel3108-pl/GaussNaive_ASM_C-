
#include "pch.h"
#include "GaussEliminationCPP.h"

#include <iostream>
#include <thread>
#include <mutex>
#include <algorithm>
#include <vector>

double* computeGaussElimination(double matrix[], int rows, int cols)
{

    for (int i = 0; i < rows; i++)
    {
        double a_ii = matrix[i * cols + i];
        for (int j = i + 1; j < rows; j++)
        {
            double a_ji = matrix[j * cols + i] / a_ii;
            for (int k = 0; k < cols; k++)
                matrix[j * cols + k] -= a_ji * matrix[i * cols + k];         
        }
    }
    double* results = new double[rows];
    for (int i = 0; i < rows; i++)
        results[i] = 0;

    for (int i = rows - 1; i > -1; i--)
    {
        results[i] = matrix[i * cols + rows];

        for (int j = i + 1; j < rows; j++)
            results[i] -= matrix[i * cols + j] * results[j];

        results[i] /= matrix[i * cols + i];
    }

    return results;
}


double* gaussBackSubstition(double* matrix, int rows, int cols) {
    double* results = new double[rows];
    for (int i = 0; i < rows; i++)
        results[i] = 0;

    for (int i = rows - 1; i > -1; i--)
    {
        results[i] = matrix[i * cols + rows];
        for (int j = i + 1; j < rows; j++)
            results[i] -= matrix[i * cols + j] * results[j];

        results[i] /= matrix[i * cols + i];
    }
    return results;
}
// Single row processing 
void threadRowProcessing(double** matrix, int rowStartIndex, int rowLen,
    double a_ii, int i_index) {

    double a_ji = (*matrix)[rowStartIndex + i_index] / a_ii;
    int j = 0;

    for (int i = 0; i < rowLen; i++) {
        (*matrix)[rowStartIndex + i] -= (*matrix)[i_index * rowLen + i] * a_ji;
        j++;
    }

}

// Parallel gauss elimination with std::thread
double* gaussElimWithThreading(double matrix[], int rows, int cols, int threadNum) 
{
    for (int i = 0; i < rows - 1; i++) {
        double a_ii = matrix[i * cols + i];

        int j = i + 1, k = 0;
        int threadsToMake = (threadNum <= rows - j) ? threadNum : rows - j;
        int diff = (rows - j) % threadsToMake;

        std::vector<std::thread> threads(threadsToMake);

        while (j < rows) {
            threads[k] = std::move(std::thread(threadRowProcessing, &matrix, (j)*cols, cols, a_ii, i));

            if (rows - j == diff)
                threadsToMake = diff;
            k++;
            if (k == threadsToMake) {
                for (int s = 0; s < threadsToMake; s++)
                    threads[s].join();
                k = 0;
            }
            j++;
        }
    }
    return gaussBackSubstition(matrix, rows, cols);
}



