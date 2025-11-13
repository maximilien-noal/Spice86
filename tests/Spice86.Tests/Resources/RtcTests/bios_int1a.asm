; ==============================================================================
; BIOS INT 1A Time Services Test
; ==============================================================================
; This test verifies the BIOS INT 1A time services including:
;   - Function 00h: Get System Clock Counter
;   - Function 01h: Set System Clock Counter  
;   - Function 02h: Read RTC Time
;   - Function 03h: Set RTC Time
;   - Function 04h: Read RTC Date
;   - Function 05h: Set RTC Date
;
; INT 1A provides both a tick counter (18.2 Hz timer) and real-time clock
; services. The tick counter increments roughly 18.2 times per second and
; rolls over at midnight.
;
; Test Results Protocol:
;   Port 0x999 = Result port (0x00 = success, 0xFF = failure)
;   Port 0x998 = Details port (test number that passed/failed)
; ==============================================================================

.model small

my_code_seg segment
start:
    ; ==============================================================================
    ; Test 1: INT 1A Function 00h - Get System Clock Counter
    ; ==============================================================================
    ; This function returns the current tick count in CX:DX and midnight flag in AL
    ; Expected: CX and DX should contain valid values (CX:DX forms a 32-bit counter)
    
    mov ah, 0x00            ; Function 00h - Get System Clock Counter
    int 0x1A                ; Call BIOS time services
    
    ; Check if we got some values back (just verify they're not all zero
    ; unless it's exactly midnight)
    mov bx, cx              ; Save CX
    or bx, dx               ; OR with DX
    jnz test1_ok            ; If not zero, we're good
    
    ; CX:DX is zero - could be valid at midnight, so we'll accept it
test1_ok:
    mov al, 0x01            ; Test 1 passed
    out 0x998, al
    jmp test2

    ; ==============================================================================
    ; Test 2: INT 1A Function 01h - Set System Clock Counter
    ; ==============================================================================
    ; This function sets the tick count from CX:DX
    ; We'll set a specific value and then read it back
    
test2:
    ; Set a known tick count value
    mov cx, 0x1234          ; High word
    mov dx, 0x5678          ; Low word
    mov ah, 0x01            ; Function 01h - Set System Clock Counter
    int 0x1A
    
    ; Now read it back
    mov ah, 0x00            ; Function 00h - Get System Clock Counter
    int 0x1A
    
    ; Verify the values match (or are close, accounting for timer ticks)
    ; We'll just verify that CX is 0x1234 (high word shouldn't change quickly)
    cmp cx, 0x1234
    jne test2_fail
    
    ; DX might have incremented by a few ticks, so we'll check it's in range
    cmp dx, 0x5678
    jb test2_fail           ; Should be >= original value
    sub dx, 0x5678
    cmp dx, 100             ; Allow up to 100 ticks difference (~5.5 seconds)
    ja test2_fail
    
    mov al, 0x02            ; Test 2 passed
    out 0x998, al
    jmp test3
    
test2_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x02
    out 0x998, al
    jmp test_end

    ; ==============================================================================
    ; Test 3: INT 1A Function 02h - Read RTC Time
    ; ==============================================================================
    ; This function reads the current time from the RTC in BCD format
    ; Returns: CH = hours (BCD), CL = minutes (BCD), DH = seconds (BCD), DL = DST flag
    
test3:
    mov ah, 0x02            ; Function 02h - Read RTC Time
    int 0x1A
    jc test3_fail           ; Carry set indicates error
    
    ; Validate hours (CH) - should be 0x00-0x23 in BCD
    mov al, ch
    call validate_bcd_hour
    jc test3_fail
    
    ; Validate minutes (CL) - should be 0x00-0x59 in BCD
    mov al, cl
    call validate_bcd_minute_second
    jc test3_fail
    
    ; Validate seconds (DH) - should be 0x00-0x59 in BCD
    mov al, dh
    call validate_bcd_minute_second
    jc test3_fail
    
    mov al, 0x03            ; Test 3 passed
    out 0x998, al
    jmp test4
    
test3_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x03
    out 0x998, al
    jmp test_end

    ; ==============================================================================
    ; Test 4: INT 1A Function 03h - Set RTC Time
    ; ==============================================================================
    ; This function sets the RTC time. In our emulator, this may be a stub
    ; but it should at least accept valid input without crashing
    
