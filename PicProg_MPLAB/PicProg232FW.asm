	    LIST	p=16F84A
	    __CONFIG	03FF1H
	    
STATUS	    equ		03h
PORTB	    equ		06h	
TRISB	    equ		06h
TRISA	    equ		05h
PORTA	    equ		05h
PCL	    equ		02h
EEDATA	    equ		08h
EEADR	    equ		09h
EECON1	    equ		08h
EECON2	    equ		09h
INTCON	    equ		0Bh
Reg_1	    equ		10h
Reg_2	    equ		11h
Reg_3	    equ		12h
ReadByte232 equ		13h
WriteByte232 equ	14h
Counter232  equ		15h
dataflags232 equ	16h;	0 - RS232 Recieved
 
ReadedByteL equ		17h
ReadedByteH equ		18h
SendByteL   equ		19h
SendByteH   equ		1Ch


Counter	    equ		1Ah
CommandBuff equ		1Bh

CommandMemory equ	1Eh
DataLMemory equ		1Dh
DataHMemory equ		1Fh
 
 
 
	    org 0
	    
	    bsf		STATUS,5
	    clrf	TRISA
	    bsf		TRISB,1
	    bcf		TRISB,0
	    bcf		TRISB,5
	    bcf		TRISB,4
	    bcf		TRISB,2
	    bcf		TRISB,3
	    bcf		STATUS,5
	    bcf		PORTA,0
	    clrf	PORTB
	    clrf	dataflags232
	    
	    
Start	    bcf		PORTA,1
	    bsf		PORTA,2
	    call	RecieveByte232
	    ;call	Delay_95us
	    btfss	dataflags232,0
	    goto	Start
	    bcf		dataflags232,0
	    bsf		PORTA,1
	    bcf		PORTA,2
	    
	    movlw	.85
	    subwf	ReadByte232,0
	    btfsc	STATUS,2
	    goto	EnableProgMode;Enable886
	    
	    movlw	.170
	    subwf	ReadByte232,0
	    btfsc	STATUS,2
	    goto	DisableProgMode;Disable886
	    
	    movlw	.18
	    subwf	ReadByte232,0
	    btfsc	STATUS,2
	    goto	RecieveCommand_com
	    
	    movlw	.68
	    subwf	ReadByte232,0
	    btfsc	STATUS,2
	    goto	RecieveExecuteCommand_com
	    
	    movlw	.13
	    subwf	ReadByte232,0
	    btfsc	STATUS,2
	    goto	Recieve2BData_com
	    
	    movlw	.53
	    subwf	ReadByte232,0
	    btfsc	STATUS,2
	    goto	ExeCommandandSend2BData_com
	    
	    movlw	.93
	    subwf	ReadByte232,0
	    btfsc	STATUS,2
	    goto	ExeCommandandRead2BData_com
	    
	    movlw	.140
	    subwf	ReadByte232,0
	    btfsc	STATUS,2
	    goto	Send2BData_com
	    
	    movlw	.75
	    subwf	ReadByte232,0
	    btfsc	STATUS,2
	    goto	Enable5VPower
	    
	    movlw	.34
	    subwf	ReadByte232,0
	    btfsc	STATUS,2
	    goto	Disable5VPower
	    
	    movlw	.51
	    movwf	WriteByte232
	    call	TransmitByte232
	    goto	Start
	
	    
EnableProgMode;Enable886
	    bsf		STATUS,5
	    bcf		TRISB,4
	    bcf		TRISB,5
	    bcf		STATUS,5
	    bcf		PORTB,4
	    bcf		PORTB,5
	    bsf		PORTA,0	    ;PWR On LED on
	    call	Delay_800ms
	    bsf		PORTB,2
	    call	Delay_8us
	    bsf		PORTB,3
	    ;bsf		STATUS,5
	    ;bsf		TRISB,4
	    ;bcf		STATUS,5
	    goto	Start
	    
	    
DisableProgMode;Disable886  
	    bcf		STATUS,5
	    bcf		PORTA,0	    ;PWR On LED off
	    movlw	b'11110011'
	    andwf	PORTB,1

	    goto	Start
	    
	    
Enable5VPower
	    bcf		STATUS,5
	    bsf		PORTA,0
	    bsf		PORTB,3
	    goto	Start
	    
	    
Disable5VPower
	    bcf		STATUS,5
	    bcf		PORTA,0
	    bcf		PORTB,3
	    goto	Start
	    
