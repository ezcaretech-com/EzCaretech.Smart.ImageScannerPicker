# Remove old files below publish
Remove-Item -Recurse -Force bin\
Remove-Item -Recurse -Force publish\

# Build
msbuild.exe /property:GenerateFullPaths=true /p:Configuration=Release /t:rebuild /consoleloggerparameters:NoSummary ImageScannerPicker
msbuild.exe /property:GenerateFullPaths=true /p:Configuration=Release /t:rebuild /consoleloggerparameters:NoSummary ImageScannerPicker.DynamicNetTwain
msbuild.exe /property:GenerateFullPaths=true /p:Configuration=Release /t:rebuild /consoleloggerparameters:NoSummary ImageScannerPicker.DynamicWebTwain

msbuild.exe /property:GenerateFullPaths=true /p:Configuration=Release /p:Platform=x86 /t:rebuild /consoleloggerparameters:NoSummary ImageScannerPicker
msbuild.exe /property:GenerateFullPaths=true /p:Configuration=Release /p:Platform=x86 /t:rebuild /consoleloggerparameters:NoSummary ImageScannerPicker.DynamicNetTwain
msbuild.exe /property:GenerateFullPaths=true /p:Configuration=Release /p:Platform=x86 /t:rebuild /consoleloggerparameters:NoSummary ImageScannerPicker.DynamicWebTwain

# Build publish contents for ImageToolsWPF
xcopy /q /y README.md publish\
xcopy /q /y /s CHANGELOG publish\CHANGELOG\

# Build publish contents for AnyCPU
xcopy /q /y /s bin\Release\*.dll publish\AnyCPU\
xcopy /q /y /s bin\x86\Release\*.dll publish\x86\

# Compress published files for deploy
Set-Location publish
Compress-Archive -Path * -DestinationPath ImageScannerPicker.zip
