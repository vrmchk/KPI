STSEG SEGMENT PARA STACK 'STACK'
    DB 128 DUP ( 'STACK' )
STSEG ENDS
DSEG SEGMENT PARA PUBLIC 'DATA'
    EnterNumberMessage DB 'Enter a number [-32767, 32762]: ', '$'
    NewLine DB 10, 13, '$'
    InputBuffer DB 7, ?, 7 DUP(' ')
    Number DW 0
    IsNegative DB 0
    NumberToAdd DW 5
    OverflowErrorMessage DB 'Overflow error! Enter a number again: ', '$'
    FormatErrorMessage DB 'Invalid number format! Enter a number again: ', '$'
DSEG ENDS
CSEG SEGMENT PARA PUBLIC 'CODE'
ASSUME CS: CSEG, DS: DSEG, SS: STSEG
Main PROC FAR
    PUSH DS
    XOR AX, AX
    PUSH AX
    MOV AX, DSEG
    MOV DS, AX
    
	LEA DX, EnterNumberMessage
    CALL WriteMessage
	
Start:
    CALL ReadNumber
    CALL WriteNewLine

    MOV AX, Number
    ADD AX, NumberToAdd
    JO OverflowErrorMain
    
    MOV Number, AX
   
    CALL WriteNumber
    CALL WriteNewLine
    
	RET

OverflowErrorMain:
    LEA DX, OverflowErrorMessage
	CALL WriteMessage
    JMP Start

Main ENDP

ReadNumber PROC
    PUSH AX
	PUSH BX
	PUSH CX
	PUSH DX
    PUSH SI

ReadStart:
    LEA DX, InputBuffer
    MOV AH, 10
    INT 21H
	
	XOR AX, AX
	XOR BX, BX
	XOR CX, CX
	MOV IsNegative, 0
    
    MOV CL, InputBuffer[1]
    MOV SI, 2
    
    MOV BL, InputBuffer[SI]
    CMP BL, '-'
    JNE MainLoop
    MOV IsNegative, 1
    INC SI
    DEC CL

	MainLoop:
		MOV BL, InputBuffer[SI]
		CMP BL, '0'
		JB FormatError
		CMP BL, '9'
		JA FormatError
		
		MOV DX, 10
		IMUL DX
		JO OverflowError
		
		SUB BX, '0'
		ADD AX, BX
		JO OverflowError
		INC SI
		LOOP MainLoop
		
	CMP IsNegative, 1
	JNE ReadEnd
	NEG AX
    
ReadEnd:
	MOV Number, AX
    POP SI
	POP DX
    POP CX
	POP BX
	POP AX
    RET

OverflowError:
	CALL WriteNewLine
	LEA DX, OverflowErrorMessage
	CALL WriteMessage
    JMP ReadStart

FormatError:
	CALL WriteNewLine
	LEA DX, FormatErrorMessage
	CALL WriteMessage
    JMP ReadStart
ReadNumber ENDP

WriteMessage PROC
	MOV AH, 9
	INT 21H
	RET
WriteMessage ENDP

WriteNewLine PROC
    LEA DX, NewLine
    CALL WriteMessage
    RET
WriteNewLine ENDP

WriteNumber PROC
    PUSH AX
	PUSH BX
	PUSH CX
	PUSH DX
	MOV  BX, Number
    OR BX, BX
    JNS  InitRegisters
    MOV  AL, '-'
    INT  29H
    NEG  BX
InitRegisters:
    MOV  AX, BX
    XOR  CX, CX
    MOV  BX, 10
PushDigitToStack:
    XOR  DX, DX
    DIV  BX
    ADD  DL, '0'
    PUSH DX
    INC  CX
    TEST AX, AX
    JNZ PushDigitToStack
	PopAndWriteDigit:
		POP AX
		INT 29H
		LOOP PopAndWriteDigit
	POP DX
	POP CX
	POP BX
	POP AX
    RET
WriteNumber ENDP
CSEG ENDS
END Main