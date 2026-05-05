; ===========================================================================
;  Pharmacy Saas - Inno Setup script
;  Builds a Windows installer for the WPF desktop app.
;  Version is supplied by CI via /DAppVersion=...
; ===========================================================================

#ifndef AppVersion
  #define AppVersion "0.1.0"
#endif

#define AppName        "Pharmacy Saas"
#define AppPublisher   "Pharmacy Saas"
#define AppExeName     "PharmacySaas.exe"
#define AppId          "{{2B8C6C2C-1A8E-4C9A-9F0D-PM50000F0001}}"

[Setup]
AppId={#AppId}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppSupportURL=https://github.com/your-org/pharmacy-saas
AppUpdatesURL=https://github.com/your-org/pharmacy-saas/releases
DefaultDirName={autopf}\PharmacySaas
DefaultGroupName=Pharmacy Saas
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=PharmacySaas-Setup-{#AppVersion}
SetupIconFile=
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
DisableDirPage=auto
UninstallDisplayIcon={app}\{#AppExeName}
UninstallDisplayName={#AppName}
VersionInfoVersion={#AppVersion}.0
VersionInfoCompany={#AppPublisher}
VersionInfoDescription={#AppName} - Offline-first Pharmacy ERP
VersionInfoProductName={#AppName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "..\publish\desktop\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Dirs]
Name: "{localappdata}\PharmacySaas"

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Launch {#AppName}"; Flags: nowait postinstall skipifsilent unchecked

[UninstallDelete]
; Default uninstall keeps the local pharmacy database (under %LOCALAPPDATA%\PharmacySaas).
; Users can manually delete that folder if they want a full reset.

[Messages]
WelcomeLabel2=This will install [name/ver] on your computer.%n%nPharmacy Saas is an offline-first Pharmacy ERP. Sales, purchases, stock and reports work without internet.%n%nNote: This build connects to SQL Server Express on the local machine (.\\SQLEXPRESS by default). Make sure SQL Server Express is installed before launching the app.%n%nClick Next to continue.

[Code]
function InitializeSetup(): Boolean;
begin
  Result := True;
end;
