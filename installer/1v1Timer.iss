; 1v1 Timer's install wizard (Capture's, adapted): language, welcome, folder, options, install, finish.
; It carries Velopack's one-click Setup and runs it silently into the chosen folder, so Velopack still owns the
; install (updates, the Apps & features entry, the uninstaller); the wizard itself installs nothing to uninstall.
; Built by tools\release.ps1:  ISCC /DAppVersion=1.0.0 /DPackId=1v1Timer /DOutDir=<releases dir> 1v1Timer.iss
; (it takes <OutDir>\<PackId>-win-Setup.exe, made there by vpk pack just before).
; The timer needs nothing else: .NET is inside the program, and there is no Visual C++ runtime to install.

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#ifndef PackId
  #define PackId "1v1Timer"
#endif
#ifndef OutDir
  #define OutDir "..\releases"
#endif
; The name people see, and the timer's settings folder (%AppData%\1v1 Timer).
#define AppTitle "1v1 Timer"
#define VeloSetup OutDir + "\" + PackId + "-win-Setup.exe"
; About what an install takes on its drive: the app (about twice the package) and the package Velopack keeps for updates;
; and the Setup itself in the temporary folder while it runs.
#define NeedMB FileSize(VeloSetup) / 1048576 * 4
#define TempMB FileSize(VeloSetup) / 1048576

