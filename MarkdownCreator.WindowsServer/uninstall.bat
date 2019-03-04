rem *************************************
rem 正在卸载Markdown Creator服务...
rem *************************************
cd /d %~dp0
net stop MarkdownCreator
C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\installutil /u MarkdownCreator.WindowsServer.exe
pause