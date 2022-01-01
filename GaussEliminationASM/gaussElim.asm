; Zmienne do zapisywania parametrow funkcji i licznikow petli
.data
j dq 0
i dq 0
k dq 0 
m dq 0
l dq 0
pivotidx dq 0
aii dq 0
ajj dq 0
rows dq 0
cols dq 0
matrix dq 0
zero dq 0

; Procedura sprowadzajaca macierz wejsciowa do postaci schodkowej
; naglowek funkcji w c# : public static extern void gaussEliminationMASM(double[] matrix, int rows, int cols);
.code
gaussEliminationMASM PROC
        mov		matrix, rcx
        mov     cols, r8
        mov     rows, rdx
		mov		aii, 0
		mov		ajj, 0
        mov     i, 0
        mov     j, 0
		mov		zero, 0
While1:
        mov     rax, rows
        cmp     i, rax
        jge     w1end

; Ten Blok od if_aiieq0 do endifc jest odpowiedzialny za sprawdzenie czy element w macierzy[i][i] jest rowny 0
; jesli jest nastepuje petla wyszukujaca od tylu macierzy czy jest jakis element w m-tym wierszu w i-tej kolumnie, ktory nie jest rowny 0
; jesli nie jest rowny zero do zmiennej pivotidx jest zapisywane m i nastepuje potem zamiana wierszy miejscami
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
; Reszta kodu zajmuje sie doprowadzeniem macierzy do postaci schodkowej
		mov		rax, i
		imul	rax, cols
		add		rax, i
		mov		rcx, matrix
		movsd	xmm0, QWORD PTR [rcx + rax * 8]
		movsd	aii, xmm0

		mov		rax, i
		inc		rax
        mov     j, rax
While2:
		mov     rax, rows
        cmp     j, rax
        jge     w2end

		mov		rax, j
		imul	rax, cols
		add		rax, i
		movsd	xmm0, QWORD PTR [rcx + rax * 8]
		divsd	xmm0, aii
		movsd	ajj, xmm0

		mov		k, 0
While3:
		mov		rax, cols
		cmp		k, raX
		jge		w3end

		mov		rax, j
		imul	rax, cols
		add		rax, k

		mov		rcx, i
		imul	rcx, cols
		add		rcx, k
		mov		rdx, matrix
		movsd	xmm0, ajj
		mulsd	xmm0, QWORD PTR [rdx + rcx * 8]
		mov		rcx, matrix
		movsd	xmm1, QWORD PTR [rcx + rax * 8]
		subsd	xmm1, xmm0
		movaps	xmm0, xmm1

		mov		rax, j
		imul	rax, cols
		adc		rax, k

		mov		rdx, matrix
		movsd	QWORD PTR [rdx + rax * 8], xmm0
		; ------ ;
		mov		rax, k
		inc		rax
		mov		k, rax
		jmp		While3
w3end:
		mov		rax, j
		inc		rax
		mov		j, rax
		jmp		While2
w2end:
		; ------- ;
        mov     rax, i
        inc     rax
        mov     i, rax
        jmp		While1
w1end:
	mov rax, 1
	ret	
gaussEliminationMASM ENDP
end
