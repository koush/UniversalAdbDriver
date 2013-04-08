inf2cat /driver:C:\Users\koush\Desktop\UniversalAdbDriver\usb_driver\ /os:XP_X86
inf2cat /driver:C:\Users\koush\Desktop\UniversalAdbDriver\usb_driver\ /os:XP_X64

signtool sign /v /s PrivateCertStore /n ClockworkMod /t http://timestamp.verisign.com/scripts/timstamp.dll usb_driver\androidwinusb86.cat
signtool sign /v /s PrivateCertStore /n ClockworkMod /t http://timestamp.verisign.com/scripts/timstamp.dll usb_driver\androidwinusba64.cat