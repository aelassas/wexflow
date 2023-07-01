set WexflowTesting=C:\WexflowTesting
set Wexflow=C:\Wexflow-netcore

::xcopy WexflowTesting\* %WexflowTesting%\ /s /e
xcopy WexflowTesting\* %WexflowTesting%\ /s /e /d /y
::del %Wexflow%\Database\Wexflow.db
::del %Wexflow%\Database\Wexflow-log.db
::xcopy Wexflow-netcore\* %Wexflow%\ /s /e
xcopy Wexflow-netcore\* %Wexflow%\ /s /e /d /y
xcopy Wexflow-netcore\Wexflow.xml %Wexflow%\ /s /e /y
xcopy Wexflow-netcore\TasksNames.json %Wexflow%\ /s /e /y
xcopy Wexflow-netcore\TasksSettings.json %Wexflow%\ /s /e /y

pause