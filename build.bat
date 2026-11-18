@echo off
setlocal
set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe

echo [1/3] Compilando SARTranslator.dll...
%CSC% /target:library /out:assets\SARTranslator.dll /r:"X:\SteamLibrary\steamapps\common\Super Alloy Ranger\Super.Alloy.Ranger_Data\Managed\netstandard.dll" /r:"X:\SteamLibrary\steamapps\common\Super Alloy Ranger\Super.Alloy.Ranger_Data\Managed\UnityEngine.CoreModule.dll" /r:"X:\SteamLibrary\steamapps\common\Super Alloy Ranger\Super.Alloy.Ranger_Data\Managed\UnityEngine.UI.dll" /r:"X:\SteamLibrary\steamapps\common\Super Alloy Ranger\Super.Alloy.Ranger_Data\Managed\UnityEngine.TextRenderingModule.dll" src\SARTranslator.cs

echo [2/3] Compilando Instalador v1.2 (Multiplataforma: Steam + Epic Games)...
%CSC% /target:winexe /win32icon:assets\app_icon.ico /out:release\Instalador_Traducao_SuperAlloyRanger_PTBR.exe /resource:assets\installer_bg.jpg,installer_bg.jpg /resource:assets\app_icon.ico,app_icon.ico /resource:assets\Assembly-CSharp.steam.dll,Assembly-CSharp.steam.dll /resource:assets\Assembly-CSharp.epic.dll,Assembly-CSharp.epic.dll /resource:assets\SARTranslator.dll,SARTranslator.dll /resource:translation_ptbr.txt,translation_ptbr.txt /resource:LEIA-ME.txt,LEIA-ME.txt src\Installer.cs

echo [3/3] Gerando pacote ZIP de distribuicao...
powershell -NoProfile -ExecutionPolicy Bypass -Command "& { $zip = 'release\SuperAlloyRanger_Traducao_PTBR.zip'; if (Test-Path $zip) { Remove-Item $zip -Force }; $tmp = 'release\tmp_zip'; if (Test-Path $tmp) { Remove-Item $tmp -Recurse -Force }; New-Item -ItemType Directory -Path \"$tmp\Super.Alloy.Ranger_Data\Managed\" -Force | Out-Null; Copy-Item 'release\Instalador_Traducao_SuperAlloyRanger_PTBR.exe' \"$tmp\Instalador_Traducao_SuperAlloyRanger_PTBR.exe\"; Copy-Item 'LEIA-ME.txt' \"$tmp\LEIA-ME.txt\"; Copy-Item 'translation_ptbr.txt' \"$tmp\Super.Alloy.Ranger_Data\Managed\translation_ptbr.txt\"; Copy-Item 'translation_ptbr.txt' \"$tmp\Super.Alloy.Ranger_Data\translation_ptbr.txt\"; Copy-Item 'assets\SARTranslator.dll' \"$tmp\Super.Alloy.Ranger_Data\Managed\SARTranslator.dll\"; New-Item -ItemType Directory -Path \"$tmp\Versao_Steam\Super.Alloy.Ranger_Data\Managed\" -Force | Out-Null; Copy-Item 'assets\Assembly-CSharp.steam.dll' \"$tmp\Versao_Steam\Super.Alloy.Ranger_Data\Managed\Assembly-CSharp.dll\"; New-Item -ItemType Directory -Path \"$tmp\Versao_EpicGames\Super.Alloy.Ranger_Data\Managed\" -Force | Out-Null; Copy-Item 'assets\Assembly-CSharp.epic.dll' \"$tmp\Versao_EpicGames\Super.Alloy.Ranger_Data\Managed\Assembly-CSharp.dll\"; Compress-Archive -Path \"$tmp\*\" -DestinationPath $zip -Force; Remove-Item $tmp -Recurse -Force; Write-Output \"ZIP criado com sucesso!\" }"

echo ==============================================
echo Build finalizado com sucesso!
echo ==============================================
