; ==============================================================================
; CMOS RTC Direct Port Access Test
; ==============================================================================
; This test verifies direct access to the CMOS/RTC hardware via I/O ports
; 0x70 (address/index port) and 0x71 (data port).
;
; The MC146818 CMOS/RTC chip stores time/date information and configuration
; in various registers. This test validates that we can read time/date values
; from the CMOS and that they are in valid BCD format.
;
; Test Results Protocol:
;   Port 0x999 = Result port (0x00 = success, 0xFF = failure)
;   Port 0x998 = Details port (error codes or diagnostic info)
;   Screen: Visual output via INT 10h (text mode)
;
; Expected behavior:
;   - CMOS registers should return valid BCD values (both nibbles 0-9)
;   - Time values should be in valid ranges (hours 0-23, minutes/seconds 0-59)
;   - Date values should be in valid ranges (month 1-12, day 1-31)
; ==============================================================================

org 100h                    ; COM file starts at offset 100h

section .text

start:
    ; Clear screen and print header
    call clear_screen
    mov si, msg_header
    call print_string
    call print_newline
    
    ; ==============================================================================
    ; Test 1: Read CMOS Seconds Register (0x00)
    ; ==============================================================================
    mov si, msg_test1
    call print_string
    
    mov al, 0x00            ; Register 0x00 = Seconds
    out 0x70, al            ; Select register via address port
    in al, 0x71             ; Read value from data port
    
    ; Validate BCD format (both nibbles should be 0-9)
    call validate_bcd
    jc test1_fail
    
    ; Validate seconds range (0-59 in BCD = 0x00-0x59)
    cmp al, 0x59
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
    mov al, 0xFF            ; Test failed
    out 0x999, al
    mov al, 0x01            ; Test number
    out 0x998, al
    jmp exit_program

    ; ==============================================================================
    ; Test 2: Read CMOS Minutes Register (0x02)
    ; ==============================================================================
test2:
    mov si, msg_test2
    call print_string
    
    mov al, 0x02            ; Register 0x02 = Minutes
    out 0x70, al
    in al, 0x71
    
    ; Validate BCD format
    call validate_bcd
    jc test2_fail
    
    ; Validate minutes range (0-59 in BCD)
    cmp al, 0x59
    ja test2_fail
    
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
    ; Test 3: Read CMOS Hours Register (0x04)
    ; ==============================================================================
test3:
    mov si, msg_test3
    call print_string
    
    mov al, 0x04            ; Register 0x04 = Hours
    out 0x70, al
    in al, 0x71
    
    ; Validate BCD format
    call validate_bcd
    jc test3_fail
    
    ; Validate hours range (0-23 in BCD = 0x00-0x23)
    cmp al, 0x23
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
    ; Test 4: Read CMOS Day of Month Register (0x07)
    ; ==============================================================================
test4:
    mov si, msg_test4
    call print_string
    
    mov al, 0x07            ; Register 0x07 = Day of Month
    out 0x70, al
    in al, 0x71
    
    ; Validate BCD format
    call validate_bcd
    jc test4_fail
    
    ; Validate day range (1-31 in BCD = 0x01-0x31)
    cmp al, 0x00
    je test4_fail           ; Day cannot be 0
    cmp al, 0x31
    ja test4_fail
    
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
    ; Test 5: Read CMOS Month Register (0x08)
    ; ==============================================================================
test5:
    mov si, msg_test5
    call print_string
    
    mov al, 0x08            ; Register 0x08 = Month
    out 0x70, al
    in al, 0x71
    
    ; Validate BCD format
    call validate_bcd
    jc test5_fail
    
    ; Validate month range (1-12 in BCD = 0x01-0x12)
    cmp al, 0x00
    je test5_fail           ; Month cannot be 0
    cmp al, 0x12
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
    ; Test 6: Read CMOS Year Register (0x09)
    ; ==============================================================================
test6:
    mov si, msg_test6
    call print_string
    
    mov al, 0x09            ; Register 0x09 = Year
    out 0x70, al
    in al, 0x71
    
    ; Validate BCD format
    call validate_bcd
    jc test6_fail
    
    ; Validate year range (0-99 in BCD = 0x00-0x99)
    cmp al, 0x99
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
    ; Test 7: Read CMOS Century Register (0x32)
    ; ==============================================================================
test7:
    mov si, msg_test7
    call print_string
    
    mov al, 0x32            ; Register 0x32 = Century
    out 0x70, al
    in al, 0x71
    
    ; Validate BCD format
    call validate_bcd
    jc test7_fail
    
    ; Validate century range (19-20 in BCD for years 1900-2099)
    cmp al, 0x19
    jb test7_fail
    cmp al, 0x20
    ja test7_fail
    
test7_pass:
    mov si, msg_pass
    call print_string
    call print_newline
    mov al, 0x07            ; Test 7 passed
    out 0x998, al
    jmp all_tests_passed

test7_fail:
    mov si, msg_fail
    call print_string
    call print_newline
    mov al, 0xFF
    out 0x999, al
    mov al, 0x07
    out 0x998, al
    jmp exit_program

all_tests_passed:
    call print_newline
    mov si, msg_all_pass
    call print_string
    call print_newline
    mov al, 0x00            ; All tests passed (success code)
    out 0x999, al
    mov al, 0x07            ; 7 tests completed
    out 0x998, al

exit_program:
    mov ax, 0x4C00          ; DOS terminate program
    int 0x21

; ==============================================================================
; validate_bcd - Validates that AL contains a valid BCD value
; ==============================================================================
; Input: AL = value to validate
; Output: Carry flag set if invalid, clear if valid
; Preserves: AL
;
; A valid BCD byte has both nibbles in range 0-9
; ==============================================================================
validate_bcd:
    push ax
    push bx
    
    ; Check high nibble (bits 4-7)
    mov bl, al
    shr bl, 4               ; Shift high nibble to low position
    cmp bl, 9
    ja .invalid             ; If > 9, invalid BCD
    
    ; Check low nibble (bits 0-3)
    mov bl, al
    and bl, 0x0F            ; Mask low nibble
    cmp bl, 9
    ja .invalid             ; If > 9, invalid BCD
    
.valid:
    pop bx
    pop ax
    clc                     ; Clear carry = valid
    ret

.invalid:
    pop bx
    pop ax
    stc                     ; Set carry = invalid
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

msg_header      db 'CMOS RTC Direct Port Access Test', 0
msg_test1       db 'Test 1: Read Seconds Register (0x00)...', 0
msg_test2       db 'Test 2: Read Minutes Register (0x02)...', 0
msg_test3       db 'Test 3: Read Hours Register (0x04)...', 0
msg_test4       db 'Test 4: Read Day Register (0x07)...', 0
msg_test5       db 'Test 5: Read Month Register (0x08)...', 0
msg_test6       db 'Test 6: Read Year Register (0x09)...', 0
msg_test7       db 'Test 7: Read Century Register (0x32)...', 0
msg_pass        db ' PASS', 0
msg_fail        db ' FAIL', 0
msg_all_pass    db 'All tests PASSED!', 0
