; ==============================================================================
; DOS INT 21H Date/Time Functions Test
; ==============================================================================
; This test verifies DOS INT 21H date/time functions (2Ah-2Dh)
;
; Functions tested:
;   2Ah - Get DOS Date
;   2Bh - Set DOS Date (valid + error cases)
;   2Ch - Get DOS Time
;   2Dh - Set DOS Time (valid + error cases)
;
; Test Results Protocol:
;   Port 0x999 = Result port (0x00 = success, 0xFF = failure)
;   Port 0x998 = Details port (test number)
;   Screen: Visual output via INT 10h (text mode)
; ==============================================================================

org 100h

section .text

start:
    ; Clear screen and print header
    call clear_screen
    mov si, msg_header
    call print_string
    call print_newline
    call print_newline
    
    ; ==============================================================================
    ; Test 1: INT 21H Function 2Ah - Get DOS Date
    ; ==============================================================================
    ; Returns:
    ;   CX = Year (1980-2099)
    ;   DH = Month (1-12)
    ;   DL = Day (1-31)
    ;   AL = Day of week (0=Sunday, 6=Saturday)
    
    mov si, msg_test1
    call print_string
    
    mov ah, 0x2A            ; Function 2Ah = Get date
    int 0x21
    
    ; Validate year (1980-2099)
    cmp cx, 1980
    jb test1_fail
    cmp cx, 2099
    ja test1_fail
    
    ; Validate month (1-12)
    cmp dh, 1
    jb test1_fail
    cmp dh, 12
    ja test1_fail
    
    ; Validate day (1-31)
    cmp dl, 1
    jb test1_fail
    cmp dl, 31
    ja test1_fail
    
    ; Validate day of week (0-6)
    cmp al, 6
    ja test1_fail
    
test1_pass:
    mov si, msg_pass
    call print_string
    call print_newline
    mov al, 0x01            ; Test 1 passed
    out 0x998, al
    jmp test2

test1_fail:
    mov si, msg_fail
    call print_string
    call print_newline
    mov al, 0xFF
    out 0x999, al
    mov al, 0x01
    out 0x998, al
    jmp exit_program

    ; ==============================================================================
    ; Test 2: INT 21H Function 2Bh - Set DOS Date (Valid)
    ; ==============================================================================
    ; Input:
    ;   CX = Year (1980-2099)
    ;   DH = Month (1-12)
    ;   DL = Day (1-31)
    ; Returns:
    ;   AL = 00h if successful, FFh if invalid
    
test2:
    mov si, msg_test2
    call print_string
        mov ah, 0x2B            ; Function 2Bh = Set date
    mov cx, 2025            ; Year 2025
    mov dh, 11              ; November
    mov dl, 13              ; 13th
    int 0x21
    
    cmp al, 0x00            ; Should return 0 for valid date
    jne test2_fail
    
test2_pass:
    mov si, msg_pass
    call print_string
    call print_newline
    mov al, 0x02            ; Test 2 passed
    out 0x998, al
    jmp test3

test2_fail:
    mov si, msg_fail
    call print_string
    call print_newline
    mov al, 0xFF
    out 0x999, al
    mov al, 0x02
    out 0x998, al
    jmp exit_program

    ; ==============================================================================
    ; Test 3: INT 21H Function 2Bh - Set DOS Date (Invalid Year)
    ; ==============================================================================
    
test3:
    mov si, msg_test3
    call print_string
        mov ah, 0x2B            ; Function 2Bh = Set date
    mov cx, 1979            ; Invalid year (< 1980)
    mov dh, 11
    mov dl, 13
    int 0x21
    
    cmp al, 0xFF            ; Should return FFh for invalid date
    jne test3_fail
    
test3_pass:
    mov si, msg_pass
    call print_string
    call print_newline
    mov al, 0x03            ; Test 3 passed
    out 0x998, al
    jmp test4

test3_fail:
    mov si, msg_fail
    call print_string
    call print_newline
    mov al, 0xFF
    out 0x999, al
    mov al, 0x03
    out 0x998, al
    jmp exit_program

    ; ==============================================================================
    ; Test 4: INT 21H Function 2Bh - Set DOS Date (Invalid Month)
    ; ==============================================================================
    
test4:
    mov si, msg_test4
    call print_string
        mov ah, 0x2B
    mov cx, 2025
    mov dh, 13              ; Invalid month (> 12)
    mov dl, 13
    int 0x21
    
    cmp al, 0xFF            ; Should return FFh
    jne test4_fail
    
test4_pass:
    mov si, msg_pass
    call print_string
    call print_newline
    mov al, 0x04            ; Test 4 passed
    out 0x998, al
    jmp test5

