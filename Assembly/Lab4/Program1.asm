STSEG SEGMENT PARA STACK 'STACK'
    DB 128 DUP ( 'STACK' )
STSEG ENDS
DSEG SEGMENT PARA PUBLIC 'DATA'
    EnterSizeMessage DB 'Enter an array size (max size is 100): ', '$'
	ItemsRangeMessage DB 'Size of a number is [-32767, 32767]', '$'
	EnterItemStartMessage DB 'Enter item [', '$'
	EnterItemEndMessage DB ']: ', '$'
	SumMessage DB 'Sum: ', '$'
	MinMessage DB 'Min: ', '$'
	MaxMessage DB 'Max: ', '$'
	SortedMessage DB 'Sorted array: ', '$'
    NewLine DB 10, 13, '$'
    InputBuffer DB 7, ?, 7 DUP(' ')
    Number DW 0
    IsNegative DB 0
	ArraySize DW 0
	MaxArraySize DW 100
	Array DW 100 DUP (' ')
	SumOverflowOccured DB 0
    OverflowErrorMessage DB 'Overflow error! Enter a number again: ', '$'
    FormatErrorMessage DB 'Invalid number format! Enter a number again: ', '$'
	OverflowOperationErrorMessage DB 'Overflow Error! ', '$'
	SizeLessThan0Message DB 'Size should be greater than 0!', '$'
	SizeGreaterThanMaxMessage DB 'Size should be less or equal than 100!', '$'
DSEG ENDS
CSEG SEGMENT PARA PUBLIC 'CODE'
ASSUME CS: CSEG, DS: DSEG, SS: STSEG
Main PROC FAR
    PUSH DS
    XOR AX, AX
    PUSH AX
	PUSH DS
    XOR AX, AX
    PUSH AX
    MOV AX, DSEG
    MOV DS, AX

Start:
	CALL ReadArraySize
	CALL ReadArray
	CALL WriteArray
	CALL WriteNewLine
	
	LEA DX, SumMessage
	CALL WriteMessage
	CALL GetSum
	CMP SumOverflowOccured, 1
	JE ContinueMain
	CALL WriteNumber
	CALL WriteNewLine

ContinueMain:
	LEA DX, MinMessage
	CALL WriteMessage
	CALL GetMin
	CALL WriteNumber
	CALL WriteNewLine
	
	LEA DX, MaxMessage
	CALL WriteMessage
	CALL GetMax
	CALL WriteNumber
	CALL WriteNewLine
	
	LEA DX, SortedMessage
	CALL WriteMessage
	CALL WriteNewLine
	CALL Sort
	CALL WriteArray
	CALL WriteNewLine
	
	RET
Main ENDP

ReadArraySize PROC
	PUSH AX
	PUSH DX

ReadSizeStart:
	LEA DX, EnterSizeMessage
	CALL WriteMessage
	CALL ReadNumber
	CALL WriteNewLine
	
	MOV AX, Number
	CMP AX, 0
	JNG SizeLessThan0
	CMP AX, MaxArraySize
	JG SizeGreaterThanMax
	MOV ArraySize, AX
	JMP ReadSizeEnd
	
SizeLessThan0:
	LEA DX, SizeLessThan0Message
	CALL WriteMessage
	CALL WriteNewLine
	JMP ReadSizeStart

SizeGreaterThanMax:
	LEA DX, SizeGreaterThanMaxMessage
	CALL WriteMessage
	CALL WriteNewLine
	JMP ReadSizeStart

ReadSizeEnd:
	POP DX
	POP AX
	RET
ReadArraySize ENDP

ReadArray PROC
	PUSH AX
	PUSH BX
	PUSH CX
	PUSH DX
	PUSH SI
	
	LEA DX, ItemsRangeMessage
	CALL WriteMessage
	CALL WriteNewLine
	XOR SI, SI
	MOV CX, ArraySize
	MOV BX, 1
	
	ReadLoop:
		LEA DX, EnterItemStartMessage
		CALL WriteMessage
		MOV Number, BX
		CALL WriteNumber
		LEA DX, EnterItemEndMessage
		CALL WriteMessage
		
		MOV Number, 0
		CALL ReadNumber
		CALL WriteNewLine
		
		MOV AX, Number
		MOV Array[SI], AX
		ADD SI, TYPE Array
		INC BX
		LOOP ReadLoop

	POP SI
	POP DX
	POP CX
	POP BX
	POP AX
	RET
ReadArray ENDP

WriteArray PROC
	PUSH CX
	PUSH SI
	PUSH AX
	
	MOV CX, ArraySize
	LEA SI, Array
	WriteLoop:
		MOV AX, [SI]
		MOV Number, AX
		CALL WriteNumber
		
		MOV AL, ' '
		INT 29H
		ADD SI, TYPE Array
		LOOP WriteLoop
		
	POP AX
	POP SI
	POP CX
	RET
WriteArray ENDP

GetSum PROC
	PUSH AX
	PUSH CX
	PUSH SI
	
	XOR AX, AX
	MOV CX, ArraySize
	LEA SI, Array
	SumLoop:
		ADD AX, [SI]
		JO OverflowSum
		ADD SI, TYPE Array
		LOOP SumLoop
	MOV Number, AX
	JMP GetSumEnd
	
OverflowSum:
	LEA DX, OverflowOperationErrorMessage
	CALL WriteMessage
	CALL WriteNewLine
	MOV SumOverflowOccured, 1

GetSumEnd:
	POP SI
	POP CX
	POP AX
	RET
GetSum ENDP

GetMin PROC
	PUSH AX
	PUSH CX
	PUSH SI
	
	XOR AX, AX
	MOV CX, ArraySize
	LEA SI, Array
	MOV AX, [SI]
	CheckMinLoop:
		CMP AX, [SI]
		JLE ContinueMin
		MOV AX, [SI]
		
	ContinueMin:
		ADD SI, TYPE Array
		LOOP CheckMinLoop
		
	MOV Number, AX
	
	POP SI
	POP CX
	POP AX
	RET
GetMin ENDP

GetMax PROC
	PUSH AX
	PUSH CX
	PUSH SI
	
	XOR AX, AX
	MOV CX, ArraySize
	LEA SI, Array
	MOV AX, [SI]
	CheckMaxLoop:
		CMP AX, [SI]
		JGE ContinueMax
		MOV AX, [SI]
		
	ContinueMax:
		ADD SI, TYPE Array
		LOOP CheckMaxLoop
		
	MOV Number, AX
	
	POP SI
	POP CX
	POP AX
	RET
GetMax ENDP

Sort PROC
	PUSH AX
	PUSH BX
	PUSH CX
	PUSH DX
	XOR AX, AX
	XOR DX, DX
	
	CMP ArraySize, 1
	JE SortEnd
	
	MOV Number, 0
	MOV CX, ArraySize
	DEC CX
	OuterLoop:
		XOR BX, BX		
		
		InnerLoop:
			MOV AX, TYPE Array
			MUL BX
			MOV SI, AX
			
			MOV AX, Array[SI]
			MOV DX, Array[SI + TYPE Array]
			CMP AX, DX
			JLE ContinueLoop
			MOV Array[SI], DX
			MOV Array[SI + TYPE Array], AX
		
		ContinueLoop:
			INC BX
			CMP BX, CX
			JL InnerLoop
			LOOP OuterLoop

SortEnd:	
	POP DX
	POP CX
	POP BX
	POP AX
	RET
Sort ENDP

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