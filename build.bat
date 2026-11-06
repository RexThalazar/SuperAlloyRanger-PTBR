@echo off
set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe

echo Compilando SARTranslator.dll...
%CSC% /target:library /out:..\release\SARTranslator.dll /r:..\..\Managed\netstandard.dll /r:..\..\Managed\UnityEngine.CoreModule.dll /r:..\..\Managed\UnityEngine.UI.dll /r:..\..\Managed\UnityEngine.TextRenderingModule.dll src\SARTranslator.cs

echo Compilando Instalador...
%CSC% /target:winexe /win32icon:assets\app_icon.ico /out:release\Instalador_Traducao_SuperAlloyRanger_PTBR.exe /resource:assets\installer_bg.jpg,installer_bg.jpg /resource:assets\app_icon.ico,app_icon.ico /resource:..\Assembly-CSharp.patched.dll,Assembly-CSharp.dll /resource:..\SARTranslator.dll,SARTranslator.dll /resource:translation_ptbr.txt,translation_ptbr.txt /resource:LEIA-ME.txt,LEIA-ME.txt src\Installer.cs

echo Concluido!
pause