test4_fail:
    mov si, msg_fail
    call print_string
    call print_newline
    mov al, 0xFF
    out 0x999, al
    mov al, 0x04
    out 0x998, al
    jmp exit_program

    ; ==============================================================================
    ; Test 5: INT 21H Function 2Bh - Set DOS Date (Invalid Day)
    ; ==============================================================================
    
test5:
    mov si, msg_test5
    call print_string
        mov ah, 0x2B
    mov cx, 2025
    mov dh, 11
    mov dl, 32              ; Invalid day (> 31)
    int 0x21
    
    cmp al, 0xFF            ; Should return FFh
    jne test5_fail
    
test5_pass:
    mov si, msg_pass
    call print_string
    call print_newline
    mov al, 0x05            ; Test 5 passed
    out 0x998, al
    jmp test6

test5_fail:
    mov si, msg_fail
    call print_string
    call print_newline
    mov al, 0xFF
    out 0x999, al
    mov al, 0x05
    out 0x998, al
    jmp exit_program

    ; ==============================================================================
    ; Test 6: INT 21H Function 2Ch - Get DOS Time
    ; ==============================================================================
    ; Returns:
    ;   CH = Hour (0-23)
    ;   CL = Minutes (0-59)
    ;   DH = Seconds (0-59)
    ;   DL = Hundredths (0-99)
    
test6:
    mov si, msg_test6
    call print_string
        mov ah, 0x2C            ; Function 2Ch = Get time
    int 0x21
    
    ; Validate hour (0-23)
    cmp ch, 23
    ja test6_fail
    
    ; Validate minutes (0-59)
    cmp cl, 59
    ja test6_fail
    
    ; Validate seconds (0-59)
    cmp dh, 59
    ja test6_fail
    
    ; Validate hundredths (0-99)
    cmp dl, 99
    ja test6_fail
    
test6_pass:
    mov si, msg_pass
    call print_string
    call print_newline
    mov al, 0x06            ; Test 6 passed
    out 0x998, al
    jmp test7

test6_fail:
    mov si, msg_fail
    call print_string
    call print_newline
    mov al, 0xFF
    out 0x999, al
    mov al, 0x06
    out 0x998, al
    jmp exit_program

    ; ==============================================================================
    ; Test 7: INT 21H Function 2Dh - Set DOS Time (Valid)
    ; ==============================================================================
    ; Input:
    ;   CH = Hour (0-23)
    ;   CL = Minutes (0-59)
    ;   DH = Seconds (0-59)
    ;   DL = Hundredths (0-99)
    ; Returns:
    ;   AL = 00h if successful, FFh if invalid
    
test7:
    mov si, msg_test7
    call print_string
        mov ah, 0x2D            ; Function 2Dh = Set time
    mov ch, 12              ; 12 hours
    mov cl, 30              ; 30 minutes
    mov dh, 45              ; 45 seconds
    mov dl, 50              ; 50 hundredths
    int 0x21
    
    cmp al, 0x00            ; Should return 0 for valid time
    jne test7_fail
    
test7_pass:
    mov si, msg_pass
    call print_string
    call print_newline
    mov al, 0x07            ; Test 7 passed
    out 0x998, al
    jmp test8

test7_fail:
    mov si, msg_fail
    call print_string
    call print_newline
    mov al, 0xFF
    out 0x999, al
    mov al, 0x07
    out 0x998, al
    jmp exit_program

    ; ==============================================================================
    ; Test 8: INT 21H Function 2Dh - Set DOS Time (Invalid Hour)
    ; ==============================================================================
    
test8:
    mov si, msg_test8
    call print_string
        mov ah, 0x2D
    mov ch, 24              ; Invalid hour (>= 24)
    mov cl, 30
    mov dh, 45
    mov dl, 50
    int 0x21
    
    cmp al, 0xFF            ; Should return FFh
    jne test8_fail
    
test8_pass:
    mov si, msg_pass
    call print_string
    call print_newline
    mov al, 0x08            ; Test 8 passed
    out 0x998, al
    jmp test9

test8_fail:
    mov si, msg_fail
    call print_string
    call print_newline
    mov al, 0xFF
    out 0x999, al
    mov al, 0x08
    out 0x998, al
    jmp exit_program

    ; ==============================================================================
    ; Test 9: INT 21H Function 2Dh - Set DOS Time (Invalid Minutes)
    ; ==============================================================================
    
test9:
    mov si, msg_test9
    call print_string
        mov ah, 0x2D
    mov ch, 12
    mov cl, 60              ; Invalid minutes (>= 60)
    mov dh, 45
    mov dl, 50
    int 0x21
    
    cmp al, 0xFF            ; Should return FFh
    jne test9_fail
    
test9_pass:
    mov si, msg_pass
    call print_string
    call print_newline
    mov al, 0x09            ; Test 9 passed
    out 0x998, al
    jmp test10

