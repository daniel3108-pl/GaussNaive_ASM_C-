
.data
i dq 0
j dq 0
rows dq 0
cols dq 0
matrix dq 0
results dq 0

.code
gaussBackSubstMASM PROC
start:
        mov		cols, r8
        mov     rows, rdx
		mov		matrix, rcx
		mov		results, r9
        mov     rax, rows
        dec     rax
        mov     i, rax
while1:
        cmp     i, -1
        jle     w1end
        mov     rax, i
        imul    rax, cols
        add     rax, rows
        mov		rcx ,i
        mov     rdx, results
        mov     r8, matrix
        movsd   xmm0, QWORD PTR [r8+rax*8]
        movsd   QWORD PTR [rdx+rcx*8], xmm0
        mov     rax, i
        inc     rax
        mov     j, rax
while2:
        mov     rax, rows
        cmp     j, rax
        jge     SHORT w2end
        mov		rax, i
        mov     rcx, i
        imul    rcx, cols
        add     rcx, j
        mov		rdx, j
        mov     r8, matrix
        mov     r9, results
        movsd   xmm0, QWORD PTR [r8+rcx*8]
        mulsd   xmm0, QWORD PTR [r9+rdx*8]
        mov     rcx, results
        movsd   xmm1, QWORD PTR [rcx+rax*8]
        subsd   xmm1, xmm0
        movaps  xmm0, xmm1
        mov		rax, i
        mov     rcx, results
        movsd   QWORD PTR [rcx+rax*8], xmm0
        
		mov     rax, j
        inc     rax
        mov     j, rax
		jmp		while2
w2end:
        mov		rax, i
        mov     rcx, i
        imul    rcx, cols
        add     rcx, i
        mov     rdx, results
        mov     r8, matrix
        movsd   xmm0, QWORD PTR [rdx+rax*8]
        divsd   xmm0, QWORD PTR [r8+rcx*8]
        mov		rax, i
        mov     rcx, results
        movsd   QWORD PTR [rcx+rax*8], xmm0
		
		mov     rax, i
        dec		rax
        mov     i, rax
        jmp     while1
w1end:
        ret     0

gaussBackSubstMASM endp
end
