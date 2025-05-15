::@echo off

set version=8.9
set dst=wexflow
set zip=wexflow-%version%-macos-netcore.zip
set dstDir=.\%dst%
set backend=Backend

if exist %zip% del %zip%
if exist %dstDir% rmdir /s /q %dstDir%
mkdir %dstDir%
mkdir %dstDir%\Wexflow\
mkdir %dstDir%\Wexflow\Database\
mkdir %dstDir%\WexflowTesting\
mkdir %dstDir%\%backend%\
mkdir %dstDir%\%backend%\images\
mkdir %dstDir%\%backend%\css\
mkdir %dstDir%\%backend%\css\images\
mkdir %dstDir%\%backend%\js\
::mkdir %dstDir%\Wexflow.Scripts.MongoDB
::mkdir %dstDir%\Wexflow.Scripts.MongoDB\Workflows
mkdir %dstDir%\Documentation\

:: WexflowTesting
xcopy ..\samples\WexflowTesting\* %dstDir%\WexflowTesting\ /s /e
xcopy ..\samples\netcore\macos\WexflowTesting\* %dstDir%\WexflowTesting\ /s /e

:: Wexflow
xcopy ..\samples\netcore\macos\Wexflow\* %dstDir%\Wexflow\ /s /e
copy ..\src\netcore\Wexflow.Core\Workflow.xsd %dstDir%\Wexflow\

:: Wexflow backend
copy "..\src\backend\Wexflow.Backend\*.html" %dstDir%\%backend%\

xcopy "..\src\backend\Wexflow.Backend\images\*" %dstDir%\%backend%\images\ /s /e

xcopy "..\src\backend\Wexflow.Backend\assets\*" %dstDir%\%backend%\assets\ /s /e

copy "..\src\backend\Wexflow.Backend\css\*.css" %dstDir%\%backend%\css
xcopy "..\src\backend\Wexflow.Backend\css\images\*" %dstDir%\%backend%\css\images`\ /s /e

copy "..\src\backend\Wexflow.Backend\js\*.js" %dstDir%\%backend%\js

:: Wexflow server
dotnet publish ..\src\netcore\Wexflow.Server\Wexflow.Server.csproj --framework net9.0 --runtime osx-x64 --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Server
copy netcore\macos\appsettings.json %dstDir%\Wexflow.Server

:: MongoDB script
dotnet publish ..\src\netcore\Wexflow.Scripts.MongoDB\Wexflow.Scripts.MongoDB.csproj --framework net9.0 --runtime osx-x64 --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Scripts.MongoDB
copy netcore\macos\MongoDB\appsettings.json %dstDir%\Wexflow.Scripts.MongoDB
xcopy "..\samples\netcore\macos\Wexflow\Workflows\*" %dstDir%\Wexflow.Scripts.MongoDB\Workflows\ /s /e

:: RavenDB script
::dotnet publish ..\src\netcore\Wexflow.Scripts.RavenDB\Wexflow.Scripts.RavenDB.csproj --framework net9.0 --runtime osx-x64 --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Scripts.RavenDB
::copy netcore\macos\RavenDB\appsettings.json %dstDir%\Wexflow.Scripts.RavenDB

:: PostgreSQL script
dotnet publish ..\src\netcore\Wexflow.Scripts.PostgreSQL\Wexflow.Scripts.PostgreSQL.csproj --framework net9.0 --runtime osx-x64 --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Scripts.PostgreSQL
copy netcore\macos\PostgreSQL\appsettings.json %dstDir%\Wexflow.Scripts.PostgreSQL

:: SQLServer script
dotnet publish ..\src\netcore\Wexflow.Scripts.SQLServer\Wexflow.Scripts.SQLServer.csproj --framework net9.0 --runtime osx-x64 --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Scripts.SQLServer
copy netcore\macos\SQLServer\appsettings.json %dstDir%\Wexflow.Scripts.SQLServer

:: MySQL script
dotnet publish ..\src\netcore\Wexflow.Scripts.MySQL\Wexflow.Scripts.MySQL.csproj --framework net9.0 --runtime osx-x64 --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Scripts.MySQL
copy netcore\macos\MySQL\appsettings.json %dstDir%\Wexflow.Scripts.MySQL

:: SQLite script
dotnet publish ..\src\netcore\Wexflow.Scripts.SQLite\Wexflow.Scripts.SQLite.csproj --framework net9.0 --runtime osx-x64 --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Scripts.SQLite
copy netcore\macos\SQLite\appsettings.json %dstDir%\Wexflow.Scripts.SQLite

:: LiteDB script
dotnet publish ..\src\netcore\Wexflow.Scripts.LiteDB\Wexflow.Scripts.LiteDB.csproj --framework net9.0 --runtime osx-x64 --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Scripts.LiteDB
copy netcore\macos\LiteDB\appsettings.json %dstDir%\Wexflow.Scripts.LiteDB

:: Firebird script
::dotnet publish ..\src\netcore\Wexflow.Scripts.Firebird\Wexflow.Scripts.Firebird.csproj --framework net9.0 --runtime osx-x64 --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Scripts.Firebird
::copy netcore\macos\Firebird\appsettings.json %dstDir%\Wexflow.Scripts.Firebird

:: Oracle script
dotnet publish ..\src\netcore\Wexflow.Scripts.Oracle\Wexflow.Scripts.Oracle.csproj --framework net9.0 --runtime osx-x64 --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Scripts.Oracle
copy netcore\macos\Oracle\appsettings.json %dstDir%\Wexflow.Scripts.Oracle

:: MariaDB script
::dotnet publish ..\src\netcore\Wexflow.Scripts.MariaDB\Wexflow.Scripts.MariaDB.csproj --framework net9.0 --runtime osx-x64 --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Scripts.MariaDB
::copy netcore\macos\MariaDB\appsettings.json %dstDir%\Wexflow.Scripts.MariaDB

:: Wexflow.Clients.CommandLine
dotnet publish ..\src\netcore\Wexflow.Clients.CommandLine\Wexflow.Clients.CommandLine.csproj --framework net9.0 --runtime osx-x64 --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Clients.CommandLine

:: Wexflow.Scripts.RunAllWorkflows
dotnet publish ..\src\netcore\Wexflow.Scripts.RunAllWorkflows\Wexflow.Scripts.RunAllWorkflows.csproj --framework net9.0 --runtime osx-x64 --configuration Release --force --output %~dp0\%dstDir%\Wexflow.Scripts.RunAllWorkflows

:: License
copy ..\LICENSE.txt %dstDir%

:: Documentation
copy "netcore\doc\_README.txt" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Core\Workflow.xml" %dstDir%\Documentation\_Workflow.xml
copy "..\src\netcore\Wexflow.Tasks.CsvToXml\CsvToXml.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FileExists\FileExists.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FileMatch\FileMatch.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FileNotExist\FileNotExist.xml" %dstDir%\Documentation\
copy "..\src\netcore\Wexflow.Tasks.FileNotMatch\FileNotMatch.xml" %dstDir%\Documentation\
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
7z.exe a -tzip %zip% %dstDir%

:: Cleanup
rmdir /s /q %dstDir%

pause
