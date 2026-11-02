; Webo installer (Inno Setup 6). Per-user install by default (no admin needed), matching decision D1/D7.
; Build:  ISCC.exe /DAppVersion=0.3.0 installer\webo.iss   (expects the publish output in ..\out)

#ifndef AppVersion
  #define AppVersion "0.3.0"
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
; Icon policy (decision D2, revised): the MONO icon is used by the installer only (Setup.exe and the wizard window).
; Everything the user sees outside the installer uses the original DARK icon: the app's exe, the Start menu and
; desktop shortcuts, and the entry in Settings > Apps (UninstallDisplayIcon below).
SetupIconFile=..\src\Webo.App\Assets\Brand\derived\icons\mono\webo-mono.ico
UninstallDisplayIcon={app}\{#IconRel}

; Wizard artwork. Drop the files named below into installer\assets\ and they are picked up automatically; until then
; Inno Setup's built-in images are used. Setup picks the size that best matches the screen DPI.
;   Large image (Welcome / Finish pages, left side), 24-bit BMP:
;     wizard-large-164x314.bmp (100%)  192x386 (125%)  246x459 (150%)  273x556 (175%)
;     328x604 (200%)  355x700 (225%)  410x797 (250%)      -> file names: wizard-large-<W>x<H>.bmp
;   Small image (top-right of the other pages), 24-bit BMP, square:
;     wizard-small-58x58.bmp (100%)  77x77 (125%)  97x97 (150%)  116x116 (175%)
;     124x124 (200%)  143x143 (225%)  159x159 (250%)      -> file names: wizard-small-<W>x<H>.bmp
#ifexist "assets\wizard-large-164x314.bmp"
WizardImageFile=assets\wizard-large-*.bmp
#endif
#ifexist "assets\wizard-small-58x58.bmp"
WizardSmallImageFile=assets\wizard-small-*.bmp
#endif
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
