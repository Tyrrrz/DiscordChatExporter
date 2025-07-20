[Setup]
AppName=Discord Chat Exporter CLI
AppVersion=2.50.0
DefaultDirName={pf}\DiscordChatExporter
DefaultGroupName=DiscordChatExporter
OutputBaseFilename=DiscordChatExporterSetup
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Files]
Source: "App\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\DiscordChatExporter CLI"; Filename: "{app}\DiscordChatExporter.Cli.exe"
Name: "{group}\Uninstall DiscordChatExporter"; Filename: "{uninstallexe}"

[Tasks]
Name: addtopath; Description: "Add DiscordChatExporter to system PATH (recommended for CLI use)"; \
GroupDescription: "Additional Options:"

[Run]
Filename: "{app}\DiscordChatExporter.Cli.exe"; Description: "Run DiscordChatExporter"; \
Flags: postinstall nowait skipifsilent

[Code]
procedure AddAppToPath();
var
  Path: string;
  NewPath: string;
begin
  if RegQueryStringValue(HKEY_CURRENT_USER, 'Environment', 'Path', Path) then
  begin
    if Pos(ExpandConstant('{app}'), Path) = 0 then
    begin
      NewPath := Path;
      if (Length(NewPath) > 0) and (NewPath[Length(NewPath)] <> ';') then
        NewPath := NewPath + ';';
      NewPath := NewPath + ExpandConstant('{app}');
      RegWriteStringValue(HKEY_CURRENT_USER, 'Environment', 'Path', NewPath);
    end;
  end
  else
  begin
    RegWriteStringValue(HKEY_CURRENT_USER, 'Environment', 'Path', ExpandConstant('{app}'));
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep = ssPostInstall) and WizardIsTaskSelected('addtopath') then
  begin
    AddAppToPath();
  end;
end;

