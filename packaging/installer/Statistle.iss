; Inno Setup script for the Statistle tester installer.
;
; Built by packaging\make-installer.ps1, which publishes the app as one self-contained exe,
; stages it with the sample projects and the README, and passes the version and folders in
; as defines. Compile by hand with:
;
;   ISCC.exe /DAppVersion=0.5.1 /DStageDir=..\..\artifacts\installer\stage /DOutputDir=..\..\artifacts Statistle.iss
;
; The installer is per-user by default (no administrator rights, lands in
; %LOCALAPPDATA%\Programs\Statistle); the wizard offers "all users" to an administrator.
; The user's files stay in Documents\Statistle and are not touched by install or uninstall.

#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif
#ifndef StageDir
  #define StageDir "..\..\artifacts\installer\stage"
#endif
#ifndef OutputDir
  #define OutputDir "..\..\artifacts"
#endif

[Setup]
AppId={{7D9C2F4E-3B61-4C8A-9E2F-5A1D0B6C8E31}
AppName=Statistle
AppVersion={#AppVersion}
AppVerName=Statistle {#AppVersion}
AppPublisher=Ron Eaglin
AppComments=Statistics for engineers: the EGN3443 course calculations, with the Python and R that reproduce them.
VersionInfoVersion={#AppVersion}.0
DefaultDirName={autopf}\Statistle
DefaultGroupName=Statistle
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.17763
OutputDir={#OutputDir}
OutputBaseFilename=Statistle-{#AppVersion}-setup
SetupIconFile=..\..\src\Statistle.App\Statistle.ico
UninstallDisplayIcon={app}\Statistle.exe
UninstallDisplayName=Statistle
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ChangesAssociations=yes
CloseApplications=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "fileassoc"; Description: "Open .statistle project files with Statistle"; GroupDescription: "File association:"

[Files]
Source: "{#StageDir}\Statistle.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#StageDir}\README.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#StageDir}\samples\*.statistle"; DestDir: "{app}\samples"; Flags: ignoreversion

[Icons]
Name: "{group}\Statistle"; Filename: "{app}\Statistle.exe"; Comment: "Statistics for engineers"
Name: "{group}\Statistle README"; Filename: "{app}\README.txt"
Name: "{group}\Uninstall Statistle"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Statistle"; Filename: "{app}\Statistle.exe"; Tasks: desktopicon

[Registry]
; HKA is HKLM for an all-users install and HKCU for a per-user one.
Root: HKA; Subkey: "Software\Classes\.statistle"; ValueType: string; ValueName: ""; ValueData: "Statistle.Project"; Flags: uninsdeletevalue uninsdeletekeyifempty; Tasks: fileassoc
Root: HKA; Subkey: "Software\Classes\Statistle.Project"; ValueType: string; ValueName: ""; ValueData: "Statistle project"; Flags: uninsdeletekey; Tasks: fileassoc
Root: HKA; Subkey: "Software\Classes\Statistle.Project\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\Statistle.exe,0"; Tasks: fileassoc
Root: HKA; Subkey: "Software\Classes\Statistle.Project\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\Statistle.exe"" ""%1"""; Tasks: fileassoc

[Run]
Filename: "{app}\Statistle.exe"; Description: "{cm:LaunchProgram,Statistle}"; Flags: nowait postinstall skipifsilent