[Setup]
AppId={#PackId}.Wizard
AppName={#AppTitle}
AppVersion={#AppVersion}
AppVerName={#AppTitle} {#AppVersion}
AppPublisher={#AppTitle}
AppPublisherURL=https://github.com/yasirmz/1v1-Timer
DefaultDirName={code:DefaultDir}
UsePreviousAppDir=no
; The language the timer keeps under this AppId's key (WizardLanguage): installing over it opens in its language.
UsePreviousLanguage=yes
DisableWelcomePage=no
DisableDirPage=no
DisableProgramGroupPage=yes
DirExistsWarning=no
PrivilegesRequired=lowest
SetupLogging=yes
Uninstallable=no
CloseApplications=no
RestartApplications=no
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.19041
WizardStyle=modern dark
WizardImageFile=wizard.png
WizardSmallImageFile=wizard-small.png
SetupIconFile=..\1v1-timer-icon.ico
OutputDir={#OutDir}
; The file people download: 1v1timer-setup.exe
OutputBaseFilename=1v1timer-setup
; Velopack's Setup is mostly compressed already.
Compression=none

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "ar"; MessagesFile: "compiler:Languages\Arabic.isl"
Name: "zhHans"; MessagesFile: "Languages\ChineseSimplified.isl"
Name: "ru"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "es"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "ptBR"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "de"; MessagesFile: "compiler:Languages\German.isl"

[CustomMessages]
en.LangCode=en
ar.LangCode=ar
zhHans.LangCode=zh-Hans
ru.LangCode=ru
es.LangCode=es
ptBR.LangCode=pt-BR
de.LangCode=de
en.StartWithWindows=Start 1v1 Timer with Windows
ar.StartWithWindows=تشغيل 1v1 Timer مع بدء ويندوز
zhHans.StartWithWindows=开机时启动 1v1 Timer
ru.StartWithWindows=Запускать 1v1 Timer вместе с Windows
es.StartWithWindows=Iniciar 1v1 Timer con Windows
ptBR.StartWithWindows=Iniciar o 1v1 Timer com o Windows
de.StartWithWindows=1v1 Timer mit Windows starten
en.InstallingApp=Installing 1v1 Timer…
ar.InstallingApp=جارٍ تثبيت 1v1 Timer…
zhHans.InstallingApp=正在安装 1v1 Timer…
ru.InstallingApp=Установка 1v1 Timer…
es.InstallingApp=Instalando 1v1 Timer…
ptBR.InstallingApp=Instalando o 1v1 Timer…
de.InstallingApp=1v1 Timer wird installiert…
en.AppRunning=1v1 Timer is running. Quit it first (right-click its icon by the clock, then Exit), then choose Retry.
ar.AppRunning=يعمل 1v1 Timer الآن. أغلقه أولًا (انقر بزر الفأرة الأيمن على أيقونته بجوار الساعة، ثم «خروج»)، ثم اختر «إعادة المحاولة».
zhHans.AppRunning=1v1 Timer 正在运行。请先退出它（右键单击时钟旁的图标，然后选择“退出”），再选择“重试”。
ru.AppRunning=1v1 Timer запущен. Сначала закройте его (щёлкните правой кнопкой по его значку у часов и выберите «Выход»), затем нажмите «Повторить».
es.AppRunning=1v1 Timer está abierto. Ciérrelo primero (clic derecho en su icono junto al reloj y luego «Salir») y después elija Reintentar.
ptBR.AppRunning=O 1v1 Timer está aberto. Feche-o primeiro (clique com o botão direito no ícone ao lado do relógio e depois em “Sair”) e escolha Repetir.
de.AppRunning=1v1 Timer läuft. Beenden Sie es zuerst (Rechtsklick auf sein Symbol neben der Uhr, dann „Beenden“) und wählen Sie dann Wiederholen.
en.InstallFailed=1v1 Timer could not be installed (error %1).
ar.InstallFailed=تعذّر تثبيت 1v1 Timer (الخطأ %1).
zhHans.InstallFailed=无法安装 1v1 Timer（错误 %1）。
ru.InstallFailed=Не удалось установить 1v1 Timer (ошибка %1).
es.InstallFailed=No se pudo instalar 1v1 Timer (error %1).
ptBR.InstallFailed=Não foi possível instalar o 1v1 Timer (erro %1).
de.InstallFailed=1v1 Timer konnte nicht installiert werden (Fehler %1).
en.CauseBlocked=Windows or your antivirus blocked a file Setup needed. Allow 1v1 Timer in your antivirus (or pause it for a minute), then run this setup again.
ar.CauseBlocked=حجب ويندوز أو برنامج مكافحة الفيروسات ملفًا يحتاجه المثبّت. اسمح لـ 1v1 Timer في برنامج الحماية (أو أوقفه دقيقة)، ثم شغّل هذا المثبّت مرة أخرى.
zhHans.CauseBlocked=Windows 或杀毒软件拦截了安装所需的文件。请在杀毒软件中允许 1v1 Timer（或将其暂停一分钟），然后重新运行本安装程序。
ru.CauseBlocked=Windows или антивирус заблокировали нужный установщику файл. Разрешите 1v1 Timer в антивирусе (или приостановите его на минуту), затем снова запустите установку.
es.CauseBlocked=Windows o su antivirus bloqueó un archivo que el instalador necesitaba. Permita 1v1 Timer en su antivirus (o pause el antivirus un minuto) y vuelva a ejecutar este instalador.
ptBR.CauseBlocked=O Windows ou seu antivírus bloqueou um arquivo de que o instalador precisava. Permita o 1v1 Timer no antivírus (ou pause-o por um minuto) e execute este instalador de novo.
de.CauseBlocked=Windows oder Ihr Virenschutz hat eine Datei blockiert, die das Setup benötigt. Erlauben Sie 1v1 Timer in Ihrem Virenschutz (oder pausieren Sie ihn eine Minute) und starten Sie dieses Setup dann erneut.
en.CauseInUse=A file in this folder is in use. Close 1v1 Timer and any window open in that folder (or restart the PC), then run this setup again.
ar.CauseInUse=ملف في هذا المجلد قيد الاستخدام. أغلق 1v1 Timer وأي نافذة مفتوحة في ذلك المجلد (أو أعد تشغيل الجهاز)، ثم شغّل هذا المثبّت مرة أخرى.
zhHans.CauseInUse=此文件夹中的某个文件正在使用中。请关闭 1v1 Timer 以及在该文件夹中打开的窗口（或重启电脑），然后重新运行本安装程序。
ru.CauseInUse=Файл в этой папке используется. Закройте 1v1 Timer и все окна, открытые в этой папке (или перезагрузите ПК), затем снова запустите установку.
es.CauseInUse=Un archivo de esta carpeta está en uso. Cierre 1v1 Timer y cualquier ventana abierta en esa carpeta (o reinicie el PC) y vuelva a ejecutar este instalador.
ptBR.CauseInUse=Um arquivo nesta pasta está em uso. Feche o 1v1 Timer e qualquer janela aberta nessa pasta (ou reinicie o PC) e execute este instalador de novo.
de.CauseInUse=Eine Datei in diesem Ordner wird verwendet. Schließen Sie 1v1 Timer und alle Fenster in diesem Ordner (oder starten Sie den PC neu) und starten Sie dieses Setup dann erneut.
en.CauseSpace=The drive is full. Free some space or choose another folder, then run this setup again.
ar.CauseSpace=القرص ممتلئ. أفرغ بعض المساحة أو اختر مجلدًا آخر، ثم شغّل هذا المثبّت مرة أخرى.
zhHans.CauseSpace=磁盘已满。请释放一些空间或选择其他文件夹，然后重新运行本安装程序。
ru.CauseSpace=Диск заполнен. Освободите место или выберите другую папку, затем снова запустите установку.
es.CauseSpace=El disco está lleno. Libere espacio o elija otra carpeta y vuelva a ejecutar este instalador.
ptBR.CauseSpace=O disco está cheio. Libere espaço ou escolha outra pasta e execute este instalador de novo.
de.CauseSpace=Das Laufwerk ist voll. Schaffen Sie Platz oder wählen Sie einen anderen Ordner und starten Sie dieses Setup dann erneut.
en.CauseOther=Run this setup again. If it fails again, attach the log to a GitHub issue (github.com/yasirmz/1v1-Timer/issues).
ar.CauseOther=شغّل هذا المثبّت مرة أخرى. وإن فشل ثانيةً، أرفق السجل في بلاغ على GitHub (github.com/yasirmz/1v1-Timer/issues).
zhHans.CauseOther=请重新运行本安装程序。如果仍然失败，请把日志附到 GitHub issue（github.com/yasirmz/1v1-Timer/issues）中。
ru.CauseOther=Запустите установку ещё раз. Если ошибка повторится, приложите журнал к issue на GitHub (github.com/yasirmz/1v1-Timer/issues).
es.CauseOther=Vuelva a ejecutar este instalador. Si falla otra vez, adjunte el registro a un issue de GitHub (github.com/yasirmz/1v1-Timer/issues).
ptBR.CauseOther=Execute este instalador de novo. Se falhar outra vez, anexe o registro a uma issue no GitHub (github.com/yasirmz/1v1-Timer/issues).
de.CauseOther=Starten Sie dieses Setup erneut. Wenn es wieder fehlschlägt, hängen Sie das Protokoll an ein GitHub-Issue an (github.com/yasirmz/1v1-Timer/issues).
en.LogTail=The setup log ends with:
ar.LogTail=آخر ما في سجل التثبيت:
zhHans.LogTail=安装日志的最后几行：
ru.LogTail=Конец журнала установки:
es.LogTail=El registro de instalación termina con:
ptBR.LogTail=O registro da instalação termina com:
de.LogTail=Das Setup-Protokoll endet mit:
en.OpenLog=The full log is %1%nOpen it now?
ar.OpenLog=السجل الكامل في %1%nهل تريد فتحه الآن؟
zhHans.OpenLog=完整日志位于 %1%n现在打开吗？
ru.OpenLog=Полный журнал: %1%nОткрыть его сейчас?
es.OpenLog=El registro completo está en %1%n¿Abrirlo ahora?
ptBR.OpenLog=O registro completo está em %1%nAbrir agora?
de.OpenLog=Das vollständige Protokoll liegt unter %1%nJetzt öffnen?
en.NotEnoughSpace=There isn't enough free space on %1: 1v1 Timer needs about %2 MB. Free some space or choose another drive.
ar.NotEnoughSpace=لا توجد مساحة كافية على %1: يحتاج 1v1 Timer إلى نحو %2 ميجابايت. أفرغ بعض المساحة أو اختر قرصًا آخر.
zhHans.NotEnoughSpace=%1 上的可用空间不足：1v1 Timer 需要约 %2 MB。请释放一些空间或选择其他磁盘。
ru.NotEnoughSpace=Недостаточно места на %1: 1v1 Timer нужно около %2 МБ. Освободите место или выберите другой диск.
es.NotEnoughSpace=No hay espacio libre suficiente en %1: 1v1 Timer necesita unos %2 MB. Libere espacio o elija otra unidad.
ptBR.NotEnoughSpace=Não há espaço livre suficiente em %1: o 1v1 Timer precisa de cerca de %2 MB. Libere espaço ou escolha outra unidade.
de.NotEnoughSpace=Auf %1 ist nicht genug Platz frei: 1v1 Timer benötigt etwa %2 MB. Schaffen Sie Platz oder wählen Sie ein anderes Laufwerk.
en.FolderNotEmpty=Please choose an empty or new folder. Uninstalling 1v1 Timer removes its whole folder, so it must not share one with other files.
ar.FolderNotEmpty=يُرجى اختيار مجلد فارغ أو جديد. إزالة 1v1 Timer تحذف مجلده كاملًا، لذا يجب ألا يحوي ملفات أخرى.
zhHans.FolderNotEmpty=请选择一个空文件夹或新文件夹。卸载 1v1 Timer 会删除其整个文件夹，因此该文件夹不能包含其他文件。
ru.FolderNotEmpty=Выберите пустую или новую папку. При удалении 1v1 Timer удаляется вся его папка, поэтому в ней не должно быть других файлов.
es.FolderNotEmpty=Elija una carpeta vacía o nueva. Al desinstalar 1v1 Timer se borra toda la carpeta del programa, así que no debe compartirla con otros archivos.
ptBR.FolderNotEmpty=Escolha uma pasta vazia ou nova. Desinstalar o 1v1 Timer apaga a pasta inteira, então ela não pode ter outros arquivos.
de.FolderNotEmpty=Bitte wählen Sie einen leeren oder neuen Ordner. Beim Deinstallieren entfernt 1v1 Timer seinen ganzen Ordner, er darf also keine anderen Dateien enthalten.
en.FolderNotWritable=1v1 Timer cannot write to this folder. Choose one in your user folder or on another drive (folders such as Program Files need administrator rights).
ar.FolderNotWritable=لا يستطيع 1v1 Timer الكتابة في هذا المجلد. اختر مجلدًا داخل مجلد المستخدم أو على قرص آخر (مجلدات مثل Program Files تحتاج صلاحيات المسؤول).
zhHans.FolderNotWritable=1v1 Timer 无法写入此文件夹。请选择用户文件夹中或其他磁盘上的文件夹（Program Files 等文件夹需要管理员权限）。
ru.FolderNotWritable=1v1 Timer не может записывать в эту папку. Выберите папку в своём профиле или на другом диске (для папок вроде Program Files нужны права администратора).
es.FolderNotWritable=1v1 Timer no puede escribir en esta carpeta. Elija una en su carpeta de usuario o en otra unidad (carpetas como Archivos de programa requieren permisos de administrador).
ptBR.FolderNotWritable=O 1v1 Timer não consegue gravar nesta pasta. Escolha uma na sua pasta de usuário ou em outra unidade (pastas como Arquivos de Programas exigem direitos de administrador).
de.FolderNotWritable=1v1 Timer kann in diesen Ordner nicht schreiben. Wählen Sie einen Ordner in Ihrem Benutzerordner oder auf einem anderen Laufwerk (Ordner wie Programme erfordern Administratorrechte).

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"
; Off unless the user ticks it (the timer's own setting starts off too).
Name: "autostart"; Description: "{cm:StartWithWindows}"; Flags: unchecked

[Files]
; Run from [Code] once the progress page shows; Velopack's Setup extracts and installs the app itself.
Source: "{#VeloSetup}"; DestName: "{#PackId}-win-Setup.exe"; Flags: dontcopy

[Icons]
; Velopack's uninstaller removes every shortcut that points into the install folder, this one too. Its picture is the
; program's own icon (1v1-timer-icon.ico, in the .exe).
Name: "{userdesktop}\{#AppTitle}"; Filename: "{app}\{#PackId}.exe"; IconFilename: "{app}\current\{#PackId}.exe"; Tasks: desktopicon

[Registry]
; The same entry the timer keeps in step with its setting (Autostart.CommandFor, AppInfo.RunValueName): the wizard writes
; the first one, or clears an old one, for when the timer is not started from the last page.
; {#PackId}.exe is Velopack's launcher at the top of the install folder (named after the package).
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#PackId}"; ValueData: """{app}\{#PackId}.exe"" --background"; Tasks: autostart
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: none; ValueName: "{#PackId}"; Flags: deletevalue; Tasks: not autostart

[Run]
Filename: "{app}\{#PackId}.exe"; Description: "{cm:LaunchProgram,{#AppTitle}}"; Flags: nowait postinstall skipifsilent

[Code]
{ Message boxes are suppressible: a silent install (/VERYSILENT /SUPPRESSMSGBOXES) never shows one, it takes the default. }
const
  UninstallKey = 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{#PackId}';
  WizardKey = 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{#PackId}.Wizard_is1';

var
  { Installing over the timer: what it has already, so the wizard passes on only what the user changes. }
  HadSettings, SeededAutostart, TasksSeeded: Boolean;
  PrevLang: String;

{ The timer's settings file (start with Windows is off unless it says on) and the language it left for the wizard. }
procedure InitializeWizard();
var
  Raw: AnsiString;
  Text: String;
begin
  HadSettings := LoadStringFromFile(ExpandConstant('{userappdata}\{#AppTitle}\settings.json'), Raw);
  SeededAutostart := False;
  if HadSettings then
  begin
    Text := Lowercase(String(Raw));
    StringChangeEx(Text, ' ', '', True);
    SeededAutostart := Pos('"startwithwindows":true', Text) > 0;
  end;
  RegQueryStringValue(HKCU, WizardKey, 'Inno Setup: Language', PrevLang); { none: stays '' }
end;

{ The value of /<Name>= on the command line, without quotes; Found says whether it is there at all. }
function CommandLineValue(Name: String; var Found: Boolean): String;
var
  I: Integer;
  Param: String;
begin
  Result := '';
  Found := False;
  for I := 1 to ParamCount do
  begin
    Param := ParamStr(I);
    if Pos('/' + Uppercase(Name) + '=', Uppercase(Param)) = 1 then
    begin
      Found := True;
      Result := RemoveQuotes(Copy(Param, Length(Name) + 3, MaxInt));
    end;
  end;
end;

{ Over the timer, the options start as it has them: start with Windows from its settings, the desktop shortcut if one is
  there. /TASKS= is the user's whole choice and replaces that; /MERGETASKS= changes only the options it names. }
procedure CurPageChanged(CurPageID: Integer);
var
  Given: Boolean;
  Merge: String;
begin
  if (CurPageID <> wpSelectTasks) or TasksSeeded then
    Exit;
  TasksSeeded := True;
  CommandLineValue('TASKS', Given);
  if not HadSettings or Given then
    Exit;
  if SeededAutostart then
    WizardSelectTasks('autostart')
  else
    WizardSelectTasks('!autostart');
  if FileExists(ExpandConstant('{userdesktop}\{#AppTitle}.lnk')) then
    WizardSelectTasks('desktopicon')
  else
    WizardSelectTasks('!desktopicon');
  Merge := CommandLineValue('MERGETASKS', Given);
  if Given and (Merge <> '') then
    WizardSelectTasks(Merge);
end;

{ The folder the timer is in already (to install over it), or the usual one. }
function DefaultDir(Param: String): String;
begin
  if not RegQueryStringValue(HKCU, UninstallKey, 'InstallLocation', Result) or (Result = '') then
    Result := ExpandConstant('{localappdata}\{#PackId}');
  Result := RemoveBackslashUnlessRoot(Result);
end;

function ChosenDir(): String;
begin
  Result := RemoveBackslashUnlessRoot(WizardDirValue());
end;

{ A Velopack install of the timer, as InstallPaths.RootFor sees one. }
function IsAppInstall(Dir: String): Boolean;
begin
  Result := FileExists(AddBackslash(Dir) + 'Update.exe') and DirExists(AddBackslash(Dir) + 'current');
end;

{ Empty, or only what Velopack puts there (an install that stopped half way: Setup replaces it). }
function HoldsNothingElse(Dir: String): Boolean;
var
  Found: TFindRec;
  Name: String;
begin
  Result := True;
  if FindFirst(AddBackslash(Dir) + '*', Found) then
    try
      repeat
        Name := Lowercase(Found.Name);
        if (Name <> '.') and (Name <> '..') and (Name <> 'update.exe') and (Name <> 'current') and (Name <> 'packages')
            and (Name <> Lowercase('{#PackId}.exe')) and (Name <> '.betaid') and (Name <> 'sq.version') then
        begin
          Result := False;
          Exit;
        end;
      until not FindNext(Found);
    finally
      FindClose(Found);
    end;
end;

function CanWriteTo(Dir: String): Boolean;
var
  Existed: Boolean;
  Probe: String;
begin
  Existed := DirExists(Dir);
  Result := ForceDirectories(Dir);
  if Result then
  begin
    Probe := AddBackslash(Dir) + '.1v1timer-setup-probe';
    Result := SaveStringToFile(Probe, 'ok', False);
    DeleteFile(Probe);
    if not Existed then
      RemoveDir(Dir);
  end;
end;

{ At least MB free on the path's drive (unknown counts as room). }
function HasRoom(Path: String; MB: Integer): Boolean;
var
  Free, Total: Int64;
begin
  Result := not GetSpaceOnDisk64(ExtractFileDrive(Path) + '\', Free, Total) or (Free div (1024 * 1024) >= MB);
end;

{ Room for the timer on the chosen drive and for its Setup in the temporary folder (both on one drive: the sum); when
  not, the drive and the MB it needs. }
function EnoughSpace(Dir: String; var Drive: String; var Need: Integer): Boolean;
var
  Temp: String;
  SameDrive: Boolean;
begin
  Temp := GetTempDir();
  Drive := ExtractFileDrive(Dir);
  SameDrive := CompareText(ExtractFileDrive(Temp), Drive) = 0;
  Need := {#NeedMB};
  if SameDrive then
    Need := Need + {#TempMB};
  Result := HasRoom(Dir, Need);
  if Result and not SameDrive then
  begin
    Drive := ExtractFileDrive(Temp);
    Need := {#TempMB};
    Result := HasRoom(Temp, Need);
  end;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  Dir, Drive: String;
  Need: Integer;
begin
  Result := True;
  if CurPageID = wpSelectDir then
  begin
    Dir := ChosenDir();
    { Uninstalling removes the whole folder: never a drive's root, never a folder that holds other things. }
    if (Length(Dir) <= 3) or (DirExists(Dir) and not IsAppInstall(Dir) and not HoldsNothingElse(Dir)) then
    begin
      SuppressibleMsgBox(CustomMessage('FolderNotEmpty'), mbError, MB_OK, IDOK);
      Result := False;
    end
    else if not CanWriteTo(Dir) then
    begin
      SuppressibleMsgBox(CustomMessage('FolderNotWritable'), mbError, MB_OK, IDOK);
      Result := False;
    end
    else if not EnoughSpace(Dir, Drive, Need) then
    begin
      SuppressibleMsgBox(FmtMessage(CustomMessage('NotEnoughSpace'), [Drive, IntToStr(Need)]), mbError, MB_OK, IDOK);
      Result := False;
    end;
  end;
end;

{ A timer started from the chosen folder (another copy elsewhere, a test or a developer's, does not count). }
function AppRunningIn(Dir: String): Boolean;
var
  Locator, Service, Found, Item: Variant;
  I: Integer;
  ExePath: String;
begin
  Result := False;
  if not IsAppInstall(Dir) then
    Exit; { a new or empty folder: nothing can be running from it }
  try
    Locator := CreateOleObject('WbemScripting.SWbemLocator');
    Service := Locator.ConnectServer('.', 'root\CIMV2');
    Found := Service.ExecQuery('SELECT ExecutablePath FROM Win32_Process WHERE Name = ''{#PackId}.exe''');
    for I := 0 to Found.Count - 1 do
    begin
      Item := Found.ItemIndex(I);
      { A process Windows does not show the path of (another user's) has no ExecutablePath. }
      if VarIsNull(Item.ExecutablePath) then
        Continue;
      ExePath := Item.ExecutablePath;
      if Pos(Lowercase(AddBackslash(Dir)), Lowercase(ExePath)) = 1 then
      begin
        Result := True;
        Exit;
      end;
    end;
  except
    { No WMI: go on (the Setup itself still copes with a running copy). }
  end;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  Result := '';
  while AppRunningIn(ChosenDir()) do
    if SuppressibleMsgBox(CustomMessage('AppRunning'), mbError, MB_RETRYCANCEL, IDCANCEL) <> IDRETRY then
    begin
      Result := CustomMessage('AppRunning');
      Exit;
    end;
end;

{ For the timer's next start: a first install's choices, or, over the timer, only the ones the user changed (none: no file). }
procedure WriteChoices();
var
  DataDir, Choices: String;
  AutostartOn: Boolean;
begin
  Choices := '';
  if not HadSettings or ((PrevLang <> '') and (PrevLang <> ActiveLanguage())) then
    Choices := 'language=' + CustomMessage('LangCode') + #13#10;
  AutostartOn := WizardIsTaskSelected('autostart');
  if not HadSettings or (AutostartOn <> SeededAutostart) then
  begin
    Choices := Choices + 'startWithWindows=';
    if AutostartOn then
      Choices := Choices + 'true' + #13#10
    else
      Choices := Choices + 'false' + #13#10;
  end;
  if Choices = '' then
    Exit;
  DataDir := ExpandConstant('{userappdata}\{#AppTitle}');
  ForceDirectories(DataDir);
  SaveStringToFile(AddBackslash(DataDir) + 'install-choices.txt', Choices, False);
end;

{ The setup log: Velopack's Setup writes it there (--log), kept after the wizard ends. }
function SetupLogPath(): String;
begin
  Result := AddBackslash(GetTempDir()) + '{#PackId}-setup.log';
end;

function HasAny(Text: String; Words: array of String): Boolean;
var
  I: Integer;
begin
  Result := False;
  for I := 0 to GetArrayLength(Words) - 1 do
    if Pos(Words[I], Text) > 0 then
    begin
      Result := True;
      Exit;
    end;
end;

{
  What to do about a failed install: a full drive (checked), else from the end of the log (or why Setup did not start):
  a file in use in the folder, a file Windows or an antivirus blocked; else try again.
}
function FailureCause(Started: Boolean; Tail: String): String;
var
  T: String;
begin
  T := Lowercase(Tail);
  if not HasRoom(ChosenDir(), {#NeedMB}) then
    Result := CustomMessage('CauseSpace')
  else if Started and HasAny(T, ['existing application directory', 'os error 32', 'used by another process']) then
    Result := CustomMessage('CauseInUse')
  else if not Started or HasAny(T, ['os error 5)', 'access is denied', 'os error 225', 'virus', 'os error 1260', 'blocked']) then
    Result := CustomMessage('CauseBlocked')
  else
    Result := CustomMessage('CauseOther');
end;

{ The failure in the user's language: what to do, the log's last lines and where it is (the box copies with Ctrl+C). }
procedure ShowFailure(Started: Boolean; Code: Integer; LogFile: String);
var
  Lines: TArrayOfString;
  Tail, Text: String;
  I, First, ErrorCode: Integer;
begin
  Tail := '';
  if LoadStringsFromFile(LogFile, Lines) then
  begin
    First := GetArrayLength(Lines) - 8;
    if First < 0 then
      First := 0;
    for I := First to GetArrayLength(Lines) - 1 do
      Tail := Tail + Copy(Lines[I], 1, 220) + #13#10;
  end;
  Text := FmtMessage(CustomMessage('InstallFailed'), [IntToStr(Code)]) + #13#10#13#10 + FailureCause(Started, Tail);
  if Tail <> '' then
    Text := Text + #13#10#13#10 + CustomMessage('LogTail') + #13#10 + Tail + #13#10
      + FmtMessage(CustomMessage('OpenLog'), [LogFile]);
  Log(Text);
  if Tail = '' then
    SuppressibleMsgBox(Text, mbError, MB_OK, IDOK)
  else if SuppressibleMsgBox(Text, mbError, MB_YESNO, IDNO) = IDYES then
    ShellExec('', LogFile, '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  Code: Integer;
  Started: Boolean;
  LogFile: String;
begin
  if CurStep = ssInstall then
  begin
    WizardForm.StatusLabel.Caption := CustomMessage('InstallingApp');
    ExtractTemporaryFile('{#PackId}-win-Setup.exe');
    LogFile := SetupLogPath();
    DeleteFile(LogFile);
    Started := Exec(ExpandConstant('{tmp}\{#PackId}-win-Setup.exe'),
      '--silent --verbose --log "' + LogFile + '" --installto "' + ChosenDir() + '"', '', SW_HIDE, ewWaitUntilTerminated, Code);
    if not Started or (Code <> 0) then
    begin
      ShowFailure(Started, Code, LogFile);
      Abort;
    end;
    WriteChoices();
  end;
end;
