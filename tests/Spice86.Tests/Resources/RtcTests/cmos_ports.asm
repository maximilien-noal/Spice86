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
;
; Expected behavior:
;   - CMOS registers should return valid BCD values (both nibbles 0-9)
;   - Time values should be in valid ranges (hours 0-23, minutes/seconds 0-59)
;   - Date values should be in valid ranges (month 1-12, day 1-31)
; ==============================================================================

org 100h                    ; COM file starts at offset 100h

section .text

start:
    ; ==============================================================================
    ; Test 1: Read CMOS Seconds Register (0x00)
    ; ==============================================================================
    ; The seconds register should contain a value in BCD format (0x00-0x59)
    ; This tests basic CMOS read functionality
    
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
    mov al, 0x01            ; Test 1 passed
    out 0x998, al
    jmp test2

test1_fail:
    mov al, 0xFF            ; Test failed
    out 0x999, al
    mov al, 0x01            ; Test number
    out 0x998, al
    jmp exit_program

    ; ==============================================================================
    ; Test 2: Read CMOS Minutes Register (0x02)
    ; ==============================================================================
    ; The minutes register should contain a value in BCD format (0x00-0x59)
    
test2:
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
    mov al, 0x02            ; Test 2 passed
    out 0x998, al
    jmp test3

test2_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x02
    out 0x998, al
    jmp exit_program

    ; ==============================================================================
    ; Test 3: Read CMOS Hours Register (0x04)
    ; ==============================================================================
    ; The hours register should contain a value in BCD format (0x00-0x23)
    
test3:
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
    mov al, 0x03            ; Test 3 passed
    out 0x998, al
    jmp test4

test3_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x03
    out 0x998, al
    jmp exit_program

    ; ==============================================================================
    ; Test 4: Read CMOS Day of Month Register (0x07)
    ; ==============================================================================
    ; The day register should contain a value in BCD format (0x01-0x31)
    
test4:
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
    mov al, 0x04            ; Test 4 passed
    out 0x998, al
    jmp test5

test4_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x04
    out 0x998, al
    jmp exit_program

    ; ==============================================================================
    ; Test 5: Read CMOS Month Register (0x08)
    ; ==============================================================================
    ; The month register should contain a value in BCD format (0x01-0x12)
    
test5:
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
    mov al, 0x05            ; Test 5 passed
    out 0x998, al
    jmp test6

test5_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x05
    out 0x998, al
    jmp exit_program

    ; ==============================================================================
    ; Test 6: Read CMOS Year Register (0x09)
    ; ==============================================================================
    ; The year register should contain a value in BCD format (0x00-0x99)
    
test6:
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
    mov al, 0x06            ; Test 6 passed
    out 0x998, al
    jmp test7

test6_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x06
    out 0x998, al
    jmp exit_program

    ; ==============================================================================
    ; Test 7: Read CMOS Century Register (0x32)
    ; ==============================================================================
    ; The century register should contain a value in BCD format (typically 0x19-0x20)
    
test7:
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
    mov al, 0x07            ; Test 7 passed
    out 0x998, al
    jmp all_tests_passed

test7_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x07
    out 0x998, al
    jmp exit_program

all_tests_passed:
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
