[hacknet_url]:http://www.hacknet-os.com/
# 介绍DecEnc工具
DecEnc是一个.dec文件加/解密器，可以快速生成.dec文件供[Hacknet](hacknet_url)游戏的Mod制作。
如果你看不懂以上内容，我建议你先玩玩[Hacknet](hacknet_url)这个游戏。

-----------------------------------------------------------------------------------------------------------

# DecEnc工具使用方法
DecEnc /i <SourceFile> \[/p password\] \[/d\] \[/f\] \[/s signature\] \[/h header\] \[/inf\]
* /i <SourceFile>: 指定输入文件，当加密模式时SourceFile可以是任意文件，当解密模式时SourceFile为加密过的.dec文件
* /p password: 指定加密/解密使用的密码
* /d: 当此项选中时程序以解密模式运行，如不选则以加密模式运行
* /f: 解密模式下如此项被选中则会让程序尝试破解该.dec文件的密码并解密，且/p password选项将会失效
* /s signature: 在加密模式下指定签名地址\(signature\)，当游戏内对加密后文件执行DECHead命令时IP将会显示为指定的签名地址
* /h header: 加密时指定DEC文件头，DEC文件头作用不太清楚，可能是DECHead显示的一项内容\(DEC文件头可随意指定，不影响解密\)
* /inf: 解密模式中如果此项被选定，程序将会在控制台输出文件的DEC文件头、签名地址以及原文件后缀
