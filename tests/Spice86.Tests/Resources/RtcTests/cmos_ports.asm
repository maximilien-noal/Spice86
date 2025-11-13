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

.model small

my_code_seg segment
start:
    ; ==============================================================================
    ; Test 1: Read CMOS Seconds Register (0x00)
    ; ==============================================================================
    ; The seconds register should contain a value in BCD format (0x00-0x59)
    ; This tests basic CMOS read functionality
    
    mov al, 0x00            ; Select register 0x00 (Seconds)
    out 0x70, al            ; Write to CMOS address port
    in al, 0x71             ; Read from CMOS data port
    
    ; Validate that the value is valid BCD (both nibbles 0-9)
    mov bl, al              ; Save the value
    and al, 0x0F            ; Check low nibble
    cmp al, 0x09
    ja test1_fail           ; Low nibble > 9 is invalid
    
    mov al, bl              ; Restore value
    shr al, 4               ; Check high nibble
    cmp al, 0x05            ; Seconds high nibble should be 0-5
    ja test1_fail
    
    ; Seconds read successfully
    mov al, 0x01            ; Test 1 passed marker
    out 0x998, al
    jmp test2
    
test1_fail:
    mov al, 0xFF            ; Failure
    out 0x999, al
    mov al, 0x01            ; Test 1 failed
    out 0x998, al
    jmp test_end

    ; ==============================================================================
    ; Test 2: Read CMOS Minutes Register (0x02)
    ; ==============================================================================
test2:
    mov al, 0x02            ; Select register 0x02 (Minutes)
    out 0x70, al
    in al, 0x71
    
    ; Validate BCD format (0x00-0x59)
    mov bl, al
    and al, 0x0F            ; Check low nibble
    cmp al, 0x09
    ja test2_fail
    
    mov al, bl
    shr al, 4               ; Check high nibble
    cmp al, 0x05            ; Minutes high nibble should be 0-5
    ja test2_fail
    
    mov al, 0x02            ; Test 2 passed marker
    out 0x998, al
    jmp test3
    
test2_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x02
    out 0x998, al
    jmp test_end

    ; ==============================================================================
    ; Test 3: Read CMOS Hours Register (0x04)
    ; ==============================================================================
test3:
    mov al, 0x04            ; Select register 0x04 (Hours)
    out 0x70, al
    in al, 0x71
    
    ; Validate BCD format (0x00-0x23 for 24-hour format)
    mov bl, al
    and al, 0x0F            ; Check low nibble
    cmp al, 0x09
    ja test3_fail
    
    mov al, bl
    shr al, 4               ; Check high nibble
    cmp al, 0x02            ; Hours high nibble should be 0-2
    ja test3_fail
    
    ; Additional check: if high nibble is 2, low nibble must be 0-3
    cmp al, 0x02
    jne test3_ok
    mov al, bl
    and al, 0x0F
    cmp al, 0x03
    ja test3_fail
    
test3_ok:
    mov al, 0x03            ; Test 3 passed marker
    out 0x998, al
    jmp test4
    
test3_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x03
    out 0x998, al
    jmp test_end

    ; ==============================================================================
    ; Test 4: Read CMOS Day of Month Register (0x07)
    ; ==============================================================================
test4:
    mov al, 0x07            ; Select register 0x07 (Day of Month)
    out 0x70, al
    in al, 0x71
    
    ; Validate BCD format (0x01-0x31)
    mov bl, al
    
    ; Check if value is at least 0x01
    cmp bl, 0x01
    jb test4_fail
    
    ; Check low nibble
    and al, 0x0F
    cmp al, 0x09
    ja test4_fail
    
    ; Check high nibble (should be 0-3)
    mov al, bl
    shr al, 4
    cmp al, 0x03
    ja test4_fail
    
    mov al, 0x04            ; Test 4 passed marker
    out 0x998, al
    jmp test5
    
test4_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x04
    out 0x998, al
    jmp test_end

    ; ==============================================================================
    ; Test 5: Read CMOS Month Register (0x08)
    ; ==============================================================================
test5:
    mov al, 0x08            ; Select register 0x08 (Month)
    out 0x70, al
    in al, 0x71
    
    ; Validate BCD format (0x01-0x12)
    mov bl, al
    
    ; Check if value is at least 0x01
    cmp bl, 0x01
    jb test5_fail
    
    ; Check if value is at most 0x12
    cmp bl, 0x12
    ja test5_fail
    
    ; Check low nibble
    and al, 0x0F
    cmp al, 0x09
    ja test5_fail
    
    ; Check high nibble (should be 0-1)
    mov al, bl
    shr al, 4
    cmp al, 0x01
    ja test5_fail
    
    mov al, 0x05            ; Test 5 passed marker
    out 0x998, al
    jmp test6
    
test5_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x05
    out 0x998, al
    jmp test_end

    ; ==============================================================================
    ; Test 6: Read CMOS Year Register (0x09)
    ; ==============================================================================
test6:
    mov al, 0x09            ; Select register 0x09 (Year)
    out 0x70, al
    in al, 0x71
    
    ; Validate BCD format (0x00-0x99)
    mov bl, al
    and al, 0x0F            ; Check low nibble
    cmp al, 0x09
    ja test6_fail
    
    mov al, bl
    shr al, 4               ; Check high nibble
    cmp al, 0x09
    ja test6_fail
    
    mov al, 0x06            ; Test 6 passed marker
    out 0x998, al
    jmp test7
    
test6_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x06
    out 0x998, al
    jmp test_end

    ; ==============================================================================
    ; Test 7: Read CMOS Century Register (0x32)
    ; ==============================================================================
test7:
    mov al, 0x32            ; Select register 0x32 (Century)
    out 0x70, al
    in al, 0x71
    
    ; Validate BCD format (should be 0x19 or 0x20 for years 1900-2099)
    cmp al, 0x19
    je test7_ok
    cmp al, 0x20
    je test7_ok
    cmp al, 0x21            ; Allow 0x21 for future compatibility
    je test7_ok
    jmp test7_fail
    
test7_ok:
    mov al, 0x07            ; Test 7 passed marker
    out 0x998, al
    jmp all_tests_pass
    
test7_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x07
    out 0x998, al
    jmp test_end

    ; ==============================================================================
    ; All Tests Passed
    ; ==============================================================================
all_tests_pass:
    mov al, 0x00            ; Success
    out 0x999, al
    mov al, 0xFF            ; All tests passed marker
    out 0x998, al

test_end:
    ; Terminate program
    mov ah, 0x4C            ; DOS terminate
    int 0x21
    hlt                     ; Halt as fallback

my_code_seg ends

my_stack_seg segment stack
    db 100h dup(?)
my_stack_seg ends

end start
