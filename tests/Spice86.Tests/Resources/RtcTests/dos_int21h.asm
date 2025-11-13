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
; ==============================================================================

org 100h

section .text

start:
    ; ==============================================================================
    ; Test 1: INT 21H Function 2Ah - Get DOS Date
    ; ==============================================================================
    ; Returns:
    ;   CX = Year (1980-2099)
    ;   DH = Month (1-12)
    ;   DL = Day (1-31)
    ;   AL = Day of week (0=Sunday, 6=Saturday)
    
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
    mov al, 0x01            ; Test 1 passed
    out 0x998, al
    jmp test2

test1_fail:
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
    mov ah, 0x2B            ; Function 2Bh = Set date
    mov cx, 2025            ; Year 2025
    mov dh, 11              ; November
    mov dl, 13              ; 13th
    int 0x21
    
    cmp al, 0x00            ; Should return 0 for valid date
    jne test2_fail
    
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
    ; Test 3: INT 21H Function 2Bh - Set DOS Date (Invalid Year)
    ; ==============================================================================
    
test3:
    mov ah, 0x2B            ; Function 2Bh = Set date
    mov cx, 1979            ; Invalid year (< 1980)
    mov dh, 11
    mov dl, 13
    int 0x21
    
    cmp al, 0xFF            ; Should return FFh for invalid date
    jne test3_fail
    
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
    ; Test 4: INT 21H Function 2Bh - Set DOS Date (Invalid Month)
    ; ==============================================================================
    
test4:
    mov ah, 0x2B
    mov cx, 2025
    mov dh, 13              ; Invalid month (> 12)
    mov dl, 13
    int 0x21
    
    cmp al, 0xFF            ; Should return FFh
    jne test4_fail
    
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
    ; Test 5: INT 21H Function 2Bh - Set DOS Date (Invalid Day)
    ; ==============================================================================
    
test5:
    mov ah, 0x2B
    mov cx, 2025
    mov dh, 11
    mov dl, 32              ; Invalid day (> 31)
    int 0x21
    
    cmp al, 0xFF            ; Should return FFh
    jne test5_fail
    
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
    ; Test 6: INT 21H Function 2Ch - Get DOS Time
    ; ==============================================================================
    ; Returns:
    ;   CH = Hour (0-23)
    ;   CL = Minutes (0-59)
    ;   DH = Seconds (0-59)
    ;   DL = Hundredths (0-99)
    
test6:
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
    mov ah, 0x2D            ; Function 2Dh = Set time
    mov ch, 12              ; 12 hours
    mov cl, 30              ; 30 minutes
    mov dh, 45              ; 45 seconds
    mov dl, 50              ; 50 hundredths
    int 0x21
    
    cmp al, 0x00            ; Should return 0 for valid time
    jne test7_fail
    
test7_pass:
    mov al, 0x07            ; Test 7 passed
    out 0x998, al
    jmp test8

test7_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x07
    out 0x998, al
    jmp exit_program

    ; ==============================================================================
    ; Test 8: INT 21H Function 2Dh - Set DOS Time (Invalid Hour)
    ; ==============================================================================
    
test8:
    mov ah, 0x2D
    mov ch, 24              ; Invalid hour (>= 24)
    mov cl, 30
    mov dh, 45
    mov dl, 50
    int 0x21
    
    cmp al, 0xFF            ; Should return FFh
    jne test8_fail
    
test8_pass:
    mov al, 0x08            ; Test 8 passed
    out 0x998, al
    jmp test9

test8_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x08
    out 0x998, al
    jmp exit_program

    ; ==============================================================================
    ; Test 9: INT 21H Function 2Dh - Set DOS Time (Invalid Minutes)
    ; ==============================================================================
    
test9:
    mov ah, 0x2D
    mov ch, 12
    mov cl, 60              ; Invalid minutes (>= 60)
    mov dh, 45
    mov dl, 50
    int 0x21
    
    cmp al, 0xFF            ; Should return FFh
    jne test9_fail
    
test9_pass:
    mov al, 0x09            ; Test 9 passed
    out 0x998, al
    jmp test10

test9_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x09
    out 0x998, al
    jmp exit_program

    ; ==============================================================================
    ; Test 10: INT 21H Function 2Dh - Set DOS Time (Invalid Seconds)
    ; ==============================================================================
    
test10:
    mov ah, 0x2D
    mov ch, 12
    mov cl, 30
    mov dh, 60              ; Invalid seconds (>= 60)
    mov dl, 50
    int 0x21
    
    cmp al, 0xFF            ; Should return FFh
    jne test10_fail
    
test10_pass:
    mov al, 0x0A            ; Test 10 passed
    out 0x998, al
    jmp test11

test10_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x0A
    out 0x998, al
    jmp exit_program

    ; ==============================================================================
    ; Test 11: INT 21H Function 2Dh - Set DOS Time (Invalid Hundredths)
    ; ==============================================================================
    
test11:
    mov ah, 0x2D
    mov ch, 12
    mov cl, 30
    mov dh, 45
    mov dl, 100             ; Invalid hundredths (>= 100)
    int 0x21
    
    cmp al, 0xFF            ; Should return FFh
    jne test11_fail
    
test11_pass:
    mov al, 0x0B            ; Test 11 passed
    out 0x998, al
    jmp all_tests_passed

test11_fail:
    mov al, 0xFF
    out 0x999, al
    mov al, 0x0B
    out 0x998, al
    jmp exit_program

all_tests_passed:
    mov al, 0x00            ; All tests passed
    out 0x999, al
    mov al, 0x0B            ; 11 tests completed
    out 0x998, al

exit_program:
    mov ax, 0x4C00          ; DOS terminate
    int 0x21
