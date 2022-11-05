set WexflowTesting=C:\WexflowTesting
set Wexflow=C:\Wexflow-netcore

::xcopy WexflowTesting\* %WexflowTesting%\ /s /e
xcopy WexflowTesting\* %WexflowTesting%\ /s /e /d
::del %Wexflow%\Database\Wexflow.db
::del %Wexflow%\Database\Wexflow-log.db
::xcopy Wexflow-netcore\* %Wexflow%\ /s /e
xcopy Wexflow-netcore\* %Wexflow%\ /s /e /d
xcopy Wexflow-netcore\Wexflow.xml %Wexflow%\ /s /e
xcopy Wexflow-netcore\TasksNames.json %Wexflow%\ /s /e
xcopy Wexflow-netcore\TasksSettings.json %Wexflow%\ /s /e

pause