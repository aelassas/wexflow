::@echo off

set version=6.0
set dst=wexflow-%version%-windows-netcore
set dstDir=.\%dst%
set backend=Backend

if exist %dstDir% rmdir /s /q %dstDir%
mkdir %dstDir%
mkdir %dstDir%\Wexflow-netcore\
mkdir %dstDir%\Wexflow-netcore\Database\
mkdir %dstDir%\WexflowTesting\
mkdir %dstDir%\%backend%\
mkdir %dstDir%\%backend%\images\
mkdir %dstDir%\%backend%\css\
mkdir %dstDir%\%backend%\css\images\
mkdir %dstDir%\%backend%\js\
mkdir %dstDir%\Wexflow.Scripts.MongoDB
mkdir %dstDir%\Wexflow.Scripts.MongoDB\Workflows
mkdir %dstDir%\Documentation\

:: WexflowTesting
xcopy ..\samples\WexflowTesting\* %dstDir%\WexflowTesting\ /s /e
xcopy ..\samples\netcore\windows\WexflowTesting\* %dstDir%\WexflowTesting\ /s /e

:: Wexflow-netcore
xcopy ..\samples\netcore\windows\Wexflow\* %dstDir%\Wexflow-netcore\ /s /e
copy ..\src\netcore\Wexflow.Core\GlobalVariables.xml %dstDir%\Wexflow-netcore\
copy ..\src\netcore\Wexflow.Core\Wexflow.xml %dstDir%\Wexflow-netcore\
copy ..\src\netcore\Wexflow.Core\Workflow.xsd %dstDir%\Wexflow-netcore\

:: Wexflow backend
copy "..\src\backend\Wexflow.Backend\index.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\forgot-password.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\dashboard.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\records.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\manager.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\approval.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\designer.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\history.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\users.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\profiles.html" %dstDir%\%backend%\
copy "..\src\backend\Wexflow.Backend\notifications.html" %dstDir%\%backend%\

xcopy "..\src\backend\Wexflow.Backend\images\*" %dstDir%\%backend%\images\ /s /e

xcopy "..\src\backend\Wexflow.Backend\assets\*" %dstDir%\%backend%\assets\ /s /e

