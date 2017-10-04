rd /S /Q %~dp0build

rd /S /Q %LocalAppData%\Microsoft\VisualStudio\14.0Exp
rd /S /Q %LocalAppData%\Microsoft\VisualStudio\Exp
reg delete HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\14.0Exp /f
reg delete HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\14.0Exp_Config /f
reg delete HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\14.0Exp_Remote /f
"C:\Program Files (x86)\Microsoft Visual Studio 14.0\VSSDK\VisualStudioIntegration\Tools\Bin\CreateExpInstance.exe" /Reset /VSInstance=14.0 /RootSuffix=Exp



rd /S /Q %LocalAppData%\Microsoft\VisualStudio\15.0Exp
rd /S /Q %LocalAppData%\Microsoft\VisualStudio\Exp
reg delete HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\15.0Exp /f
reg delete HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\15.0Exp_Config /f
reg delete HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\15.0Exp_Remote /f
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\VSSDK\VisualStudioIntegration\Tools\Bin\CreateExpInstance.exe" /Reset /VSInstance=15.0 /RootSuffix=Exp


rd /S /Q %LocalAppData%\Microsoft\VisualStudio\15.0Exp1
rd /S /Q %LocalAppData%\Microsoft\VisualStudio\Exp1
reg delete HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\15.0Exp1 /f
reg delete HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\15.0Exp1_Config /f
reg delete HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\15.0Exp1_Remote /f
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\VSSDK\VisualStudioIntegration\Tools\Bin\CreateExpInstance.exe" /Reset /VSInstance=15.0 /RootSuffix=Exp1



REM wait for 1 second, so you can see what's happing
timeout 2 > NUL