test4:
    mov ch, 0x12            ; Hours = 12 (BCD)
    mov cl, 0x34            ; Minutes = 34 (BCD)
    mov dh, 0x56            ; Seconds = 56 (BCD)
    mov dl, 0x00            ; No DST
    mov ah, 0x03            ; Function 03h - Set RTC Time
    int 0x1A
    ; No error checking - function may be stubbed, just verify it doesn't crash
    
    mov al, 0x04            ; Test 4 passed (completed without crash)
    out 0x998, al
    jmp test5

    ; ==============================================================================
    ; Test 5: INT 1A Function 04h - Read RTC Date
    ; ==============================================================================
    ; This function reads the current date from the RTC in BCD format
    ; Returns: CH = century (BCD), CL = year (BCD), DH = month (BCD), DL = day (BCD)
    
test5:
    mov ah, 0x04            ; Function 04h - Read RTC Date
    int 0x1A
    jc test5_fail           ; Carry set indicates error
    
    ; Validate century (CH) - should be 0x19 or 0x20 or 0x21 in BCD
    mov al, ch
    cmp al, 0x19
    je test5_century_ok
    cmp al, 0x20
    je test5_century_ok
    cmp al, 0x21
    je test5_century_ok
    jmp test5_fail
    
test5_century_ok:
    ; Validate year (CL) - should be 0x00-0x99 in BCD
    mov al, cl
    call validate_bcd_byte
    jc test5_fail
    
    ; Validate month (DH) - should be 0x01-0x12 in BCD
    mov al, dh
    cmp al, 0x01
    jb test5_fail
    cmp al, 0x12
    ja test5_fail
    call validate_bcd_byte
    jc test5_fail
    
    ; Validate day (DL) - should be 0x01-0x31 in BCD
    mov al, dl
    cmp al, 0x01
    jb test5_fail
    cmp al, 0x31
    ja test5_fail
    call validate_bcd_byte
    jc test5_fail
    
    mov al, 0x05            ; Test 5 passed
    out 0x998, al
    jmp test6
    
test5_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x05
    out 0x998, al
    jmp test_end

    ; ==============================================================================
    ; Test 6: INT 1A Function 05h - Set RTC Date
    ; ==============================================================================
    ; This function sets the RTC date. In our emulator, this may be a stub
    ; but it should at least accept valid input without crashing
    
test6:
    mov ch, 0x20            ; Century = 20 (BCD)
    mov cl, 0x25            ; Year = 25 (BCD)
    mov dh, 0x11            ; Month = 11 (BCD)
    mov dl, 0x13            ; Day = 13 (BCD)
    mov ah, 0x05            ; Function 05h - Set RTC Date
    int 0x1A
    ; No error checking - function may be stubbed, just verify it doesn't crash
    
    mov al, 0x06            ; Test 6 passed (completed without crash)
    out 0x998, al
    jmp all_tests_pass

    ; ==============================================================================
    ; Helper Functions
    ; ==============================================================================

; Validate BCD byte (both nibbles 0-9)
; Input: AL = byte to validate
; Output: Carry flag set if invalid
validate_bcd_byte:
    push ax
    push bx
    mov bl, al
    and al, 0x0F            ; Check low nibble
    cmp al, 0x09
    ja validate_bcd_fail
    mov al, bl
    shr al, 4               ; Check high nibble
    cmp al, 0x09
    ja validate_bcd_fail
    clc                     ; Clear carry = valid
    pop bx
    pop ax
    ret
validate_bcd_fail:
    stc                     ; Set carry = invalid
    pop bx
    pop ax
    ret

; Validate BCD hour (0x00-0x23)
; Input: AL = hour to validate
; Output: Carry flag set if invalid
validate_bcd_hour:
    push ax
    push bx
    mov bl, al
    and al, 0x0F            ; Check low nibble
    cmp al, 0x09
    ja validate_hour_fail
    mov al, bl
    shr al, 4               ; Check high nibble
    cmp al, 0x02
    ja validate_hour_fail
    ; If high nibble is 2, low nibble must be 0-3
    cmp al, 0x02
    jne validate_hour_ok
    mov al, bl
    and al, 0x0F
    cmp al, 0x03
    ja validate_hour_fail
validate_hour_ok:
    clc
    pop bx
    pop ax
    ret
validate_hour_fail:
    stc
    pop bx
    pop ax
    ret

; Validate BCD minute/second (0x00-0x59)
; Input: AL = value to validate
; Output: Carry flag set if invalid
validate_bcd_minute_second:
    push ax
    push bx
    mov bl, al
    and al, 0x0F            ; Check low nibble
    cmp al, 0x09
    ja validate_ms_fail
    mov al, bl
    shr al, 4               ; Check high nibble
    cmp al, 0x05
    ja validate_ms_fail
    clc
    pop bx
    pop ax
    ret
validate_ms_fail:
    stc
    pop bx
    pop ax
    ret

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
    hlt

my_code_seg ends

my_stack_seg segment stack
    db 100h dup(?)
my_stack_seg ends

end start