xcopy "..\src\backend\Wexflow.Backend\css\images\*" %dstDir%\%backend%\css\images`\ /s /e
copy "..\src\backend\Wexflow.Backend\css\login.min.css" %dstDir%\%backend%\css
copy "..\src\backend\Wexflow.Backend\css\forgot-password.min.css" %dstDir%\%backend%\css
copy "..\src\backend\Wexflow.Backend\css\dashboard.min.css" %dstDir%\%backend%\css
copy "..\src\backend\Wexflow.Backend\css\records.min.css" %dstDir%\%backend%\css
copy "..\src\backend\Wexflow.Backend\css\manager.min.css" %dstDir%\%backend%\css
copy "..\src\backend\Wexflow.Backend\css\approval.min.css" %dstDir%\%backend%\css
copy "..\src\backend\Wexflow.Backend\css\designer.min.css" %dstDir%\%backend%\css
copy "..\src\backend\Wexflow.Backend\css\history.min.css" %dstDir%\%backend%\css
copy "..\src\backend\Wexflow.Backend\css\users.min.css" %dstDir%\%backend%\css
copy "..\src\backend\Wexflow.Backend\css\profiles.min.css" %dstDir%\%backend%\css
copy "..\src\backend\Wexflow.Backend\css\notifications.min.css" %dstDir%\%backend%\css

copy "..\src\backend\Wexflow.Backend\js\settings.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\language.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\login.min.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\forgot-password.min.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\dashboard.min.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\records.min.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\manager.min.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\approval.min.js" %dstDir%\%backend%\js

copy "..\src\backend\Wexflow.Backend\js\ace.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\worker-xml.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\mode-xml.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\worker-json.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\mode-json.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\ext-searchbox.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\ext-prompt.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\ext-keybinding_menu.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\ext-settings_menu.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\theme-*.js" %dstDir%\%backend%\js

copy "..\src\backend\Wexflow.Backend\js\blockly_compressed.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\blocks_compressed.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\en.js" %dstDir%\%backend%\js

copy "..\src\backend\Wexflow.Backend\js\designer.min.js" %dstDir%\%backend%\js

copy "..\src\backend\Wexflow.Backend\js\history.min.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\users.min.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\profiles.min.js" %dstDir%\%backend%\js
copy "..\src\backend\Wexflow.Backend\js\notifications.min.js" %dstDir%\%backend%\js

:: Wexflow server
dotnet publish ..\src\netcore\Wexflow.Server\Wexflow.Server.csproj --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Server
copy netcore\windows\install.bat %dstDir%
copy netcore\windows\run.bat %dstDir%

:: MongoDB script
dotnet publish ..\src\netcore\Wexflow.Scripts.MongoDB\Wexflow.Scripts.MongoDB.csproj --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Scripts.MongoDB
copy netcore\windows\MongoDB\appsettings.json %dstDir%\Wexflow.Scripts.MongoDB
xcopy "..\samples\netcore\windows\Wexflow\Workflows\*" %dstDir%\Wexflow.Scripts.MongoDB\Workflows /s /e
copy netcore\windows\install-MongoDB.bat %dstDir%

:: RavenDB script
dotnet publish ..\src\netcore\Wexflow.Scripts.RavenDB\Wexflow.Scripts.RavenDB.csproj --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Scripts.RavenDB
copy netcore\windows\RavenDB\appsettings.json %dstDir%\Wexflow.Scripts.RavenDB
copy netcore\windows\install-RavenDB.bat %dstDir%

:: PostgreSQL script
dotnet publish ..\src\netcore\Wexflow.Scripts.PostgreSQL\Wexflow.Scripts.PostgreSQL.csproj --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Scripts.PostgreSQL
copy netcore\windows\PostgreSQL\appsettings.json %dstDir%\Wexflow.Scripts.PostgreSQL
copy netcore\windows\install-PostgreSQL.bat %dstDir%

:: SQLServer script
dotnet publish ..\src\netcore\Wexflow.Scripts.SQLServer\Wexflow.Scripts.SQLServer.csproj --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Scripts.SQLServer
copy netcore\windows\SQLServer\appsettings.json %dstDir%\Wexflow.Scripts.SQLServer
copy netcore\windows\install-SQLServer.bat %dstDir%

:: MySQL script
dotnet publish ..\src\netcore\Wexflow.Scripts.MySQL\Wexflow.Scripts.MySQL.csproj --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Scripts.MySQL
copy netcore\windows\MySQL\appsettings.json %dstDir%\Wexflow.Scripts.MySQL
copy netcore\windows\install-MySQL.bat %dstDir%

:: SQLite script
dotnet publish ..\src\netcore\Wexflow.Scripts.SQLite\Wexflow.Scripts.SQLite.csproj --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Scripts.SQLite
copy netcore\windows\SQLite\appsettings.json %dstDir%\Wexflow.Scripts.SQLite
copy netcore\windows\install-SQLite.bat %dstDir%

:: Firebird script
dotnet publish ..\src\netcore\Wexflow.Scripts.Firebird\Wexflow.Scripts.Firebird.csproj --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Scripts.Firebird
copy netcore\windows\Firebird\appsettings.json %dstDir%\Wexflow.Scripts.Firebird
copy netcore\windows\install-Firebird.bat %dstDir%

:: Oracle script
dotnet publish ..\src\netcore\Wexflow.Scripts.Oracle\Wexflow.Scripts.Oracle.csproj --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Scripts.Oracle
copy netcore\windows\Oracle\appsettings.json %dstDir%\Wexflow.Scripts.Oracle
copy netcore\windows\install-Oracle.bat %dstDir%

:: MariaDB script
dotnet publish ..\src\netcore\Wexflow.Scripts.MariaDB\Wexflow.Scripts.MariaDB.csproj --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Scripts.MariaDB
copy netcore\windows\MariaDB\appsettings.json %dstDir%\Wexflow.Scripts.MariaDB
copy netcore\windows\install-MariaDB.bat %dstDir%

:: Wexflow.Clients.CommandLine
dotnet publish ..\src\netcore\Wexflow.Clients.CommandLine\Wexflow.Clients.CommandLine.csproj --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Clients.CommandLine

:: License
copy ..\LICENSE.txt %dstDir%

:: Documentation
copy "netcore\doc\_README.txt" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Core\Workflow.xml" %dstDir%\Documentation\_Workflow.xml
copy "..\src\netcore\Wexflow.Tasks.CsvToXml\CsvToXml.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FileExists\FileExists.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FilesConcat\FilesConcat.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FilesCopier\FilesCopier.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FilesExist\FilesExist.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FilesInfo\FilesInfo.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FilesLoader\FilesLoader.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FilesMover\FilesMover.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FilesRemover\FilesRemover.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FilesRenamer\FilesRenamer.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Ftp\Ftp.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Http\Http.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.ImagesTransformer\ImagesTransformer.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.ListEntities\ListEntities.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.ListFiles\ListFiles.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.MailsReceiver\MailsReceiver.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.MailsSender\MailsSender.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Md5\Md5.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Mkdir\Mkdir.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Movedir\Movedir.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.ProcessLauncher\ProcessLauncher.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Rmdir\Rmdir.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Sha1\Sha1.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Sha256\Sha256.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Sha512\Sha512.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Sql\Sql.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Tar\Tar.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Template\Template.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Tgz\Tgz.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Touch\Touch.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Twitter\Twitter.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Wait\Wait.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.XmlToCsv\XmlToCsv.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Xslt\Xslt.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Zip\Zip.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Now\Now.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FilesSplitter\FilesSplitter.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Unzip\Unzip.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Untar\Untar.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Untgz\Untgz.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.ProcessInfo\ProcessInfo.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.SqlToXml\SqlToXml.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.SqlToCsv\SqlToCsv.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Guid\Guid.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FilesEqual\FilesEqual.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FilesDiff\FilesDiff.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Torrent\Torrent.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.ImagesResizer\ImagesResizer.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.ImagesCropper\ImagesCropper.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.CsvToSql\CsvToSql.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.ImagesConcat\ImagesConcat.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.ImagesOverlay\ImagesOverlay.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FilesEncryptor\FilesEncryptor.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FilesDecryptor\FilesDecryptor.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.TextsEncryptor\TextsEncryptor.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.TextsDecryptor\TextsDecryptor.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FileMatch\FileMatch.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Ping\Ping.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.HttpPost\HttpPost.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.HttpPut\HttpPut.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.HttpPatch\HttpPatch.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.HttpDelete\HttpDelete.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.UglifyJs\UglifyJs.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.UglifyCss\UglifyCss.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.UglifyHtml\UglifyHtml.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.HttpGet\HttpGet.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.ScssToCss\ScssToCss.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.YamlToJson\YamlToJson.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.JsonToYaml\JsonToYaml.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.CsvToJson\CsvToJson.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.CsvToYaml\CsvToYaml.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.EnvironmentVariable\EnvironmentVariable.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.MessageCorrect\MessageCorrect.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.InstagramUploadImage\InstagramUploadImage.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.InstagramUploadVideo\InstagramUploadVideo.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FolderExists\FolderExists.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FileContentMatch\FileContentMatch.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Approval\Approval.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.VimeoListUploads\VimeoListUploads.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Vimeo\Vimeo.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Slack\Slack.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FilesLoaderEx\FilesLoaderEx.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FilesJoiner\FilesJoiner.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Twilio\Twilio.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.SshCmd\SshCmd.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FileSystemWatcher\FileSystemWatcher.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.SubWorkflow\SubWorkflow.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.YouTube\YouTube.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.YouTubeListUploads\YouTubeListUploads.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.YouTubeSearch\YouTubeSearch.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.Reddit\Reddit.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.RedditListComments\RedditListComments.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.RedditListPosts\RedditListPosts.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.ApproveRecord\ApproveRecord.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.ApprovalRecordsCreator\ApprovalRecordsCreator.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.ApprovalWorkflowsCreator\ApprovalWorkflowsCreator.xml" %dstDir%\Documentation\

:: compress
7z.exe a -tzip %dst%.zip %dstDir%

:: Cleanup
rmdir /s /q %dstDir%

pause