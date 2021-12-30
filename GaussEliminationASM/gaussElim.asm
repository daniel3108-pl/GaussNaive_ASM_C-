.data
j dq 0
i dq 0
k dq 0 
aii dq 0
ajj dq 0
sum dq 0
rows dq 0
cols dq 0

matrix dq 0
arr dq 0


.code
gaussEliminationMASM PROC
        mov     cols, r8
        mov     rows, rdx
		mov		aii, 0
		mov		ajj, 0
        mov     i, 0
        mov     j, 0
While1:
        mov     rax, rows
        cmp     i, rax
        jge     w1end

		mov		rax, i
		imul	rax, cols
		add		rax, i
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

		mov		rdx, i
		imul	rdx, cols
		add		rdx, k

		movsd	xmm0, ajj
		mulsd	xmm0, QWORD PTR [rcx + rdx * 8]
		movsd	xmm1, QWORD PTR [rcx + rax * 8]
		subsd	xmm1, xmm0
		movaps	xmm0, xmm1

		mov		rax, j
		imul	rax, cols
		adc		rax, k

		movsd	QWORD PTR [rcx + rax * 8], xmm0
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
	ret
gaussEliminationMASM ENDP
end
