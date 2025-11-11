; Performance Test Binary for Spice86
; This program performs various math-heavy operations and reports results via port I/O
; Uses ports 0x90-0x9F for performance reporting
;
; Port Mapping:
; 0x90: Test ID (byte)
; 0x91: Cycle count low word (word)
; 0x93: Cycle count high word (word)  
; 0x95: Result low word (word)
; 0x97: Result high word (word)
; 0x99: Status (0=running, 1=complete, 0xFF=error)

BITS 16
ORG 0x0100          ; COM file format

section .text
start:
    ; Initialize
    mov ax, cs
    mov ds, ax
    mov es, ax
    
    ; Run all performance tests
    call test_integer_arithmetic
    call test_multiplication
    call test_division
    call test_bit_manipulation
    call test_loop_performance
    
    ; Signal completion and exit
    mov al, 0xFF        ; All tests complete signal
    mov dx, 0x90
    out dx, al
    
    ; Exit to DOS
    mov ah, 0x4C
    xor al, al
    int 0x21

;=============================================================================
; Test 1: Integer Arithmetic (ADD/SUB operations)
;=============================================================================
test_integer_arithmetic:
    push bp
    mov bp, sp
    
    ; Report test start
    mov al, 1           ; Test ID
    mov dx, 0x90
    out dx, al
    
    ; Get start "cycle" count (use loop counter as proxy)
    xor cx, cx
    mov si, 0           ; Result accumulator
    
    ; Perform 10000 iterations of arithmetic
    mov bx, 10000
.loop:
    add si, bx
    sub si, 5
    add si, 10
    sub si, 3
    add si, bx
    inc cx
    dec bx
    jnz .loop
    
    ; Report results
    mov ax, cx          ; Cycle count (iterations)
    mov dx, 0x91
    out dx, ax          ; Low word of cycles
    xor ax, ax
    mov dx, 0x93
    out dx, ax          ; High word of cycles (0)
    
    mov ax, si          ; Result low
    mov dx, 0x95
    out dx, ax
    xor ax, ax
    mov dx, 0x97
    out dx, ax          ; Result high (0)
    
    mov al, 1           ; Status: complete
    mov dx, 0x99
    out dx, al
    
    mov sp, bp
    pop bp
    ret

;=============================================================================
; Test 2: Multiplication Operations
;=============================================================================
test_multiplication:
    push bp
    mov bp, sp
    
    ; Report test start
    mov al, 2           ; Test ID
    mov dx, 0x90
    out dx, al
    
    xor cx, cx
    xor di, di          ; Result accumulator
    
    ; Perform 5000 multiplication operations
    mov bx, 5000
.loop:
    mov ax, bx
    mov dx, 13          ; Multiply by 13
    mul dx
    add di, ax
    inc cx
    dec bx
    jnz .loop
    
    ; Report results
    mov ax, cx
    mov dx, 0x91
    out dx, ax
    xor ax, ax
    mov dx, 0x93
    out dx, ax
    
    mov ax, di
    mov dx, 0x95
    out dx, ax
    xor ax, ax
    mov dx, 0x97
    out dx, ax
    
    mov al, 1
    mov dx, 0x99
    out dx, al
    
    mov sp, bp
    pop bp
    ret

;=============================================================================
; Test 3: Division Operations
;=============================================================================
test_division:
    push bp
    mov bp, sp
    
    ; Report test start
    mov al, 3           ; Test ID
    mov dx, 0x90
    out dx, al
    
    xor cx, cx
    xor di, di          ; Result accumulator
    
    ; Perform 3000 division operations
    mov bx, 60000
.loop:
    mov ax, bx
    xor dx, dx          ; Clear DX before division (DX:AX dividend)
    mov si, 7           ; Divisor
    div si              ; Divide DX:AX by SI, result in AX
    add di, ax          ; Add quotient
    inc cx
    sub bx, 20
    cmp bx, 20
    jae .loop
    
    ; Report results
    mov ax, cx
    mov dx, 0x91
    out dx, ax
    xor ax, ax
    mov dx, 0x93
    out dx, ax
    
    mov ax, di
    mov dx, 0x95
    out dx, ax
    xor ax, ax
    mov dx, 0x97
    out dx, ax
    
    mov al, 1
    mov dx, 0x99
    out dx, al
    
    mov sp, bp
    pop bp
    ret

;=============================================================================
; Test 4: Bit Manipulation
;=============================================================================
test_bit_manipulation:
    push bp
    mov bp, sp
    
    ; Report test start
    mov al, 4           ; Test ID
    mov dx, 0x90
    out dx, al
    
    xor cx, cx
    mov di, 0xAAAA      ; Initial pattern
    
    ; Perform 8000 bit operations
    mov bx, 8000
.loop:
    rol di, 1
    xor di, 0x5555
    ror di, 1
    and di, 0xFFFF
    or di, 0x1
    not di
    not di
    inc cx
    dec bx
    jnz .loop
    
    ; Report results
    mov ax, cx
    mov dx, 0x91
    out dx, ax
    xor ax, ax
    mov dx, 0x93
    out dx, ax
    
    mov ax, di
    mov dx, 0x95
    out dx, ax
    xor ax, ax
    mov dx, 0x97
    out dx, ax
    
    mov al, 1
    mov dx, 0x99
    out dx, al
    
    mov sp, bp
    pop bp
    ret

;=============================================================================
; Test 5: Loop and Branch Performance
;=============================================================================
test_loop_performance:
    push bp
    mov bp, sp
    
    ; Report test start
    mov al, 5           ; Test ID
    mov dx, 0x90
    out dx, al
    
    xor cx, cx
    xor di, di
    
    ; Nested loops for branch testing
    mov bx, 100
.outer_loop:
    push bx
    mov bx, 100
.inner_loop:
    inc di
    inc cx
    cmp di, 5000
    jbe .skip
    sub di, 5000
.skip:
    dec bx
    jnz .inner_loop
    pop bx
    dec bx
    jnz .outer_loop
    
    ; Report results
    mov ax, cx
    mov dx, 0x91
    out dx, ax
    xor ax, ax
    mov dx, 0x93
    out dx, ax
    
    mov ax, di
    mov dx, 0x95
    out dx, ax
    xor ax, ax
    mov dx, 0x97
    out dx, ax
    
    mov al, 1
    mov dx, 0x99
    out dx, al
    
    mov sp, bp
    pop bp
    ret
