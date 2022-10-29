set WexflowTesting=C:\WexflowTesting
set Wexflow=C:\Wexflow-netcore

xcopy WexflowTesting\* %WexflowTesting%\ /s /e
del %Wexflow%\Database\Wexflow.db
xcopy Wexflow-netcore\* %Wexflow%\ /s /e

pause