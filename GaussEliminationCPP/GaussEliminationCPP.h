#pragma once

extern "C" {
	__declspec(dllexport) double* computeGaussElimination(double matrix[], int rows, int cols);

	__declspec(dllexport) double* gaussElimWithThreading(double matrix[], int rows, int cols, int threadsNum);
}

void threadRowProcessing(double** matrix, int rowStartIndex, int rowLen, double a_ii, int i_index);
double* gaussBackSubstition(double* matrix, int rows, int cols);