#define AppName "MacFontRenderer"
#define AppVersion GetEnv("MACFONTRENDERER_VERSION")
#if AppVersion == ""
  #define AppVersion "1.0.0"
#endif

[Setup]
AppId={{9E9A55D4-5E4F-42B1-A984-8CE6F4D4F714}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=BlackLine
AppPublisherURL=https://github.com/
AppSupportURL=https://github.com/
AppUpdatesURL=https://github.com/
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir=..\dist\installer
OutputBaseFilename=MacFontRenderer-Setup-{#AppVersion}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
UninstallDisplayIcon={app}\MacFontRenderer.exe
SetupLogging=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "installservice"; Description: "Install background font rendering service"; GroupDescription: "Service options:"; Flags: checkedonce

[Dirs]
Name: "{commonappdata}\MacFontRenderer"
Name: "{commonappdata}\MacFontRenderer\Assets"
Name: "{commonappdata}\MacFontRenderer\Hooks"

[Files]
Source: "..\artifacts\publish\app\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\artifacts\publish\service\*"; DestDir: "{app}\Service"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\fonts\*"; DestDir: "{commonappdata}\MacFontRenderer\Assets\fonts"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\fonts\FontMod64.dll"; DestDir: "{commonappdata}\MacFontRenderer\Assets"; Flags: ignoreversion
Source: "..\fonts\FontMod64.dll"; DestDir: "{commonappdata}\MacFontRenderer\Hooks"; DestName: "FontMod.dll"; Flags: ignoreversion
Source: "..\fonts\FontMod.yaml.txt"; DestDir: "{commonappdata}\MacFontRenderer"; DestName: "FontMod.yaml"; Flags: ignoreversion onlyifdoesntexist

[Icons]
Name: "{group}\MacFontRenderer"; Filename: "{app}\MacFontRenderer.exe"
Name: "{commondesktop}\MacFontRenderer"; Filename: "{app}\MacFontRenderer.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\Service\MacFontRenderer.Service.exe"; Parameters: "install"; StatusMsg: "Installing MacFontRenderer service..."; Flags: runhidden waituntilterminated; Tasks: installservice
Filename: "sc.exe"; Parameters: "start MacFontRendererService"; StatusMsg: "Starting MacFontRenderer service..."; Flags: runhidden waituntilterminated; Tasks: installservice
Filename: "{app}\MacFontRenderer.exe"; Description: "Launch MacFontRenderer"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "sc.exe"; Parameters: "stop MacFontRendererService"; Flags: runhidden waituntilterminated; RunOnceId: "StopService"
Filename: "{app}\Service\MacFontRenderer.Service.exe"; Parameters: "uninstall"; Flags: runhidden waituntilterminated; RunOnceId: "UninstallService"

[Code]
function InitializeSetup(): Boolean;
begin
  Result := True;
  MsgBox(
    'This installer bundles Apple SF/New York font files and a FontMod hook DLL because the project owner explicitly accepted that licensing/security risk.' + #13#10#13#10 +
    'Install only on systems where you are authorized to use these assets.',
    mbInformation,
    MB_OK
  );
end;
