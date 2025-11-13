; ==============================================================================
; BIOS INT 1A Time Services Test
; ==============================================================================
; This test verifies BIOS INT 1A time services (functions 00h-05h)
;
; Functions tested:
;   00h - Get System Clock Counter
;   01h - Set System Clock Counter
;   02h - Read RTC Time
;   03h - Set RTC Time (stub implementation)
;   04h - Read RTC Date
;   05h - Set RTC Date (stub implementation)
;
; Test Results Protocol:
;   Port 0x999 = Result port (0x00 = success, 0xFF = failure)
;   Port 0x998 = Details port (test number or error codes)
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
    ; Test 1: INT 1A Function 00h - Get System Clock Counter
    ; ==============================================================================
    ; Returns tick counter (18.2 Hz timer) in CX:DX
    ; AL = midnight flag (non-zero if midnight passed since last read)
    
    mov si, msg_test1
    call print_string
    
    mov ah, 0x00            ; Function 00h = Get tick count
    int 0x1A                ; BIOS time service
    
    ; Just verify the function didn't crash - any tick value is valid
    mov si, msg_pass
    call print_string
    call print_newline
    mov al, 0x01            ; Test 1 passed
    out 0x998, al
    
    ; ==============================================================================
    ; Test 2: INT 1A Function 01h - Set System Clock Counter
    ; ==============================================================================
    ; Sets tick counter to value in CX:DX
    
    mov si, msg_test2
    call print_string
    
    mov ah, 0x01            ; Function 01h = Set tick count
    mov cx, 0x1234          ; Set test value
    mov dx, 0x5678
    int 0x1A
    
    ; Read back to verify
    mov ah, 0x00            ; Function 00h = Get tick count
    int 0x1A
    
    ; Verify CX:DX matches what we set
    cmp cx, 0x1234
    jne test2_fail
    cmp dx, 0x5678
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
    ; Test 3: INT 1A Function 02h - Read RTC Time
    ; ==============================================================================
    ; Returns time in BCD format:
    ;   CH = Hours (0x00-0x23)
    ;   CL = Minutes (0x00-0x59)
    ;   DH = Seconds (0x00-0x59)
    ;   Carry flag clear if valid
    
