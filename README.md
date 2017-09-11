# PicProger
A programmer for PIC16 chips.
Only works now with PIC16F886 and a computer SW is crap (only works now if com is COM1).

# Commands for programmer
85  - Enable programming mode.
170 - Disable programming mode.
18  - Receive command for PIC and store it.
68  - Receive command for PIC and execute immediately.
16  - Reveive 2 Bytes of data and store them.
53  - Execute stored command and send stored data sequentially.
93  - Execute stored command, read 2 Bytes of data from PIC and send them to a computer immediately.
140 - Send stored data to a PIC.
75  - Enable 5V power for PIC.
34  - Disable 5V power for PIC.


# Commands for Pic chip
Programming specification http://ww1.microchip.com/downloads/en/DeviceDoc/41287D.pdf

xxxx0000 + 0,(data 14),0        - Load configuration 
xxxx0010 + 0,(data 14),0        - Load data for program memory
xxxx0011 + 0,(data 8), 0000000  - Load data for data memory
xxxx0100 + 0,(data 14),0        - Read data from program memory
xxxx0101 + 0,(data 8), 0000000  - Read data from data memory
xxxx0110                        - Increment address
xxx01000                        - Begin programming (Internal Clock)
xxx11000                        - Begin programming (External Clock)
xxx01010                        - End programming
xxxx1001                        - Bulk erase program memory
xxxx1011                        - Bulk erase data memory
xxx10001                        - Row erase program memory
