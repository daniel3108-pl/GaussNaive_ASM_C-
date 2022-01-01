; Zmienne przechowujace argumenty funkcji i indeksy do petli
.data
i dq 0
j dq 0
rows dq 0
cols dq 0
matrix dq 0
results dq 0

; Procedura obliczajaca z macierzy schodkowej wartosci niewiadomych ukladu rownan
; postac w c# : public static extern void gaussBackSubstMASM(double[] matrix, int rows, int cols, double[] results);
.code
gaussBackSubstMASM PROC
; Zapisywanie do zmiennych wartosci parametrow procedury podanych z C#
; Zeby moc korzystac z kazdego rejestru bez obawy przed utracenie danych
start:
		; rcx = wskaznik na tablice matrix | rdx = ilosc wierszy macierzy | r8 = ilosc kolumn macierzy
		; r9 = wskaznik na tablice results
        mov		cols, r8
        mov     rows, rdx
		mov		matrix, rcx
		mov		results, r9
        mov     rax, rows
        dec     rax
        mov     i, rax
; Pierwsza petla idaca od tylu tablicy z wynikami
while1:
		; sparwdzenie warunku while'a
        cmp     i, -1
        jle     w1end
		; Zapisanie do rax indeksu dla tablicy matrix z ktorego pobierany bedzie
		; wartosc do zapisu na i-ta pozycje tablicy results => zapis wyrozu wolnego w rownaniu na i-tym wierszu
        mov     rax, i
        imul    rax, cols
        add     rax, rows
        mov		rcx ,i
		; Zaladowanie do rdx wskaznika na tablice results oraz do r8 wskaznika na tablice macierzy
        mov     rdx, results
        mov     r8, matrix
		; zapisanie do rejrestu xmm0 wartosci matrix[i * cols + rows]
        movsd   xmm0, QWORD PTR [r8+rax*8]
		; kopiowanie zawartosci xmm0 do tablicy result na indexie i
        movsd   QWORD PTR [rdx+rcx*8], xmm0
		
		; przygotowanie licznika j = rows + 1 dla kolejnej petli  
        mov     rax, i
        inc     rax
        mov     j, rax
; petla odejmujaca od wartosci result[i] obliczone poprzednie niewiadome * ich wspolczynniki
while2:
		; sprawdzenie warunku petli while(j < rows)
        mov     rax, rows
        cmp     j, rax
        jge     SHORT w2end
		; przygotowanie indeksu i * cols + j dla macierzy do rejestru rcx
        mov		rax, i
        mov     rcx, i
        imul    rcx, cols
        add     rcx, j
        mov		rdx, j
		; zapis do r8 wskaznika na matrix oraz do r9 wskaznika na resutls
        mov     r8, matrix
        mov     r9, results
		; zapis do rejestru xmm0 wartosci matrix[ i * cols + j]
        movsd   xmm0, QWORD PTR [r8+rcx*8]
		; pomnozenie wartosci xmm0 przez obliczona niewiadoma w poprzednim etapie pierwszego while'a
        mulsd   xmm0, QWORD PTR [r9+rdx*8]
        mov     rcx, results
		; zapis do xmm1 wartosci naszego aktualnego wyrazu wolnego w rownaniu
        movsd   xmm1, QWORD PTR [rcx+rax*8]
		; odjecie od xmm1 - xmm0 czyli od wyrazu wolnego odejmujemy j-ta niewiadoma * jej wspolczynnik 
        subsd   xmm1, xmm0
		; zapis wartosci roznicy do rejestru xmm0
        movaps  xmm0, xmm1

        mov		rax, i
        mov     rcx, results
        ; zapis w tablicy result na i-tej pozycji wartosci roznicy
		movsd   QWORD PTR [rcx+rax*8], xmm0
        
		; incrementacja licznika j 
		mov     rax, j
        inc     rax
        mov     j, rax
		; skok do poczatku petli
		jmp		while2
; cialo petli whlie 1 po zakonczeniu petli wewnetrznej
w2end:
		; przygotowanie indeksow matrix[ i * cols + i ] oraz i dla tablic wynikow
        mov		rax, i
        mov     rcx, i
        imul    rcx, cols
        add     rcx, i
        mov     rdx, results
        mov     r8, matrix
		; zapis wyrazu wolnego results[i] do xmm0
        movsd   xmm0, QWORD PTR [rdx+rax*8]
		; dzielenie w xmm0 wyrazu wolnego przez wartosc macierzy[ i* cols + i] czyli wspolczynnik dla szukanego x'a (niewiadomej)
        divsd   xmm0, QWORD PTR [r8+rcx*8]
        mov		rax, i
        mov     rcx, results
		; zapis obliczonej wartosci niewiadomej na jej odpowiedniej pozycji i-tej
        movsd   QWORD PTR [rcx+rax*8], xmm0
		
		; decrementacja licznika 
		mov     rax, i
        dec		rax
        mov     i, rax
        jmp     while1
; koniec procedury powrot do miejsca jej wywolania
w1end:
        ret     0
gaussBackSubstMASM endp
end
