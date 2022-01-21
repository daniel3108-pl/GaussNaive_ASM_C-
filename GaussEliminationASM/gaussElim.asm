; Zmienne do zapisywania parametrow funkcji i licznikow petli
.data
j dq 0
i dq 0
k dq 0 
m dq 0
l dq 0
pivotidx dq 0
rows dq 0
cols dq 0
matrix dq 0
zero dq 0

arr_aii dq 0
ajj dq 0
rowStartIndex dq 0 
rowLen dq 0
idx dq 0
; Procedura sprowadzajaca macierz wejsciowa do postaci schodkowej
; naglowek funkcji w c# : public static extern double gaussEliminationMASM(double[] matrix, double[] arr_aii, int rowStartIndex, int cols, int idx)
.code
gaussEliminationMASM PROC
start:
		; Zapis do zmiennych wartosci aprametrów z funkcji
        mov     eax, dword ptr [rbp + 48] ; pobranie ze stosu 5 arugmentu  (int idx)
		cdqe    ; konwersja dword do qworda ( eax zapisywany w rax )
		mov		idx, rax ; zapis indeksu wiersza w macierzy zapomoca ktorego elementow odejmujemy od elementow wiersza do obliczenia w watku
        mov     rowLen, r9 ; zapis dlugosci wirsza
        mov     rowStartIndex, r8 ; zapis indeksu startowego wiersza od ktorego elementow bedziemy odejmowac
		mov		arr_aii, rdx ; zapis wskaznika na arr_aii czyli idx-ty wiersz w macierzy
        mov     matrix, rcx ; zapis wskaznika na macierz

		; Zapis do rax, indeksu dla matrix[idx + rowStartIndex]
        mov     rax, idx
        add     rax, rowStartIndex
		; Zapis do rcx, indeksu dla matrix[idx * rowLen + idx]
        mov     rcx, idx

		; Zapis wskaznikow na poczatek matrix do rdx oraz r8
        mov     rdx, matrix
        mov     r8, arr_aii
		; Zapis do xmm0, matrix[idx + rowStartIndex]
        movsd   xmm0, QWORD PTR [rdx+rax*8]
		; Podzielenie xmm0 przez arr_aii[idx]
        divsd   xmm0, QWORD PTR [r8+rcx*8]
        ; Zapis wyniku do zmiennej ajj
		movsd	ajj, xmm0
        mov     i, 0
; Petla While( i < rowlen ), przechodzi po ka¿dym elemencie wiersza od rowStartIndex do rowStartIndex + rowLen
while_i_lt_rowlen:
; sprawdzenie warunku i < rowLen
        mov     rax, rowLen
        cmp     i, rax
        jge     endwhile
		; zapis indexu matrix[rowStartIndex + i]
        mov     rax, i
        add     rax, rowStartIndex
		; zapis do rcx indeksu matrix[idx * rowLen + i]
        mov     rcx, i
		; zapis do rdx wskaznika na poczatek matrix
        mov     rdx, arr_aii
		; zapis do xmm0 arr_aii[idx  + i]
        movsd   xmm0, QWORD PTR [rdx+rcx*8]
        mulsd   xmm0, ajj ; arr_aii[idx  + i] * ajj
        mov     rcx, matrix 
		; Wykonanie dzia³ania xmm1 = matirx[i + rowStartIndex] - arr_aii[idx + i] (xmm0)
        movsd   xmm1, QWORD PTR [rcx+rax*8]
        subsd   xmm1, xmm0
		; xmm0 = xmm1
        movaps  xmm0, xmm1
        mov     rax, i
        add     rax, rowStartIndex
        mov     rcx, matrix
		; matrix[rowStartIndex + i] = xmm0
        movsd   QWORD PTR [rcx+rax*8], xmm0

		; inkrementacja i i przejscie do kolejnej iteracji petli
	    mov     rax, i
        inc     rax
        mov     i, rax
        jmp     while_i_lt_rowlen
endwhile:
        ret     0
gaussEliminationMASM ENDP

