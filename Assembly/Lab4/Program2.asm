STSEG SEGMENT PARA STACK 'STACK'
    DB 128 DUP ( 'STACK' )
STSEG ENDS
DSEG SEGMENT PARA PUBLIC 'DATA'
    EnterRowsMessage DB 'Enter rows count (max count is 10): ', '$'
    EnterColumnsMessage DB 'Enter columns count (max count is 10): ', '$'
	ItemsRangeMessage DB 'Size of a number is [-32767, 32767]', '$'
	EnterItemStartMessage DB 'Enter item [', '$'
	EnterItemMiddleMessage DB '][', '$'
	EnterItemEndMessage DB ']: ', '$'
	EnterNumberToFindMessage DB 'Enter number to find: ', '$'
	FoundStartMessage DB 'Number found at [', '$'
	FoundMiddleMessage DB '][', '$'
	FoundEndMessage DB ']', '$'
	NotFoundMessage DB 'Number not found', '$'
    NewLine DB 10, 13, '$'
    InputBuffer DB 7, ?, 7 DUP(' ')
    Number DW 0
    IsNegative DB 0
	RowsCount DW 0
	ColumnsCount DW 0
	MaxArraySize DW 10
	Array DW 100 DUP (' ')
	Row DW 0
	Column DW 0
	NumberFound DB 0
    OverflowErrorMessage DB 'Overflow error! Enter a number again: ', '$'
    FormatErrorMessage DB 'Invalid number format! Enter a number again: ', '$'
	OverflowOperationErrorMessage DB 'Overflow Error! ', '$'
	SizeLessThan0Message DB 'Size should be greater than 0! Enter size again: ', '$'
	SizeGreaterThanMaxMessage DB 'Size should be less or equal than 10! Enter size again: ', '$'
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
	LEA DX, EnterRowsMessage
	CALL WriteMessage
	CALL ReadDimension
	MOV AX, Number
	MOV RowsCount, AX
	
	LEA DX, EnterColumnsMessage
	CALL WriteMessage
	CALL ReadDimension
	MOV AX, Number
	MOV ColumnsCount, AX
	
	CALL ReadMatrix
	LEA DX, EnterNumberToFindMessage
	CALL WriteMessage
	CALL ReadNumber
	CALL WriteNewLine
	
	CALL FindCoordinates
	RET

OverflowOperation:
	LEA DX, OverflowOperationErrorMessage
	MOV AH, 9
	INT 21H
	CALL WriteNewLine
	JMP Start

Main ENDP

ReadDimension PROC
	PUSH AX
	PUSH DX

ReadSizeStart:
	CALL ReadNumber
	CALL WriteNewLine
	
	MOV AX, Number
	CMP AX, 0
	JNG SizeLessThan0
	CMP AX, MaxArraySize
	JG SizeGreaterThanMax
	JMP ReadSizeEnd
	
SizeLessThan0:
	LEA DX, SizeLessThan0Message
	CALL WriteMessage
	JMP ReadSizeStart

SizeGreaterThanMax:
	LEA DX, SizeGreaterThanMaxMessage
	CALL WriteMessage
	JMP ReadSizeStart

ReadSizeEnd:
	POP DX
	POP AX
	RET
ReadDimension ENDP

ReadMatrix PROC
	PUSH AX
	PUSH BX
	PUSH CX
	PUSH DX
	PUSH SI
	PUSH DI

	LEA DX, ItemsRangeMessage
	CALL WriteMessage
	CALL WriteNewLine
	
	XOR SI, SI
	MOV CX, RowsCount
	MOV BX, 1
	
	OuterReadLoop:
		MOV DI, 1
		InnerReadLoop:
			LEA DX, EnterItemStartMessage
			CALL WriteMessage
			MOV Number, BX
			CALL WriteNumber
			LEA DX, EnterItemMiddleMessage
			CALL WriteMessage
			MOV Number, DI
			CALL WriteNumber
			LEA DX, EnterItemEndMessage
			CALL WriteMessage
		
			MOV Number, 0
			CALL ReadNumber
			CALL WriteNewLine
		
			MOV AX, Number
			MOV Array[SI], AX
			ADD SI, TYPE Array
			INC DI
			CMP DI, ColumnsCount
			JLE InnerReadLoop
			
		InnerReadLoopEnd:
			INC BX
			LOOP OuterReadLoop
	
	POP DI
	POP SI
	POP DX
	POP CX
	POP BX
	POP AX
	RET
ReadMatrix ENDP

FindCoordinates PROC
	PUSH AX
	PUSH BX
	PUSH CX
	PUSH SI
	PUSH DI
	
	MOV CX, RowsCount
	LEA SI, Array
	MOV BX, 1
	OuterFindLoop:
		MOV DI, 1
		InnerFindLoop:
			MOV AX, [SI]
			CMP Number, AX
			JNE MoveNext
		
			MOV NumberFound, 1
			MOV Row, BX
			MOV Column, DI
			CALL WriteCoordinates
		
		MoveNext:
			ADD SI, TYPE Array
			INC DI
			CMP DI, ColumnsCount
			JLE InnerFindLoop
		
		InnerFindLoopEnd:
			INC BX
			LOOP OuterFindLoop
	
	CMP NumberFound, 1
	JE FindEnd
	LEA DX, NotFoundMessage
	CALL WriteMessage
	CALL WriteNewLine
	
FindEnd:
	POP DI
	POP SI
	POP CX
	POP BX
	POP AX
	RET
FindCoordinates ENDP

WriteCoordinates PROC
	PUSH AX
	PUSH DX
	PUSH Number
	
	LEA DX, FoundStartMessage
	CALL WriteMessage
	MOV AX, Row
	MOV Number, AX
	CALL WriteNumber
	LEA DX, FoundMiddleMessage
	CALL WriteMessage
	MOV AX, Column
	MOV Number, AX
	CALL WriteNumber
	LEA DX, FoundEndMessage
	CALL WriteMessage
	CALL WriteNewLine
	
	POP Number
	POP DX
	POP AX
	RET
WriteCoordinates ENDP

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