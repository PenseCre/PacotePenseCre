; -- PenseCreTemplate.iss --
; Demonstrates a multilingual installation of a
; windows Unity game built with the Pacote Pense & Cre.
; This script is based off the Inno Setup Script Wizard.

; SEE THE DOCUMENTATION FOR DETAILS ON CREATING .ISS SCRIPT FILES!

; #defines managed by Pense & CRE package inside Unity
#define MyAppName "Nosso Projeto"
#define MyAppVersion "0.0.1"
#define MyAppPublisher "Pense & Cre"
#define MyAppExeName "Nosso Projeto.exe" 
#define InputDir "..\Builds\Windows\Release\Main"
#define MyInstallerName "NossoProjeto_0.0.1_2022-04-22_13-00.exe"
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, select value below, click Tools > Generate GUID inside the InnoSetupCompiler IDE.)
#define MyAppId "{{0F4BE4DB-FDDD-489F-88EC-59C323987541}}"
;#define MyIcon ""

; #defines self managed
#define Date GetDateTimeString('yyyy/mm/dd', '-', '')

[Setup]
AppName={#MyAppName}
AppPublisher={#MyAppPublisher}
AppId={#MyAppId}
VersionInfoDescription={#MyAppName} Setup
VersionInfoProductName={#MyAppName}
OutputDir=..\Builds\
AppVersion={#MyAppVersion}
AppVerName={#MyAppVersion}
DefaultDirName={autopf}\{#MyAppName}
; Comment the line above and uncomment the line below so that the default location is in the C drive
;DefaultDirName=C:\{#MyAppName}
DefaultGroupName={#MyAppName}
WizardStyle=modern
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
; Comment line below and uncomment following to add version+date to the installer
OutputBaseFilename={#MyInstallerName}
;OutputBaseFilename={#MyInstallerName}_{#MyAppVersion}_{#Date}
UninstallDisplayIcon={app}\{#MyAppName}
Compression=lzma
SolidCompression=yes
MissingMessagesWarning=yes
NotRecognizedMessagesWarning=yes
; Uncomment the following line to disable the "Select Setup Language"
; dialog and have it rely solely on auto-detection.
;ShowLanguageDialog=no

[Languages]
Name: en; MessagesFile: "compiler:Default.isl"
Name: pt; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: es; MessagesFile: "compiler:Languages\Spanish.isl"

[Messages]
en.BeveledLabel=English
pt.BeveledLabel=Português
es.BeveledLabel=Español

[CustomMessages]
en.MyDescription=My description
en.MyAppName=My Program
en.MyAppVerName=My Program %1
en.CreateStartupIcon=Start application when user logs in
pt.MyDescription=Meu programa
pt.MyAppName=Meu programa
pt.MyAppVerName=Meu programa %1
pt.CreateStartupIcon=Iniciar o programa quando o usuário fazer o login
es.MyDescription=Mi programa
es.MyAppName=Mi programa
es.MyAppVerName=Mi programa %1
es.CreateStartupIcon=Iniciar la aplicación cuando el usuario inicie sesión

[Files]
Source: "{#InputDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#InputDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
;Source: "Readme.txt"; DestDir: "{app}"; Languages: en; Flags: isreadme
;Source: "Readme-Portuguese.txt"; DestName: "Leia-me.txt"; DestDir: "{app}"; Languages: pt; Flags: isreadme
;Source: "Readme-Spanish.txt"; DestName: "Léame.txt"; DestDir: "{app}"; Languages: es; Flags: isreadme

[Icons]
;Name: "{group}\{cm:MyAppName}"; Filename: "{app}\MyProg.exe"
;Name: "{group}\{cm:UninstallProgram,{cm:MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Parameters: "";
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Parameters: ""; Tasks: desktopicon
Name: "{autostartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Parameters: ""; Tasks: startupicon

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}";
Name: "startupicon"; Description: "{cm:CreateStartupIcon}"; GroupDescription: "{cm:AdditionalIcons}";

[Run]
Filename: "{app}\{#MyAppExeName}"; Parameters: ""; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent