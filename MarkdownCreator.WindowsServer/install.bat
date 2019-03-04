rem *************************************
rem 正在安装Markdown Creator服务...
rem *************************************
cd /d %~dp0
C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\installutil MarkdownCreator.WindowsServer.exe
net start MarkdownCreator
pause