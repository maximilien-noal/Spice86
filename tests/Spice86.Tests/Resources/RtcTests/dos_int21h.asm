; ==============================================================================
; DOS INT 21H Date/Time Services Test
; ==============================================================================
; This test verifies the DOS INT 21H date and time functions:
;   - Function 2Ah: Get DOS Date
;   - Function 2Bh: Set DOS Date
;   - Function 2Ch: Get DOS Time
;   - Function 2Dh: Set DOS Time
;
; DOS date/time functions access the CMOS RTC through I/O ports and provide
; DOS-level services for date and time manipulation. The functions use binary
; (not BCD) format in registers.
;
; Test Results Protocol:
;   Port 0x999 = Result port (0x00 = success, 0xFF = failure)
;   Port 0x998 = Details port (test number that passed/failed)
; ==============================================================================

.model small

my_code_seg segment
start:
    ; ==============================================================================
    ; Test 1: INT 21H Function 2Ah - Get DOS Date
    ; ==============================================================================
    ; This function returns the current date in binary format
    ; Returns: CX = year (1980-2099), DH = month (1-12), DL = day (1-31), AL = day of week (0-6)
    
    mov ah, 0x2A            ; Function 2Ah - Get DOS Date
    int 0x21
    
    ; Validate year (CX) - should be 1980-2099
    cmp cx, 1980
    jb test1_fail
    cmp cx, 2099
    ja test1_fail
    
    ; Validate month (DH) - should be 1-12
    cmp dh, 0x01
    jb test1_fail
    cmp dh, 0x0C
    ja test1_fail
    
    ; Validate day (DL) - should be 1-31
    cmp dl, 0x01
    jb test1_fail
    cmp dl, 0x1F
    ja test1_fail
    
    ; Validate day of week (AL) - should be 0-6
    cmp al, 0x06
    ja test1_fail
    
    mov al, 0x01            ; Test 1 passed
    out 0x998, al
    jmp test2
    
test1_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x01
    out 0x998, al
    jmp test_end

    ; ==============================================================================
    ; Test 2: INT 21H Function 2Bh - Set DOS Date (Valid Date)
    ; ==============================================================================
    ; This function sets the DOS date. It should return AL=0 for success, AL=0xFF for failure
    ; We'll set a valid date: November 13, 2025
    
test2:
    mov cx, 2025            ; Year = 2025
    mov dh, 0x0B            ; Month = 11 (November)
    mov dl, 0x0D            ; Day = 13
    mov ah, 0x2B            ; Function 2Bh - Set DOS Date
    int 0x21
    
    ; Check if successful (AL = 0)
    cmp al, 0x00
    jne test2_fail
    
    ; Verify by reading the date back
    mov ah, 0x2A            ; Function 2Ah - Get DOS Date
    int 0x21
    
    ; Check year
    cmp cx, 2025
    jne test2_fail
    
    ; Check month
    cmp dh, 0x0B
    jne test2_fail
    
    ; Check day
    cmp dl, 0x0D
    jne test2_fail
    
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
    ; Test 3: INT 21H Function 2Bh - Set DOS Date (Invalid Year)
    ; ==============================================================================
    ; This tests that setting an invalid year returns AL=0xFF
    
test3:
    mov cx, 1979            ; Year = 1979 (too early, DOS supports 1980+)
    mov dh, 0x06            ; Month = 6
    mov dl, 0x01            ; Day = 1
    mov ah, 0x2B            ; Function 2Bh - Set DOS Date
    int 0x21
    
    ; Should return AL=0xFF for invalid date
    cmp al, 0xFF
    jne test3_fail
    
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
    ; Test 4: INT 21H Function 2Bh - Set DOS Date (Invalid Month)
    ; ==============================================================================
    ; This tests that setting an invalid month returns AL=0xFF
    
test4:
    mov cx, 2025            ; Year = 2025
    mov dh, 0x0D            ; Month = 13 (invalid)
    mov dl, 0x01            ; Day = 1
    mov ah, 0x2B            ; Function 2Bh - Set DOS Date
    int 0x21
    
    ; Should return AL=0xFF for invalid date
    cmp al, 0xFF
    jne test4_fail
    
    mov al, 0x04            ; Test 4 passed
    out 0x998, al
    jmp test5
    
test4_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x04
    out 0x998, al
    jmp test_end

    ; ==============================================================================
    ; Test 5: INT 21H Function 2Bh - Set DOS Date (Invalid Day)
    ; ==============================================================================
    ; This tests that setting an invalid day returns AL=0xFF
    
test5:
    mov cx, 2025            ; Year = 2025
    mov dh, 0x06            ; Month = 6
    mov dl, 0x20            ; Day = 32 (invalid)
    mov ah, 0x2B            ; Function 2Bh - Set DOS Date
    int 0x21
    
    ; Should return AL=0xFF for invalid date
    cmp al, 0xFF
    jne test5_fail
    
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
    ; Test 6: INT 21H Function 2Ch - Get DOS Time
    ; ==============================================================================
    ; This function returns the current time in binary format
    ; Returns: CH = hour (0-23), CL = minutes (0-59), DH = seconds (0-59), DL = hundredths (0-99)
    
