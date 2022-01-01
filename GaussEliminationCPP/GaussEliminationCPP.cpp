
#include "pch.h"
#include "GaussEliminationCPP.h"

#include <iostream>
#include <thread>
#include <mutex>
#include <algorithm>
#include <vector>


/*
	Funkcja ktora oblicza wartosci szukanych na podstawie macierzy schodkowej
	uzyskanej w glownej funkcji algorytmu.
*/
double* gaussBackSubstition(double* matrix, int rows, int cols) {
	// Inicjacja zwracanej tablicy
	double* results = new double[rows] {0};

	// Petla przechodzaca od ostatniego wiersza do 1. macierzy schodkowej
	// obliczajac kolejne niewiadome na podstawie poprzenich obliczonych
	// niewiadomych
    for (int i = rows - 1; i > -1; i--)
    {
		// i-ta niewiadoma = wyraz wolny i tego wiersza macierzy
        results[i] = matrix[i * cols + rows];
		
		// wyraz wolny -= kazdy wspolczynnik od i-tej niewiadomej w prawo * wartosc tej niewiadomej obliczonej poprzednim  etapie for'a
        for (int j = i + 1; j < rows; j++)
            results[i] -= matrix[i * cols + j] * results[j];

		// dzielenie i-tego wyrazu wolnego przez wspolczynnik przy obliczanej i-tej niewiadomej
        results[i] /= matrix[i * cols + i];
    }
    return results;
}

/*
	Funkcja ktora dokonuje przeksztalcania jednego wierza macierzy odejmujac od kazdego elementu wiersza
	iloczyn wspolczynnika a_ji (iloraz matrix[j][i] / matrix[i][i] ) i 
	elementowi wiersza i-tego na j-tej kolumnie od j-tego elementu przetwarzanego wiersza
*/
void threadRowProcessing(double** matrix, int rowStartIndex, int rowLen,
    double a_ii, int i_index) {

    double a_ji = (*matrix)[rowStartIndex + i_index] / a_ii;
    int j = 0;

    for (int i = 0; i < rowLen; i++) {
        (*matrix)[rowStartIndex + i] -= (*matrix)[i_index * rowLen + i] * a_ji;
        j++;
    }

}

void rowSwap(double** matrix, int index, int pivotIdx, int cols) {
	for (int i = 0; i < cols; i++) {
		double temp = (*matrix)[index + i];
		(*matrix)[index + i] = (*matrix)[pivotIdx + i];
		(*matrix)[pivotIdx + i] = temp;
	}
}

/*
	Glowna funkcja dll c++, ktora najpierw przechodzi po wszystkich wierszach macierzy i doprowadza ja do postaci schodkowej
	a nastepnie korzysta z funkcji gaussBackSubstition by zwrocic tablice 1-wymiarowa z wartosciami obliczonych niewiadomych
	w kolejnosci [x0, x1, x2, x3 ... , xn] (0,1,2 - kolumna macierzy odpowiada wspolczynnikom dla zmiennych)
*/
double* gaussElimWithThreading(double matrix[], int rows, int cols, int threadNum) 
{
	for (int i = 0; i < rows - 1; i++) {

		if ((int)matrix[i * cols + i] == 0) {
			int pivotIdx = i;
			for (int m = rows - 1; m > i; m--)
				if (matrix[m * cols + i] != 0) {
					pivotIdx = m;
					break;
				}
			rowSwap(&matrix, i * cols, pivotIdx * cols, cols);
		}
		

		// Obliczanie wspolczynnika do eliminacji kolejnych wierszy w watkach
        double a_ii = matrix[i * cols + i];

		// Przygotowanie danych do watkow
        int j = i + 1, k = 0;
		// Ilosc watkow do przetworzenia (kazda iteracja petli z i moze miec max rows - i - 1 watkow)
        int threadsToMake = (threadNum <= rows - j) ? threadNum : rows - j;
		// Pozostala roznica przewidzianych watkow do inicjalizaji gdy rows - i - 1 % threads nie jest 0 
		//(nie przysta liczba watkow a przysta wierszy lub vice versa)
        int diff = (rows - j) % threadsToMake;

		// Vector watkow
        std::vector<std::thread> threads(threadsToMake);

		// Petla uruchamiania watkow do przetwarzania nastepnych rows - 1 - i wierszy
        while (j < rows) {
			// Uruchomienie watkow z argumentami, macierzy, poczatkowego indeksu wiersza do przetworzenia
			// wspolczynnika a_ii oraz indeksu wiersza w glownej petli
            threads[k] = std::move(std::thread(threadRowProcessing, &matrix, (j)*cols, cols, a_ii, i));

			// gdy ilosc pozostalych wierszy jest rowna roznicy watkow do zrobienia a pozostalych wierszy
			// ustawiana ilosc kolejnych watkow do uruchomienia na ta roznice
            if (rows - j == diff)
                threadsToMake = diff;
            k++;
			// jesli ilosc uruchomionych watkow jest rowna przewidzianym do wykonania
			// nastepuje petla z join'ami watkow, ktora blokuje zakonczenie petli while
			// przed ukonczeniem wszystkich watkow
            if (k == threadsToMake) {
                for (int s = 0; s < threadsToMake; s++)
                    threads[s].join();
                k = 0;
            }
            j++;
        }
    }
	// Zwraca wynik funkcji obliczajacej niewiadome
    return gaussBackSubstition(matrix, rows, cols);
}