RecieveCommand_com
	    call	RecieveByte232
	    btfss	dataflags232,0
	    goto	RecieveCommand_com
	    bcf		dataflags232,0
	    movf	ReadByte232,0
	    movwf	CommandMemory
	    goto	Start
	    
	    
RecieveExecuteCommand_com
	    call	RecieveByte232
	    btfss	dataflags232,0
	    goto	RecieveExecuteCommand_com
	    bcf		dataflags232,0
	    movf	ReadByte232,0
	    movwf	CommandBuff
	    call	SendCommand
	    goto	Start
	    
Recieve2BData_com
	    call	RecieveByte232
	    btfss	dataflags232,0
	    goto	Recieve2BData_com
	    bcf		dataflags232,0
	    
	    movf	ReadByte232,0
	    movwf	DataLMemory
	    
	    
Recieve2BData_com2
	    ;call	Delay_50us
	    call	RecieveByte232
	    btfss	dataflags232,0
	    goto	Recieve2BData_com2
	    bcf		dataflags232,0
	    
	    movf	ReadByte232,0
	    movwf	DataHMemory
	    goto	Start
	    
Send2BData_com
	    movf	DataLMemory,0
	    movwf	SendByteL
	    movf	DataHMemory,0
	    movwf	SendByteH
	    
	    call	SendData
	    goto	Start
	    
	    
ExeCommandandSend2BData_com
	    movf	CommandMemory,0
	    movwf	CommandBuff
	    movf	DataLMemory,0
	    movwf	SendByteL
	    movf	DataHMemory,0
	    movwf	SendByteH
	    call	SendCommand
	    call	SendData
	    
	    goto	Start
	    
	    
	    
	    
	    
ExeCommandandRead2BData_com	    
	    movf	CommandMemory,0
	    movwf	CommandBuff
	    call	SendCommand
	    call	ReadData
	    
	    movf	ReadedByteL,0
	    movwf	WriteByte232
	    call	TransmitByte232
	    ;call	Delay_100us
	    movf	ReadedByteH,0
	    movwf	WriteByte232
	    call	TransmitByte232
	    
	    goto	Start
	    
	    
	    
	    
	    
	    
; ------------------------Read data from 886 --------------	    
ReadData    clrf	ReadedByteL
	    clrf	ReadedByteH
	    movlw	.8
	    movwf	Counter
	    bsf		STATUS,5
	    bcf		TRISB,5
	    bsf		TRISB,4
	    bcf		STATUS,5
	    bsf		PORTB,5
	    ;;bcf		ReadedByteL,7
	    nop
	    bcf		PORTB,5
	    
	    
ReadL	    rrf		ReadedByteL,1
	    bcf		ReadedByteL,7
	    bsf		PORTB,5
	    nop
	    btfsc	PORTB,4
	    bsf		ReadedByteL,7
	    bcf		PORTB,5
	    
	    
	    decfsz	Counter,1
	    goto	ReadL
	    movlw	.6
	    movwf	Counter
	    
ReadH	    
	    bcf		ReadedByteH,7
	    bsf		PORTB,5
	    nop
	    btfsc	PORTB,4
	    bsf		ReadedByteH,7
	    bcf		PORTB,5
	    rrf		ReadedByteH,1
	    decfsz	Counter,1
	    goto	ReadH
	    bsf		PORTB,5
	    nop
	    bcf		PORTB,5
	    rrf		ReadedByteH,1
	    bcf		ReadedByteH,7
	    ;rrf		ReadedByteH,1
	    ;bcf		ReadedByteH,7
	    bcf		PORTB,4
	    bcf		PORTB,5
	    bsf		STATUS,5
	    bcf		TRISB,5
	    bcf		TRISB,4
	    bcf		STATUS,5
	    
	    return
;------------------------------------------------------	    
	    
	    
	    
;------------------   Send command to 886   -----------	    
	    
SendCommand movlw	.6
	    movwf	Counter
	    bsf		STATUS,5
	    bcf		TRISB,4
	    bcf		TRISB,5
	    bcf		STATUS,5
NextBitCom  bcf		PORTB,4
	    btfsc	CommandBuff,0
	    bsf		PORTB,4
	    bsf		PORTB,5
	    nop
	    bcf		PORTB,5
	    rrf		CommandBuff,1
	    decfsz	Counter,1
	    goto	NextBitCom
	    return
;-------------------------------------------------------	    
	    

;-----------------    Send data to 886    ----------------	    
	    
SendData    movlw	.8
	    movwf	Counter
	    rlf		SendByteL,1
	    rlf		SendByteH,1
	    bcf		SendByteL,0
	    bcf		SendByteH,7
	    bsf		STATUS,5
	    bcf		TRISB,4
	    bcf		TRISB,5
	    bcf		STATUS,5
