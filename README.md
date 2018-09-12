UniversalAdbDriver
==================

A single Windows driver that supports the ADB (and fastboot) interface for most Android phones.


[Download Windows Installer](http://download.clockworkmod.com/test/UniversalAdbDriverSetup.msi)

本项目是从其他项目fork过来的，他的做法是把各种手机型号的 硬件ID写进.inf这样做会使 .inf非常庞大，并且并不能保证全面。
%Samsung%     = USB_Install_17, USB\VID_04E8&PID_685E&REV_0400&MI_03
%Samsung%     = USB_Install_17, USB\VID_04E8&PID_6866&REV_0228&MI_01


Adb 驱动本身就是通用的，不同的设备的区别在于 .inf 文件中的 VID 和PID 以及接口不同，如果你使用compatible ID 的话就可以把驱动变成通用驱动。
如下所示：

%CompositeAdbInterface% = USB_Install, USB\Class_FF&SubClass_42&Prot_01  
%SingleAdbInterface% = USB_Install, USB\Class_FF&SubClass_42&Prot_03

这样做存在的不足是，在用inf2cat.exe 从inf生成cat的时候，会出错，但是有一个工具可以完成。
亚信数字签名：http://www.trustasia.com/solutions/signtools/ 
需要导入证书 并添加证书规则才可以使用。

生成证书参考：
http://www.cnblogs.com/bearhb/archive/2012/07/03/2574206.html

