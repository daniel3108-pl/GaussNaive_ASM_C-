; Program do wykonywania eliminacji gaussa
.data
index dq 0
len dq 0
rows dq 0
cols dq 0
ind2 dq 0
a_ii real8 0.0
a_ji real8 0.0

.code
gaussEliminationMASM proc
	mov index, 0
	mov ind2, 0
	mov rows, rdx
	mov cols, r8
    imul rdx, cols
	mov len, rdx
while1L:
	mov r12, index
	mov rbx, len
	cmp r12, rbx
	jg end1w
	mov rdi, index
	mov r10, ind2
	mov r12, [rcx + rdi * 8]
	mov a_ii, r12
	movupd xmm0, REAL8 PTR [rcx + rdi * 8]
	movupd real8 ptr [r9 + r10 * 8], xmm0
	mov rbx, cols
	add index, rbx
	add ind2, 1
	add ind2, 1
	jmp while1L
end1w:
	ret

gaussEliminationMASM endp
end

;.data
;counter dw 0

; loop example
;whileL:
;	mov r12, index
;	mov rbx, len
;	cmp r12, rbx
;	jg endw ; daj not warunek
;	mov rdi, index
;	movupd xmm0, REAL8 PTR [rcx + rdi]
;	mulpd xmm0, xmm0
;	movupd REAL8 PTR [r9 + rdi], xmm0
;	add index, 16
;	jmp whileL na start
;endw:
;	ret