test9_fail:
    mov si, msg_fail
    call print_string
    call print_newline
    mov al, 0xFF
    out 0x999, al
    mov al, 0x09
    out 0x998, al
    jmp exit_program

    ; ==============================================================================
    ; Test 10: INT 21H Function 2Dh - Set DOS Time (Invalid Seconds)
    ; ==============================================================================
    
test10:
    mov si, msg_test10
    call print_string
        mov ah, 0x2D
    mov ch, 12
    mov cl, 30
    mov dh, 60              ; Invalid seconds (>= 60)
    mov dl, 50
    int 0x21
    
    cmp al, 0xFF            ; Should return FFh
    jne test10_fail
    
test10_pass:
    mov si, msg_pass
    call print_string
    call print_newline
    mov al, 0x0A            ; Test 10 passed
    out 0x998, al
    jmp test11

test10_fail:
    mov si, msg_fail
    call print_string
    call print_newline
    mov al, 0xFF
    out 0x999, al
    mov al, 0x0A
    out 0x998, al
    jmp exit_program

    ; ==============================================================================
    ; Test 11: INT 21H Function 2Dh - Set DOS Time (Invalid Hundredths)
    ; ==============================================================================
    
test11:
    mov si, msg_test11
    call print_string
        mov ah, 0x2D
    mov ch, 12
    mov cl, 30
    mov dh, 45
    mov dl, 100             ; Invalid hundredths (>= 100)
    int 0x21
    
    cmp al, 0xFF            ; Should return FFh
    jne test11_fail
    
test11_pass:
    mov si, msg_pass
    call print_string
    call print_newline
    mov al, 0x0B            ; Test 11 passed
    out 0x998, al
    jmp all_tests_passed

test11_fail:
    mov si, msg_fail
    call print_string
    call print_newline
    mov al, 0xFF
    out 0x999, al
    mov al, 0x0B
    out 0x998, al
    jmp exit_program

all_tests_passed:
    call print_newline
    mov si, msg_all_pass
    call print_string
    call print_newline
    mov al, 0x00            ; All tests passed
    out 0x999, al
    mov al, 0x0B            ; 11 tests completed
    out 0x998, al

exit_program:
    mov ax, 0x4C00          ; DOS terminate
    int 0x21

; ==============================================================================
; Helper Functions for Screen Output
; ==============================================================================

; ==============================================================================
; clear_screen - Clears the screen using INT 10h
; ==============================================================================
clear_screen:
    push ax
    mov ax, 0x0003          ; Set video mode 3 (80x25 text, clears screen)
    int 0x10
    pop ax
    ret

; ==============================================================================
; print_string - Prints a null-terminated string using INT 10h
; ==============================================================================
; Input: SI = pointer to null-terminated string
; Modifies: AX, BX, SI
; ==============================================================================
print_string:
    push ax
    push bx
.loop:
    lodsb                   ; Load byte from [SI] into AL, increment SI
    cmp al, 0               ; Check for null terminator
    je .done
    mov ah, 0x0E            ; BIOS teletype output
    mov bx, 0x0007          ; Page 0, light gray color
    int 0x10
    jmp .loop
.done:
    pop bx
    pop ax
    ret

; ==============================================================================
; print_newline - Prints CR+LF
; ==============================================================================
print_newline:
    push ax
    push bx
    mov ah, 0x0E
    mov bx, 0x0007
    mov al, 13              ; Carriage return
    int 0x10
    mov al, 10              ; Line feed
    int 0x10
    pop bx
    pop ax
    ret

; ==============================================================================
; Data section - Test messages
; ==============================================================================
section .data

msg_header      db 'DOS INT 21H Date/Time Functions Test', 0
msg_test1       db 'Test 1: Get DOS Date (2Ah)...', 0
msg_test2       db 'Test 2: Set DOS Date - Valid (2Bh)...', 0
msg_test3       db 'Test 3: Set DOS Date - Invalid Year...', 0
msg_test4       db 'Test 4: Set DOS Date - Invalid Month...', 0
msg_test5       db 'Test 5: Set DOS Date - Invalid Day...', 0
msg_test6       db 'Test 6: Get DOS Time (2Ch)...', 0
msg_test7       db 'Test 7: Set DOS Time - Valid (2Dh)...', 0
msg_test8       db 'Test 8: Set DOS Time - Invalid Hour...', 0
msg_test9       db 'Test 9: Set DOS Time - Invalid Minutes...', 0
msg_test10      db 'Test 10: Set DOS Time - Invalid Seconds...', 0
msg_test11      db 'Test 11: Set DOS Time - Invalid Hundredths...', 0
msg_pass        db ' PASS', 0
msg_fail        db ' FAIL', 0
msg_all_pass    db 'All tests PASSED!', 0
