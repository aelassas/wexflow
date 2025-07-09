@echo off

set WexflowTesting=C:\WexflowTesting
set Wexflow=C:\Wexflow-netcore

xcopy "WexflowTesting\*" "%WexflowTesting%\" /s /e /y

rem === Backup original database files ===
move "%Wexflow%\Database\Wexflow.sqlite" "%TEMP%\Wexflow.sqlite.bak"
move "%Wexflow%\Database\Wexflow.db" "%TEMP%\Wexflow.db.bak"
move "%Wexflow%\Database\Wexflow-log.db" "%TEMP%\Wexflow-log.db.bak"

rem === Copy everything from source to Wexflow directory ===
xcopy "Wexflow-netcore\*" "%Wexflow%\"/s /e /y

rem === Restore original database files ===
move "%TEMP%\Wexflow.sqlite.bak" "%Wexflow%\Database\Wexflow.sqlite"
move "%TEMP%\Wexflow.db.bak" "%Wexflow%\Database\Wexflow.db"
move "%TEMP%\Wexflow-log.db.bak" "%Wexflow%\Database\Wexflow-log.db"

xcopy "Wexflow-netcore\Wexflow.xml" "%Wexflow%\"/s /e /y
xcopy "Wexflow-netcore\TasksNames.json" "%Wexflow%\"/s /e /y
xcopy "Wexflow-netcore\TasksSettings.json" "%Wexflow%\"/s /e /y

pause
