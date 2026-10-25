; Webo installer (Inno Setup 6). Per-user install by default (no admin needed), matching decision D1/D7.
; Build:  ISCC.exe /DAppVersion=0.2.0 installer\webo.iss   (expects the publish output in ..\out)

#ifndef AppVersion
  #define AppVersion "0.2.0"
#endif
#define AppName "Webo"
#define AppExe "Webo.exe"
#define IconRel "Assets\Brand\derived\icons\dark\webo-dark.ico"

[Setup]
AppId={{F236C1B9-6933-4D69-9A10-9061476E954D}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher=Creatix Studio
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.17763
OutputDir=..\dist
OutputBaseFilename=Webo-Setup-{#AppVersion}
; The installer and the executable use the fixed default (dark) icon (decision D2).
SetupIconFile=..\src\Webo.App\Assets\Brand\derived\icons\dark\webo-dark.ico
UninstallDisplayIcon={app}\{#IconRel}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Shortcuts:"; Flags: unchecked

[Files]
Source: "..\out\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"; IconFilename: "{app}\{#IconRel}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; IconFilename: "{app}\{#IconRel}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "Launch {#AppName}"; Flags: nowait postinstall skipifsilent

; Uninstall removes only what the installer put in {app}.
; User projects (wherever the user saved them) and settings in %LOCALAPPDATA%\Webo are never deleted.