test6:
    mov ah, 0x2C            ; Function 2Ch - Get DOS Time
    int 0x21
    
    ; Validate hour (CH) - should be 0-23
    cmp ch, 0x17
    ja test6_fail
    
    ; Validate minutes (CL) - should be 0-59
    cmp cl, 0x3B
    ja test6_fail
    
    ; Validate seconds (DH) - should be 0-59
    cmp dh, 0x3B
    ja test6_fail
    
    ; Validate hundredths (DL) - should be 0-99
    cmp dl, 0x63
    ja test6_fail
    
    mov al, 0x06            ; Test 6 passed
    out 0x998, al
    jmp test7
    
test6_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x06
    out 0x998, al
    jmp test_end

    ; ==============================================================================
    ; Test 7: INT 21H Function 2Dh - Set DOS Time (Valid Time)
    ; ==============================================================================
    ; This function sets the DOS time. It should return AL=0 for success, AL=0xFF for failure
    ; We'll set a valid time: 14:30:45.99
    
test7:
    mov ch, 0x0E            ; Hour = 14
    mov cl, 0x1E            ; Minutes = 30
    mov dh, 0x2D            ; Seconds = 45
    mov dl, 0x63            ; Hundredths = 99
    mov ah, 0x2D            ; Function 2Dh - Set DOS Time
    int 0x21
    
    ; Check if successful (AL = 0)
    cmp al, 0x00
    jne test7_fail
    
    ; Verify by reading the time back
    mov ah, 0x2C            ; Function 2Ch - Get DOS Time
    int 0x21
    
    ; Check hour
    cmp ch, 0x0E
    jne test7_fail
    
    ; Check minutes
    cmp cl, 0x1E
    jne test7_fail
    
    ; Check seconds (might have incremented by 1, so allow 45 or 46)
    cmp dh, 0x2D
    je test7_seconds_ok
    cmp dh, 0x2E
    je test7_seconds_ok
    jmp test7_fail
    
test7_seconds_ok:
    ; Note: hundredths may not match exactly due to timing, so we'll skip that check
    
    mov al, 0x07            ; Test 7 passed
    out 0x998, al
    jmp test8
    
test7_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x07
    out 0x998, al
    jmp test_end

    ; ==============================================================================
    ; Test 8: INT 21H Function 2Dh - Set DOS Time (Invalid Hour)
    ; ==============================================================================
    ; This tests that setting an invalid hour returns AL=0xFF
    
test8:
    mov ch, 0x18            ; Hour = 24 (invalid)
    mov cl, 0x00            ; Minutes = 0
    mov dh, 0x00            ; Seconds = 0
    mov dl, 0x00            ; Hundredths = 0
    mov ah, 0x2D            ; Function 2Dh - Set DOS Time
    int 0x21
    
    ; Should return AL=0xFF for invalid time
    cmp al, 0xFF
    jne test8_fail
    
    mov al, 0x08            ; Test 8 passed
    out 0x998, al
    jmp test9
    
test8_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x08
    out 0x998, al
    jmp test_end

    ; ==============================================================================
    ; Test 9: INT 21H Function 2Dh - Set DOS Time (Invalid Minutes)
    ; ==============================================================================
    ; This tests that setting invalid minutes returns AL=0xFF
    
test9:
    mov ch, 0x0C            ; Hour = 12
    mov cl, 0x3C            ; Minutes = 60 (invalid)
    mov dh, 0x00            ; Seconds = 0
    mov dl, 0x00            ; Hundredths = 0
    mov ah, 0x2D            ; Function 2Dh - Set DOS Time
    int 0x21
    
    ; Should return AL=0xFF for invalid time
    cmp al, 0xFF
    jne test9_fail
    
    mov al, 0x09            ; Test 9 passed
    out 0x998, al
    jmp test10
    
test9_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x09
    out 0x998, al
    jmp test_end

    ; ==============================================================================
    ; Test 10: INT 21H Function 2Dh - Set DOS Time (Invalid Seconds)
    ; ==============================================================================
    ; This tests that setting invalid seconds returns AL=0xFF
    
test10:
    mov ch, 0x0C            ; Hour = 12
    mov cl, 0x1E            ; Minutes = 30
    mov dh, 0x3C            ; Seconds = 60 (invalid)
    mov dl, 0x00            ; Hundredths = 0
    mov ah, 0x2D            ; Function 2Dh - Set DOS Time
    int 0x21
    
    ; Should return AL=0xFF for invalid time
    cmp al, 0xFF
    jne test10_fail
    
    mov al, 0x0A            ; Test 10 passed
    out 0x998, al
    jmp test11
    
test10_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x0A
    out 0x998, al
    jmp test_end

    ; ==============================================================================
    ; Test 11: INT 21H Function 2Dh - Set DOS Time (Invalid Hundredths)
    ; ==============================================================================
    ; This tests that setting invalid hundredths returns AL=0xFF
    
test11:
    mov ch, 0x0C            ; Hour = 12
    mov cl, 0x1E            ; Minutes = 30
    mov dh, 0x2D            ; Seconds = 45
    mov dl, 0x64            ; Hundredths = 100 (invalid)
    mov ah, 0x2D            ; Function 2Dh - Set DOS Time
    int 0x21
    
    ; Should return AL=0xFF for invalid time
    cmp al, 0xFF
    jne test11_fail
    
    mov al, 0x0B            ; Test 11 passed
    out 0x998, al
    jmp all_tests_pass
    
test11_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x0B
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
    hlt

my_code_seg ends

my_stack_seg segment stack
    db 100h dup(?)
my_stack_seg ends

end start