NextBitSendL 
	    bcf		PORTB,4
	    btfsc	SendByteL,0
	    bsf		PORTB,4
	    bsf		PORTB,5
	    nop
	    bcf		PORTB,5
	    rrf		SendByteL,1
	    decfsz	Counter,1
	    goto	NextBitSendL
	    
	    movlw	.8
	    movwf	Counter
NextBitSendH ;bsf	STATUS,5
	    ;bcf		TRISB,4
	    ;bcf		STATUS,5
	    bcf		PORTB,4
	    btfsc	SendByteH,0
	    bsf		PORTB,4
	    bsf		PORTB,5
	    nop
	    bcf		PORTB,5
	    rrf		SendByteH,1
	    decfsz	Counter,1
	    goto	NextBitSendH
	    call	Delay_100us
	    return
;---------------------------------------------------------

	    
	    

	    
	    
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;	    
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;	    
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;	    
	    
	    
	    
	    
	    
	    
;--------------------    TRANSMIT BYTE 232    --------------	    
TransmitByte232 movlw	.8
	    movwf	Counter232
	    bcf		STATUS,5
	    bsf		PORTB,0
	    
	    bsf		STATUS,5
	    bcf		TRISB,0
	    bcf		STATUS,5
	    bcf		PORTB,0
	    ;call	Delay_826us	    ;baud rate 1200
	    call	Delay_95us	    ;baud rate 9600
	    nop
	    nop
	    nop

reptrbit232 call	TransmitBit232
	    rrf		WriteByte232,1
	    ;call	Delay_826us	    ;baud rate 1200
	    call	Delay_95us	    ;baud rate 9600
	    decfsz	Counter232,1
	    goto	reptrbit232
	    bsf		STATUS,5
	    bsf		TRISB,0
	    bcf		STATUS,5
	    bsf		PORTB,0
	    call	Delay_100us
	    return
	    
TransmitBit232 btfsc	WriteByte232,0
	    goto	$+3
	    bcf		PORTB,0
	    return
	    bsf		PORTB,0
	    return    
;----------------------------------------------------------	    

	    
;--------------------     RECIEVE BYTE  232   ----------------	    
RecieveByte232 
	    bcf		STATUS,5
	    btfsc	PORTB,1
	    return
	    bsf		dataflags232,0
	    clrf	ReadByte232
	    movlw	.8
	    movwf	Counter232
	    ;call	Delay_50us	    ;baud rate 1200
	    call	Delay_10us	    ;baud rate 9600
	    
reprebit    ;call	Delay_826us	    ;baud rate 1200
	    call	Delay_95us	    ;baud rate 9600
	    nop
	    nop
	    rrf		ReadByte232,1
	    call	RecieveBit
	    
	    decfsz	Counter232,1
	    goto	reprebit
	    
wait1	    btfss	PORTB,1
	    goto	wait1
	    return
	    
	    
	    
RecieveBit  btfsc	PORTB,1
	    goto	$+3
	    bcf		ReadByte232,7
	    return
	    bsf		ReadByte232,7
	    return
    
;----------------------------------------------------------
	    
Delay_100us movlw       .33
            movwf       Reg_1
            decfsz      Reg_1,F
            goto        $-1   
	    return
	    
Delay_95us  movlw       .31
            movwf       Reg_1
            decfsz      Reg_1,F
            goto        $-1
            nop
	    return
	    
	    
Delay_50us  movlw       .16
            movwf       Reg_1
            decfsz      Reg_1,F
            goto        $-1
	    return
	    
Delay_10ms  movlw       .251
            movwf       Reg_1
            movlw       .13
            movwf       Reg_2
            decfsz      Reg_1,F
            goto        $-1
            decfsz      Reg_2,F
            goto        $-3
            nop
            nop
	    return
	    
Delay_10us  movlw       .3
            movwf       Reg_1
            decfsz      Reg_1,F
            goto        $-1
	    return
	    
Delay_8us   movlw       .2
            movwf       Reg_1
            decfsz      Reg_1,F
            goto        $-1
            nop
	    return
	    
Delay_800ms movlw       .241
            movwf       Reg_1
            movlw       .15
            movwf       Reg_2
            movlw       .5
            movwf       Reg_3
            decfsz      Reg_1,F
            goto        $-1
            decfsz      Reg_2,F
            goto        $-3
            decfsz      Reg_3,F
            goto        $-5
	    return
	    
	    end