gaussPivotingMASM PROC
	; Ten Blok od if_aiieq0 do endifc jest odpowiedzialny za sprawdzenie czy element w macierzy[i][i] jest rowny 0
; jesli jest nastepuje petla wyszukujaca od tylu macierzy czy jest jakis element w m-tym wierszu w i-tej kolumnie, ktory nie jest rowny 0
; jesli nie jest rowny zero do zmiennej pivotidx jest zapisywane m i nastepuje potem zamiana wierszy miejscami
start:
		mov		matrix, rcx
        mov     cols, r8
        mov     rows, rdx
        mov     i, r9
        mov     j, 0
		mov		zero, 0
if_aiieq0:
		; sprawdzenie warunku matrix[i * cols + i] == 0
		mov		rax, i
		imul	rax, cols
		add		rax, i
		mov		rcx, matrix
		movsd	xmm0, QWORD PTR [rcx + rax * 8]
	    ucomisd xmm0, zero
		jne		endifc
thenif:
; jest rowny wiec do pivotidx jest przypisywane i
		mov		rax, i
		mov		pivotidx, rax
		; incializacja licznika m = rows - 1
		mov		rax, rows
		dec		rax
		mov		m, rax
while_m_gt_i:
; petla while(m > i)
		mov		rax, i ; rax = i
		cmp		m, rax ; sprawdzenie m z rax
		jle		endwmlti 

if_m_mne0:
; warunek if ( matrix[m * cols + i] != 0 ) 
		mov		rax, m
		imul	rax, cols
		add		rax, i
		mov		rcx, matrix
		movsd	xmm0, QWORD PTR [rcx + rax *8]
		ucomisd	xmm0, zero ; porownanie rejestru xmm0  do 0
		je		endifmmi ; jesli rowne if sie nie wykonuje skok do endifmmi
thenif2:
		; przyjecie pivotidx = m i break z petli while_m_gt_i
		mov		rax, m
		mov		pivotidx, rax
		jmp		breakwmlti
endifmmi:
; jesli if sie nie wykonal nastepuje dekrementacja m i dalsza iteracja petli
		mov		rax, m
		dec		rax
		mov		m, rax
		jmp		while_m_gt_i
endwmlti:
; nieznaleziono zadnego wiersza gdzie element pocz nie jest 0, funkcja zwraca 0 => Macierz nie rozwiazywalna
		mov		rax, 0
		ret
breakwmlti:
; licznik petli zamieniajacej wiersze 
		mov		l, 0
whilerowsswap:
; while ( l < l )
		mov		rax, cols
		cmp		l, rax
		jge		endifc

		;; Zamiana wierszy w macierzy
		; przygotowanie indeksu do wskaznika na matrix
		mov		rax, i
		imul	rax, cols
		add		rax, l
		; r10 = wskaznik na matrix
		mov		r10, matrix
		movsd	xmm1, QWORD PTR [r10 + rax * 8] ; xmm1 = matrix[ i * cols + l ]
		mov		rdx, pivotidx
		imul	rdx, cols
		add		rdx, l
		mov		r9, matrix ; r1 = wskaznik na matrix
		mov		r8, matrix ; r8 = wskaznik na matrix
		movsd	xmm0, QWORD PTR [r9 + rdx * 8] ; xmm0 = matrix[ pivotidx * cols + l]
		movsd	QWORD PTR [r8 + rax * 8], xmm0 ; matrix[i * cols + l] = xmm0
		mov		rcx, matrix
		movsd	QWORD PTR [rcx + rdx * 8], xmm1 ; matrix[pivotidx * cols + l] = xmm1

		mov		rax, l
		inc		rax
		mov		l, rax ; l += 1 | incrementacja licznika l
		jmp		whilerowsswap
endifc:
		mov rax, 1
		ret 
gaussPivotingMASM ENDP

end

; do raportu dopisac 
; dlaczego scalary
; dane na 65 zmiennych
; ustawic z automaty ilosc dostepnych logiczych domyslnie
; liczyc inaczej czas