test3:
    mov si, msg_test3
    call print_string
    
    mov ah, 0x02            ; Function 02h = Read RTC time
    int 0x1A
    
    jc test3_fail           ; Carry set = RTC not available
    
    ; Validate hours (BCD 0x00-0x23)
    mov al, ch
    call validate_bcd
    jc test3_fail
    cmp ch, 0x23
    ja test3_fail
    
    ; Validate minutes (BCD 0x00-0x59)
    mov al, cl
    call validate_bcd
    jc test3_fail
    cmp cl, 0x59
    ja test3_fail
    
    ; Validate seconds (BCD 0x00-0x59)
    mov al, dh
    call validate_bcd
    jc test3_fail
    cmp dh, 0x59
    ja test3_fail
    
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
    ; Test 4: INT 1A Function 03h - Set RTC Time
    ; ==============================================================================
    ; Sets RTC time (stub - just verify it doesn't crash)
    ; Input format:
    ;   CH = Hours (BCD)
    ;   CL = Minutes (BCD)
    ;   DH = Seconds (BCD)
    ;   DL = Daylight savings flag (00h standard, 01h daylight)
    
test4:
    mov si, msg_test4
    call print_string
    
    mov ah, 0x03            ; Function 03h = Set RTC time
    mov ch, 0x12            ; 12 hours
    mov cl, 0x34            ; 34 minutes
    mov dh, 0x56            ; 56 seconds
    mov dl, 0x00            ; Standard time
    int 0x1A
    
    ; If we got here without crashing, test passed
    mov si, msg_pass
    call print_string
    call print_newline
    mov al, 0x04            ; Test 4 passed
    out 0x998, al
    
    ; ==============================================================================
    ; Test 5: INT 1A Function 04h - Read RTC Date
    ; ==============================================================================
    ; Returns date in BCD format:
    ;   CH = Century (0x19-0x20)
    ;   CL = Year (0x00-0x99)
    ;   DH = Month (0x01-0x12)
    ;   DL = Day (0x01-0x31)
    ;   Carry flag clear if valid
    
test5:
    mov si, msg_test5
    call print_string
    
    mov ah, 0x04            ; Function 04h = Read RTC date
    int 0x1A
    
    jc test5_fail           ; Carry set = RTC not available
    
    ; Validate century (BCD 0x19-0x20 for 1900-2099)
    mov al, ch
    call validate_bcd
    jc test5_fail
    cmp ch, 0x19
    jb test5_fail
    cmp ch, 0x20
    ja test5_fail
    
    ; Validate year (BCD 0x00-0x99)
    mov al, cl
    call validate_bcd
    jc test5_fail
    cmp cl, 0x99
    ja test5_fail
    
    ; Validate month (BCD 0x01-0x12)
    mov al, dh
    call validate_bcd
    jc test5_fail
    cmp dh, 0x00
    je test5_fail
    cmp dh, 0x12
    ja test5_fail
    
    ; Validate day (BCD 0x01-0x31)
    mov al, dl
    call validate_bcd
    jc test5_fail
    cmp dl, 0x00
    je test5_fail
    cmp dl, 0x31
    ja test5_fail
    
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
    ; Test 6: INT 1A Function 05h - Set RTC Date
    ; ==============================================================================
    ; Sets RTC date (stub - just verify it doesn't crash)
    ; Input format:
    ;   CH = Century (BCD)
    ;   CL = Year (BCD)
    ;   DH = Month (BCD)
    ;   DL = Day (BCD)
    
test6:
    mov si, msg_test6
    call print_string
    
    mov ah, 0x05            ; Function 05h = Set RTC date
    mov ch, 0x20            ; Century 20
    mov cl, 0x25            ; Year 25 (2025)
    mov dh, 0x11            ; Month 11 (November)
    mov dl, 0x13            ; Day 13
    int 0x1A
    
    ; If we got here without crashing, test passed
    mov si, msg_pass
    call print_string
    call print_newline
    mov al, 0x06            ; Test 6 passed
    out 0x998, al
    
all_tests_passed:
    call print_newline
    mov si, msg_all_pass
    call print_string
    call print_newline
    mov al, 0x00            ; All tests passed
    out 0x999, al
    mov al, 0x06            ; 6 tests completed
    out 0x998, al

exit_program:
    mov ax, 0x4C00          ; DOS terminate
    int 0x21

; ==============================================================================
; validate_bcd - Validates that AL contains a valid BCD value
; ==============================================================================
; Input: AL = value to validate
; Output: Carry flag set if invalid, clear if valid
; Preserves: AL
; ==============================================================================
validate_bcd:
    push ax
    push bx
    
    ; Check high nibble
    mov bl, al
    shr bl, 4
    cmp bl, 9
    ja .invalid
    
    ; Check low nibble
    mov bl, al
    and bl, 0x0F
    cmp bl, 9
    ja .invalid
    
.valid:
    pop bx
    pop ax
    clc
    ret

.invalid:
    pop bx
    pop ax
    stc
    ret

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

msg_header      db 'BIOS INT 1A Time Services Test', 0
msg_test1       db 'Test 1: Get System Clock Counter...', 0
msg_test2       db 'Test 2: Set System Clock Counter...', 0
msg_test3       db 'Test 3: Read RTC Time...', 0
msg_test4       db 'Test 4: Set RTC Time (stub)...', 0
msg_test5       db 'Test 5: Read RTC Date...', 0
msg_test6       db 'Test 6: Set RTC Date (stub)...', 0
msg_pass        db ' PASS', 0
msg_fail        db ' FAIL', 0
msg_all_pass    db 'All tests PASSED!', 0
