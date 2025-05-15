#define MyAppName "Wexflow"
#define MyAppVersion "8.9"
#define MyAppPublisher "Akram El Assas"
#define MyAppPublisherURL "https://wexflow.github.io/"
#define MyAppExeName "Wexflow.Clients.Manager.exe"

[Setup]
;SignTool=signtool
PrivilegesRequired=admin
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{36E7859C-FD7F-47E1-91C6-41B5F522E2F7}
SetupMutex=SetupMutex{#SetupSetting("AppId")}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppPublisherURL}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
OutputDir=.
OutputBaseFilename=wexflow-{#MyAppVersion}-windows-x86
SetupIconFile="..\src\net\Wexflow.Clients.Manager\Wexflow.ico"
Compression=lzma
SolidCompression=yes
WizardStyle=modern
LicenseFile="..\LICENSE.txt"

[Messages]
SetupAppRunningError=Setup has detected that %1 is currently running.%n%nPlease close all instances of it now, then click OK to continue, or Cancel to exit.

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
;Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1

[Types]
Name: "full"; Description: "Full installation"
Name: "custom"; Description: "Custom installation"; Flags: iscustom

[Components]
Name: "program"; Description: "Program Files"; Types: full custom; Flags: fixed
Name: "samples"; Description: "Workflow samples"; Types: full

[Files]
; Wexflow server
;Source: "..\src\net\Wexflow.Core.Db.MongoDB\bin\x86\Release\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Core.Db.SQLite\bin\x86\Release\x86\SQLite.Interop.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Server\bin\Release\Wexflow.Server.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Server\bin\Release\Wexflow.Server.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Server\bin\Release\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Server\bin\Release\swagger-ui\*"; DestDir: "{app}\swagger-ui"; Flags: ignoreversion recursesubdirs
;Source: "..\src\net\Wexflow.Server\bin\Release\x86\7z.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.SevenZip\bin\x86\Release\x86\7z.dll"; DestDir: "{app}"; Flags: ignoreversion
;Source: "..\src\net\Wexflow.Server\bin\Release\x86\MediaInfo.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.MediaInfo\bin\x86\Release\x86\MediaInfo.dll"; DestDir: "{app}"; Flags: ignoreversion
;Source: "..\libs\chromedriver.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.WebToScreenshot\bin\x86\Release\selenium-manager\*"; DestDir: "{app}\selenium-manager"; Flags: ignoreversion recursesubdirs

; Wexflow Manager
Source: "..\src\net\Wexflow.Clients.Manager\bin\Release\Wexflow.Clients.Manager.exe"; DestDir: "{app}\Manager"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Clients.Manager\bin\Release\Wexflow.Clients.Manager.exe.config"; DestDir: "{app}\Manager"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Clients.Manager\bin\Release\Wexflow.Core.Service.Client.dll"; DestDir: "{app}\Manager"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Clients.Manager\bin\Release\Newtonsoft.Json.dll"; DestDir: "{app}\Manager"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Clients.Manager\bin\Release\Wexflow.Core.Service.Contracts.dll"; DestDir: "{app}\Manager"; Flags: ignoreversion

; Wexflow.Clients.CommandLine
Source: "..\src\net\Wexflow.Clients.CommandLine\bin\x86\Release\Wexflow.Clients.CommandLine.exe"; DestDir: "{app}\Wexflow.Clients.CommandLine"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Clients.CommandLine\bin\x86\Release\Wexflow.Clients.CommandLine.exe.config"; DestDir: "{app}\Wexflow.Clients.CommandLine"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Clients.CommandLine\bin\x86\Release\*.dll"; DestDir: "{app}\Wexflow.Clients.CommandLine"; Flags: ignoreversion

; Wexflow Backend
Source: "..\src\backend\Wexflow.Backend\*.html"; DestDir: "{app}\Backend"; Flags: ignoreversion

Source: "..\src\backend\Wexflow.Backend\images\*"; DestDir: "{app}\Backend\images"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\assets\*"; DestDir: "{app}\Backend\assets"; Flags: ignoreversion

Source: "..\src\backend\Wexflow.Backend\css\*.css"; DestDir: "{app}\Backend\css"; Flags: ignoreversion
Source: "..\src\backend\Wexflow.Backend\css\images\*"; DestDir: "{app}\Backend\css\images"; Flags: ignoreversion

Source: "..\src\backend\Wexflow.Backend\js\*.js"; DestDir: "{app}\Backend\js"; Flags: ignoreversion

; Wexflow's Documentation
Source: "..\src\net\Wexflow.Core\Workflow.xml"; DestDir: "{app}\Documentation\"; DestName: "_Workflow.xml"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.CsvToXml\CsvToXml.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.FileExists\FileExists.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.FileMatch\FileMatch.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.FileNotExist\FileNotExist.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.FileNotMatch\FileNotMatch.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.FilesConcat\FilesConcat.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.FilesCopier\FilesCopier.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.FilesExist\FilesExist.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.FilesInfo\FilesInfo.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.FilesLoader\FilesLoader.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.FilesMover\FilesMover.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.FilesRemover\FilesRemover.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.FilesRenamer\FilesRenamer.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Ftp\Ftp.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Http\Http.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.ImagesTransformer\ImagesTransformer.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.ListEntities\ListEntities.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.ListFiles\ListFiles.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.MailsReceiver\MailsReceiver.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.MailsSender\MailsSender.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Md5\Md5.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.MediaInfo\MediaInfo.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Mkdir\Mkdir.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Movedir\Movedir.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.ProcessLauncher\ProcessLauncher.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Rmdir\Rmdir.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Sha1\Sha1.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Sha256\Sha256.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Sha512\Sha512.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Sql\Sql.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Tar\Tar.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Template\Template.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Tgz\Tgz.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Touch\Touch.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Twitter\Twitter.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Wait\Wait.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Wmi\Wmi.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.XmlToCsv\XmlToCsv.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Xslt\Xslt.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Zip\Zip.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Now\Now.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Workflow\Workflow.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.FilesSplitter\FilesSplitter.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.ProcessKiller\ProcessKiller.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Unzip\Unzip.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Untar\Untar.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Untgz\Untgz.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.ProcessInfo\ProcessInfo.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.TextToPdf\TextToPdf.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.HtmlToPdf\HtmlToPdf.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.SqlToXml\SqlToXml.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.SqlToCsv\SqlToCsv.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Guid\Guid.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.FilesEqual\FilesEqual.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.FilesDiff\FilesDiff.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Torrent\Torrent.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.ImagesResizer\ImagesResizer.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.ImagesCropper\ImagesCropper.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.CsvToSql\CsvToSql.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.ImagesConcat\ImagesConcat.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.ImagesOverlay\ImagesOverlay.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Unrar\Unrar.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.UnSevenZip\UnSevenZip.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.FilesEncryptor\FilesEncryptor.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.FilesDecryptor\FilesDecryptor.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.TextsEncryptor\TextsEncryptor.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.TextsDecryptor\TextsDecryptor.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.IsoCreator\IsoCreator.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.IsoExtractor\IsoExtractor.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.SevenZip\SevenZip.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.TextToSpeech\TextToSpeech.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.SpeechToText\SpeechToText.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Ping\Ping.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.WebToScreenshot\WebToScreenshot.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.WebToHtml\WebToHtml.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.ExecCs\ExecCs.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.ExecPython\ExecPython.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.ExecVb\ExecVb.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.HttpPost\HttpPost.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.HttpPut\HttpPut.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.HttpPatch\HttpPatch.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.HttpDelete\HttpDelete.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.UglifyJs\UglifyJs.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.UglifyCss\UglifyCss.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.UglifyHtml\UglifyHtml.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.HtmlToText\HtmlToText.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.HttpGet\HttpGet.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
;Source: "..\src\net\Wexflow.Tasks.ScssToCss\ScssToCss.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.YamlToJson\YamlToJson.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.JsonToYaml\JsonToYaml.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.CsvToJson\CsvToJson.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.CsvToYaml\CsvToYaml.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.EnvironmentVariable\EnvironmentVariable.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.MessageCorrect\MessageCorrect.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.InstagramUploadImage\InstagramUploadImage.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.InstagramUploadVideo\InstagramUploadVideo.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.FolderExists\FolderExists.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.FileContentMatch\FileContentMatch.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Approval\Approval.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.VimeoListUploads\VimeoListUploads.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Vimeo\Vimeo.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Slack\Slack.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.FilesLoaderEx\FilesLoaderEx.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.FilesJoiner\FilesJoiner.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.Twilio\Twilio.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.SshCmd\SshCmd.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.FileSystemWatcher\FileSystemWatcher.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.SubWorkflow\SubWorkflow.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.ApproveRecord\ApproveRecord.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.ApprovalRecordsCreator\ApprovalRecordsCreator.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.ApprovalWorkflowsCreator\ApprovalWorkflowsCreator.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion
Source: "..\src\net\Wexflow.Tasks.PdfToText\PdfToText.xml"; DestDir: "{app}\Documentation"; Flags: ignoreversion

Source: "..\LICENSE.txt"; DestDir: "{app}\"; Flags: ignoreversion

; Wexflow's configuration
Source: "..\samples\net\Wexflow\Database\*"; DestDir: "C:\Wexflow\Database"; Components: samples; Flags: ignoreversion recursesubdirs onlyifdoesntexist uninsneveruninstall
Source: "..\samples\net\Wexflow\Workflows\*"; DestDir: "C:\Wexflow\Workflows"; Components: samples; Flags: ignoreversion recursesubdirs uninsneveruninstall
Source: "..\samples\net\Wexflow\Records\*"; DestDir: "C:\Wexflow\Records"; Components: samples; Flags: ignoreversion recursesubdirs uninsneveruninstall
Source: "..\samples\net\Wexflow\Xslt\*"; DestDir: "C:\Wexflow\Xslt"; Components: samples; Flags: ignoreversion recursesubdirs uninsneveruninstall

Source: "..\samples\net\Wexflow\Approval\*"; DestDir: "C:\Wexflow\Approval"; Flags: ignoreversion recursesubdirs uninsneveruninstall
Source: "..\samples\net\Wexflow\TasksNames.json"; DestDir: "C:\Wexflow\"; Flags: ignoreversion recursesubdirs uninsneveruninstall
Source: "..\samples\net\Wexflow\TasksSettings.json"; DestDir: "C:\Wexflow\"; Flags: ignoreversion recursesubdirs uninsneveruninstall
Source: "..\src\net\Wexflow.Core\Wexflow.xml"; DestDir: "C:\Wexflow\"; Flags: ignoreversion recursesubdirs uninsneveruninstall
Source: "..\src\net\Wexflow.Core\Workflow.xsd"; DestDir: "C:\Wexflow\"; Flags: ignoreversion recursesubdirs uninsneveruninstall
Source: "..\src\net\Wexflow.Core\GlobalVariables.xml"; DestDir: "C:\Wexflow\"; Flags: ignoreversion recursesubdirs uninsneveruninstall

Source: "..\samples\WexflowTesting\*"; DestDir: "C:\WexflowTesting\"; Flags: ignoreversion recursesubdirs onlyifdoesntexist uninsneveruninstall

; Wexflow.Scripts.MongoDB
Source: "..\src\net\Wexflow.Scripts.MongoDB\bin\x86\Release\Wexflow.Scripts.MongoDB.exe"; DestDir: "{app}\Wexflow.Scripts.MongoDB"; Flags: ignoreversion recursesubdirs
Source: ".\net\Wexflow.Scripts.MongoDB\Wexflow.Scripts.MongoDB.exe.config"; DestDir: "{app}\Wexflow.Scripts.MongoDB"; Flags: ignoreversion recursesubdirs
Source: "..\src\net\Wexflow.Core.Db.MongoDB\bin\x86\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.MongoDB"; Flags: ignoreversion recursesubdirs
Source: "..\src\net\Wexflow.Scripts.MongoDB\bin\x86\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.MongoDB"; Flags: ignoreversion recursesubdirs
Source: "..\samples\net\Wexflow\Workflows\*.xml"; DestDir: "{app}\Wexflow.Scripts.MongoDB\Workflows"; Flags: ignoreversion recursesubdirs

; Wexflow.Scripts.RavenDB
;Source: "..\src\net\Wexflow.Scripts.RavenDB\bin\x86\Release\Wexflow.Scripts.RavenDB.exe"; DestDir: "{app}\Wexflow.Scripts.RavenDB"; Flags: ignoreversion recursesubdirs
;Source: ".\net\Wexflow.Scripts.RavenDB\Wexflow.Scripts.RavenDB.exe.config"; DestDir: "{app}\Wexflow.Scripts.RavenDB"; Flags: ignoreversion recursesubdirs
;Source: "..\src\net\Wexflow.Core.Db.RavenDB\bin\x86\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.RavenDB"; Flags: ignoreversion recursesubdirs
;Source: "..\src\net\Wexflow.Scripts.RavenDB\bin\x86\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.RavenDB"; Flags: ignoreversion recursesubdirs

; Wexflow.Scripts.PostgreSQL
Source: "..\src\net\Wexflow.Scripts.PostgreSQL\bin\x86\Release\Wexflow.Scripts.PostgreSQL.exe"; DestDir: "{app}\Wexflow.Scripts.PostgreSQL"; Flags: ignoreversion recursesubdirs
Source: ".\net\Wexflow.Scripts.PostgreSQL\Wexflow.Scripts.PostgreSQL.exe.config"; DestDir: "{app}\Wexflow.Scripts.PostgreSQL"; Flags: ignoreversion recursesubdirs
Source: "..\src\net\Wexflow.Core.Db.PostgreSQL\bin\x86\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.PostgreSQL"; Flags: ignoreversion recursesubdirs
Source: "..\src\net\Wexflow.Scripts.PostgreSQL\bin\x86\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.PostgreSQL"; Flags: ignoreversion recursesubdirs

; Wexflow.Scripts.SQLServer
Source: "..\src\net\Wexflow.Scripts.SQLServer\bin\x86\Release\Wexflow.Scripts.SQLServer.exe"; DestDir: "{app}\Wexflow.Scripts.SQLServer"; Flags: ignoreversion recursesubdirs
Source: ".\net\Wexflow.Scripts.SQLServer\Wexflow.Scripts.SQLServer.exe.config"; DestDir: "{app}\Wexflow.Scripts.SQLServer"; Flags: ignoreversion recursesubdirs
Source: "..\src\net\Wexflow.Core.Db.SQLServer\bin\x86\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.SQLServer"; Flags: ignoreversion recursesubdirs
Source: "..\src\net\Wexflow.Scripts.SQLServer\bin\x86\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.SQLServer"; Flags: ignoreversion recursesubdirs

; Wexflow.Scripts.MySQL
Source: "..\src\net\Wexflow.Scripts.MySQL\bin\x86\Release\Wexflow.Scripts.MySQL.exe"; DestDir: "{app}\Wexflow.Scripts.MySQL"; Flags: ignoreversion recursesubdirs
Source: ".\net\Wexflow.Scripts.MySQL\Wexflow.Scripts.MySQL.exe.config"; DestDir: "{app}\Wexflow.Scripts.MySQL"; Flags: ignoreversion recursesubdirs
Source: "..\src\net\Wexflow.Core.Db.MySQL\bin\x86\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.MySQL"; Flags: ignoreversion recursesubdirs
Source: "..\src\net\Wexflow.Scripts.MySQL\bin\x86\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.MySQL"; Flags: ignoreversion recursesubdirs

; Wexflow.Scripts.SQLite
Source: "..\src\net\Wexflow.Scripts.SQLite\bin\x86\Release\Wexflow.Scripts.SQLite.exe"; DestDir: "{app}\Wexflow.Scripts.SQLite"; Flags: ignoreversion recursesubdirs
Source: ".\net\Wexflow.Scripts.SQLite\Wexflow.Scripts.SQLite.exe.config"; DestDir: "{app}\Wexflow.Scripts.SQLite"; Flags: ignoreversion recursesubdirs
Source: "..\src\net\Wexflow.Core.Db.SQLite\bin\x86\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.SQLite"; Flags: ignoreversion recursesubdirs
Source: "..\src\net\Wexflow.Scripts.SQLite\bin\x86\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.SQLite"; Flags: ignoreversion recursesubdirs

; Wexflow.Scripts.LiteDB
Source: "..\src\net\Wexflow.Scripts.LiteDB\bin\x86\Release\Wexflow.Scripts.LiteDB.exe"; DestDir: "{app}\Wexflow.Scripts.LiteDB"; Flags: ignoreversion recursesubdirs
Source: ".\net\Wexflow.Scripts.LiteDB\Wexflow.Scripts.LiteDB.exe.config"; DestDir: "{app}\Wexflow.Scripts.LiteDB"; Flags: ignoreversion recursesubdirs
Source: "..\src\net\Wexflow.Core.Db.LiteDB\bin\x86\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.LiteDB"; Flags: ignoreversion recursesubdirs
Source: "..\src\net\Wexflow.Scripts.LiteDB\bin\x86\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.LiteDB"; Flags: ignoreversion recursesubdirs

; Wexflow.Scripts.Firebird
;Source: "..\src\net\Wexflow.Scripts.Firebird\bin\x86\Release\Wexflow.Scripts.Firebird.exe"; DestDir: "{app}\Wexflow.Scripts.Firebird"; Flags: ignoreversion recursesubdirs
;Source: ".\net\Wexflow.Scripts.Firebird\Wexflow.Scripts.Firebird.exe.config"; DestDir: "{app}\Wexflow.Scripts.Firebird"; Flags: ignoreversion recursesubdirs
;Source: "..\src\net\Wexflow.Core.Db.Firebird\bin\x86\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.Firebird"; Flags: ignoreversion recursesubdirs
;Source: "..\src\net\Wexflow.Scripts.Firebird\bin\x86\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.Firebird"; Flags: ignoreversion recursesubdirs

; Wexflow.Scripts.Oracle
;Source: "..\src\net\Wexflow.Scripts.Oracle\bin\x86\Release\Wexflow.Scripts.Oracle.exe"; DestDir: "{app}\Wexflow.Scripts.Oracle"; Flags: ignoreversion recursesubdirs
;Source: ".\net\Wexflow.Scripts.Oracle\Wexflow.Scripts.Oracle.exe.config"; DestDir: "{app}\Wexflow.Scripts.Oracle"; Flags: ignoreversion recursesubdirs
;Source: "..\src\net\Wexflow.Core.Db.Oracle\bin\x86\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.Oracle"; Flags: ignoreversion recursesubdirs
;Source: "..\src\net\Wexflow.Scripts.Oracle\bin\x86\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.Oracle"; Flags: ignoreversion recursesubdirs

; Wexflow.Scripts.MariaDB
;Source: "..\src\net\Wexflow.Scripts.MariaDB\bin\x86\Release\Wexflow.Scripts.MariaDB.exe"; DestDir: "{app}\Wexflow.Scripts.MariaDB"; Flags: ignoreversion recursesubdirs
;Source: ".\net\Wexflow.Scripts.MariaDB\Wexflow.Scripts.MariaDB.exe.config"; DestDir: "{app}\Wexflow.Scripts.MariaDB"; Flags: ignoreversion recursesubdirs
;Source: "..\src\net\Wexflow.Core.Db.MariaDB\bin\x86\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.MariaDB"; Flags: ignoreversion recursesubdirs
;Source: "..\src\net\Wexflow.Scripts.MariaDB\bin\x86\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.MariaDB"; Flags: ignoreversion recursesubdirs

; Wexflow.Scripts.RunAllWorkflows
Source: "..\src\net\Wexflow.Scripts.RunAllWorkflows\bin\x86\Release\Wexflow.Scripts.RunAllWorkflows.exe"; DestDir: "{app}\Wexflow.Scripts.RunAllWorkflows"; Flags: ignoreversion recursesubdirs
Source: "..\src\net\Wexflow.Scripts.RunAllWorkflows\bin\x86\Release\Wexflow.Scripts.RunAllWorkflows.exe.config"; DestDir: "{app}\Wexflow.Scripts.RunAllWorkflows"; Flags: ignoreversion recursesubdirs
Source: "..\src\net\Wexflow.Scripts.RunAllWorkflows\bin\x86\Release\*.dll"; DestDir: "{app}\Wexflow.Scripts.RunAllWorkflows"; Flags: ignoreversion recursesubdirs

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{commonprograms}\{#MyAppName}\Manager"; Filename: "{app}\Manager\{#MyAppExeName}";
Name: "{commonprograms}\{#MyAppName}\Backend"; Filename: "{app}\Backend\index.html";
Name: "{commonprograms}\{#MyAppName}\Swagger"; Filename: "http://localhost:8000";
;Name: "{commonprograms}\{#MyAppName}\Start Wexflow Windows Service"; Filename: {sys}\sc.exe; Parameters: "start Wexflow" ; IconFilename: "{app}\Wexflow.ico";
;Name: "{commonprograms}\{#MyAppName}\Stop Wexflow Windows Service"; Filename: {sys}\sc.exe; Parameters: "stop Wexflow" ; IconFilename: "{app}\Wexflow.ico";
Name: "{commonprograms}\{#MyAppName}\Configuration"; Filename: "C:\Wexflow\";
Name: "{commonprograms}\{#MyAppName}\Documentation"; Filename: "{app}\Documentation";
Name: "{commonprograms}\{#MyAppName}\Logs"; Filename: "{app}\Wexflow.log";
Name: "{commonprograms}\{#MyAppName}\Install MongoDB samples"; Filename: "{app}\Wexflow.Scripts.MongoDB\Wexflow.Scripts.MongoDB.exe";
;Name: "{commonprograms}\{#MyAppName}\Install RavenDB samples"; Filename: "{app}\Wexflow.Scripts.RavenDB\Wexflow.Scripts.RavenDB.exe";
Name: "{commonprograms}\{#MyAppName}\Install PostgreSQL samples"; Filename: "{app}\Wexflow.Scripts.PostgreSQL\Wexflow.Scripts.PostgreSQL.exe";
Name: "{commonprograms}\{#MyAppName}\Install SQL Server samples"; Filename: "{app}\Wexflow.Scripts.SQLServer\Wexflow.Scripts.SQLServer.exe";
Name: "{commonprograms}\{#MyAppName}\Install MySQL samples"; Filename: "{app}\Wexflow.Scripts.MySQL\Wexflow.Scripts.MySQL.exe";
Name: "{commonprograms}\{#MyAppName}\Install SQLite samples"; Filename: "{app}\Wexflow.Scripts.SQLite\Wexflow.Scripts.SQLite.exe";
Name: "{commonprograms}\{#MyAppName}\Install LiteDB samples"; Filename: "{app}\Wexflow.Scripts.LiteDB\Wexflow.Scripts.LiteDB.exe";
;Name: "{commonprograms}\{#MyAppName}\Install Firebird samples"; Filename: "{app}\Wexflow.Scripts.Firebird\Wexflow.Scripts.Firebird.exe";
;Name: "{commonprograms}\{#MyAppName}\Install Oracle samples"; Filename: "{app}\Wexflow.Scripts.Oracle\Wexflow.Scripts.Oracle.exe";
;Name: "{commonprograms}\{#MyAppName}\Install MariaDB samples"; Filename: "{app}\Wexflow.Scripts.MariaDB\Wexflow.Scripts.MariaDB.exe";
;Name: "{commonprograms}\{#MyAppName}\Run All Workflows"; Filename: "{app}\Wexflow.Scripts.RunAllWorkflows\Wexflow.Scripts.RunAllWorkflows.exe";
Name: "{commonprograms}\{#MyAppName}\Uninstall"; Filename: "{uninstallexe}";

Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\Manager\{#MyAppExeName}"; Tasks: desktopicon
;Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\Manager\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
;Filename: "{app}\Manager\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
Filename: "{sys}\sc.exe"; Parameters: "create Wexflow start= auto binPath= ""{app}\Wexflow.Server.exe""" ; Flags: runhidden  waituntilterminated
Filename: "{sys}\sc.exe"; Parameters: "description Wexflow ""Wexflow workflow engine."""; Flags: runhidden  waituntilterminated
Filename: "{sys}\sc.exe"; Parameters: "start Wexflow"; Flags: runhidden  waituntilterminated

[UninstallRun]
Filename: "taskkill"; Parameters: "/im ""{#MyAppExeName}"" /t /f"; Flags: runhidden waituntilterminated; RunOnceId: StopApp
Filename: "{sys}\sc.exe"; Parameters: "stop Wexflow"; Flags: runhidden waituntilterminated; RunOnceId: StopService 
Filename: "taskkill"; Parameters: "/im ""Wexflow.Server.exe"" /t /f"; Flags: runhidden waituntilterminated; RunOnceId: StopAppWexflowServer
Filename: "taskkill"; Parameters: "/im ""chromedriver.exe"" /t /f"; Flags: runhidden waituntilterminated; RunOnceId: StopChromeDriver
Filename: "{sys}\sc.exe"; Parameters: "delete Wexflow"; Flags: runhidden waituntilterminated; RunOnceId: RemoveService

[UninstallDelete]
Type: files; Name: "{app}\chromedriver.exe"

;[InstallDelete]
;Type: files; Name: "C:\Wexflow\Database\Wexflow.db"
;Type: files; Name: "C:\Wexflow\Database\Wexflow-log.db"

[Code]
function GetUninstallString(): String;
var
  sUnInstPath, sUnInstallString: String;
begin
  sUnInstPath := ExpandConstant('SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');
  sUnInstallString := '';

  if not RegQueryStringValue(HKLM64, sUnInstPath, 'UninstallString', sUnInstallString) then
  begin
    RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString);
  end;

  if sUnInstallString = '' then
  begin
    sUnInstPath := ExpandConstant('SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');

    if not RegQueryStringValue(HKLM32, sUnInstPath, 'UninstallString', sUnInstallString) then
    begin
      RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString);
    end;
  end;

  Result := sUnInstallString;
end;

function IsUpgrade(): Boolean;
begin
  Result := (GetUninstallString() <> '');
end;

function BoolToStr(Value: Boolean): String; 
begin
  if Value then 
  begin
    Result := 'True';
  end 
  else
  begin
    Result := 'False';
  end;
end;

function UnInstallOldVersion(): Integer;
var
  sUnInstallString: String;
  iResultCode: Integer;
begin
  Result := 0;
  sUnInstallString := GetUninstallString();
  if sUnInstallString <> '' then
  begin
    sUnInstallString := RemoveQuotes(sUnInstallString);
    if Exec(sUnInstallString, '/SILENT /NORESTART /SUPPRESSMSGBOXES','', SW_HIDE, ewWaitUntilTerminated, iResultCode) then 
    begin
      Result := 3;
    end 
    else
    begin
      Result := 2;
    end;
  end
  else
  begin
    Result := 1;
  end;
  
  Log('UnInstallOldVersion.Result = ' + IntToStr(Result));
end;

function GetInstalledVersion(): String;
var
  sUnInstPath, sVersionString: String;
begin
  sVersionString := ''
  sUnInstPath := ExpandConstant('SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');
  
  if not RegQueryStringValue(HKLM64, sUnInstPath, 'DisplayVersion', sVersionString) then
  begin
    RegQueryStringValue(HKCU, sUnInstPath, 'DisplayVersion', sVersionString);
  end;

  if sVersionString = '' then
  begin
    sUnInstPath := ExpandConstant('SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');
  
    if not RegQueryStringValue(HKLM32, sUnInstPath, 'DisplayVersion', sVersionString) then
    begin
      RegQueryStringValue(HKCU, sUnInstPath, 'DisplayVersion', sVersionString);
    end;
  end;
  
  Result := sVersionString;
end;

function NumericVersion(sVersion: String): Integer;
var
  s1, s2, i: Integer;
  sv : String;
begin
  s1 := 0;
  for i := 1 to Length(sVersion) do
  begin
    sv := sVersion[i];

    if (sv >= '0') and (sv <= '9') then
      begin
        s2 := StrToInt(sv);

        if i = 1 then
        begin
          s2 := s2  * 10;
        end;
        
        s1 := s1 + s2;
      end;
  end;
  
  Result := s1;
end;

function InitializeSetup(): Boolean;
var
  sInstalledVersion, message: String;
  installedVersion, myAppVersion: Integer;
  v: Integer;
begin
  Result := True;
  sInstalledVersion := GetInstalledVersion();
 
  if IsUpgrade() and (sInstalledVersion <> '') then
  begin
    Log('InitializeSetup.InstalledVersion: ' + sInstalledVersion);
    installedVersion := NumericVersion(sInstalledVersion);
    myAppVersion :=  NumericVersion(ExpandConstant('{#MyAppVersion}'));
    message := '';

    if installedVersion < myAppVersion  then 
    begin 
      message := 'An older version of Wexflow is already installed. Would you like to replace it with this newer version?';
    end 
    else if installedVersion > myAppVersion then
    begin
      message := 'A newer version of Wexflow is already installed. Would you like to replace it with this older version?';
    end
    else if installedVersion = myAppVersion then
    begin
      message := 'The same version of Wexflow is already installed. Would you like to repair it?';
    end;

    v := MsgBox(message, mbInformation, MB_YESNO);
    if v <> IDYES then
    begin
      Result := False;
    end;
  end;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  sNeedsRestart : String;
begin
  sNeedsRestart := BoolToStr(NeedsRestart);
  Log('PrepareToInstall(' + sNeedsRestart + ') called');
  if IsUpgrade() then
  begin
    UnInstallOldVersion();
  end;
  ForceDirectories('C:\Wexflow');
  ForceDirectories('C:\Wexflow\Database');
  ForceDirectories('C:\Wexflow\Workflows');
  ForceDirectories('C:\Wexflow\Records');
  ForceDirectories('C:\Wexflow\Tasks');
  ForceDirectories('C:\Wexflow\Temp');
  ForceDirectories('C:\Wexflow\Approval');
  Log('PrepareToInstall.ForceDirectories done');
  Result := '';  
end;