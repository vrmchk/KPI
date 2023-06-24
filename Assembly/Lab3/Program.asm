STSEG SEGMENT PARA STACK 'STACK'
    DB 128 DUP ( 'STACK' )
STSEG ENDS
DSEG SEGMENT PARA PUBLIC 'DATA'
    EnterXMessage DB 'Enter x [-32767, 32767]: ', '$'
    EnterYMessage DB 'Enter y [-32767, 32767]: ', '$'
    EnterTMessage DB 'Enter t [-32767, 32767]: ', '$'
	ResultMessage DB 'Result: ', '$'
	RemainderMessage DB 'Remainder: ', '$'
	Formula1Message Db '(5x - y^2)/7(t - y)', '$'
	Formula2Message Db '13x + 5y + 7t', '$'
	Formula3Message Db '2xy', '$'
    NewLine DB 10, 13, '$'
    InputBuffer DB 7, ?, 7 DUP(' ')
    Number DW 0
	X DW 0
	Y DW 0
	T DW 0
	Remainder DW 0
    IsNegative DB 0
	OverflowOccured DB 0
    OverflowErrorMessage DB 'Overflow error! Enter a number again: ', '$'
    FormatErrorMessage DB 'Invalid number format! Enter a number again: ', '$'
	OverflowOperationErrorMessage DB 'Overflow Error! ', '$'
DSEG ENDS
CSEG SEGMENT PARA PUBLIC 'CODE'
ASSUME CS: CSEG, DS: DSEG, SS: STSEG
Main PROC FAR
    PUSH DS
    XOR AX, AX
    PUSH AX
    MOV AX, DSEG
    MOV DS, AX

Start:
	MOV OverflowOccured, 0
	LEA DX, EnterXMessage 
    CALL WriteMessage
    CALL ReadNumber
    CALL WriteNewLine
    MOV AX, Number
    MOV X, AX
	
	LEA DX, EnterYMessage 
    CALL WriteMessage
    CALL ReadNumber
    CALL WriteNewLine
    MOV AX, Number
    MOV Y, AX
	
	LEA DX, EnterTMessage 
	CALL WriteMessage
    CALL ReadNumber
    CALL WriteNewLine
    MOV AX, Number
    MOV T, AX
	
	MOV AX, Y
    CMP AX, X
    JGE Case2 			;X <= Y
    CMP AX, T
    JE Case3			;T == Y

Case1:
	CALL Formula1
	JMP ContinueMain

Case2:
    CALL Formula2
	JMP ContinueMain

Case3:
	CALL Formula3
	JMP ContinueMain

OverflowOperation:
	LEA DX, OverflowOperationErrorMessage
	CALL WriteMessage
	CALL WriteNewLine
	JMP Start

ContinueMain:
	CMP OverflowOccured, 1
	JE OverflowOperation
	LEA DX, ResultMessage
	CALL WriteMessage
	CALL WriteNumber
    CALL WriteNewLine
	CMP Remainder, 0
	JNE RemainderNotZero
	JMP MainEnd

RemainderNotZero:
	LEA DX, RemainderMessage
	CALL WriteMessage
	MOV AX, Remainder
	MOV Number, AX
	CALL WriteNumber
	JMP MainEnd

MainEnd:
	RET
Main ENDP

Formula1 PROC
	PUSH AX
	PUSH BX
	PUSH CX
	PUSH DX
	
	MOV AX, Y
	MUL Y
	JO OverflowFormula1
	MOV CX, AX 			;CX = Y^2
	
	MOV AX, X
	MOV BX, 5
	IMUL BX 			;AX = 5X
	JO OverflowFormula1
	SUB AX, CX			;AX = 5X - Y^2
	JO OverflowFormula1
	
	MOV BX, 7
	IDIV BX 			;AX = (5X - Y^2)/7
	MOV Remainder, DX
	JO OverflowFormula1
	
	MOV BX, T
	SUB BX, Y			;BX = T - Y
	JO OverflowFormula1
	IMUL BX				;AX = (5X - Y^2)/7 * (T - Y)
	
	MOV Number, AX
	LEA DX, Formula1Message
	CALL WriteMessage
	CALL WriteNewLine
	JMP Formula1End
	
OverflowFormula1:
	MOV OverflowOccured, 1
	
Formula1End:
	POP DX
	POP CX
	POP BX
	POP AX
	RET
Formula1 ENDP

Formula2 PROC
	PUSH AX
	PUSH BX
	PUSH CX
	PUSH DX
	
	MOV AX, X
    MOV BX, 13
    IMUL BX 			;AX = 13X
	JO OverflowFormula2
    MOV CX, AX			;CX = 13X

    MOV AX, Y
    MOV BX, 7
    IMUL BX				;AX = 7Y
	JO OverflowFormula2
    ADD CX, AX			;CX = 13X + 7Y
	JO OverflowFormula2

    MOV AX, T
    MOV BX, 5
    IMUL BX				;AX = 5T
	JO OverflowFormula2
    ADD CX, AX			;CX = 13X + 7Y + 5T
	JO OverflowFormula2

    MOV Number, CX
	LEA DX, Formula2Message
	CALL WriteMessage
	CALL WriteNewLine
	JMP Formula2End
	
OverflowFormula2:
	MOV OverflowOccured, 1
	
Formula2End:
	POP DX
	POP CX
	POP BX
	POP AX
	RET
Formula2 ENDP

Formula3 PROC
	PUSH AX
	PUSH BX
	PUSH CX
	PUSH DX
	
	MOV AX, X
    MOV BX, Y
    MOV CX, 2
	IMUL CX
	JO OverflowFormula3
    IMUL BX				;AX = 2*X*Y
	JO OverflowFormula3
	MOV Number, AX 
	LEA DX, Formula3Message
	CALL WriteMessage
	CALL WriteNewLine
	JMP Formula3End
	
OverflowFormula3:
	MOV OverflowOccured, 1
	
Formula3End:
	POP DX
	POP CX
	POP BX
	POP AX
	RET
Formula3 ENDP

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