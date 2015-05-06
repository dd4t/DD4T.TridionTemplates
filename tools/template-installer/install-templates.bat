@echo off

cls
set DD4T_REMOVE_CONFIG=Y
title DD4T Template Installer
if not exist "%~dp0\dd4t-upload-config.xml" ( 
	TcmUploadAssembly.exe dd4t-upload-config.xml
	GoTo AskKeepConfig
)

GoTo AfterAskKeepConfig
:AskKeepConfig
echo Would you like to keep the configuration file for future uploads? [Y/N] 
set /p "DD4T_KEEP_CONFIG= "
if %DD4T_KEEP_CONFIG%==N (
	GoTo RemoveConfig
)
if %DD4T_KEEP_CONFIG%==n (
	GoTo RemoveConfig
)
:AfterAskKeepConfig


GoTo AfterRemoveConfig
:RemoveConfig
set DD4T_REMOVE_CONFIG=y
:AfterRemoveConfig

if not exist "%~dp0\dd4t-upload-config.xml" ( 
	echo Something must have gone wrong, there is no dd4t-upload-config.xml
	GoTo Exit
)

If "%1"=="" (
	GoTo AskFolder
)
set DD4T_FOLDER=%1%
GoTo AfterAskFolder
:AskFolder
echo Enter the URI of the folder where you want to store the DD4T template building blocks:
set /p "DD4T_FOLDER= "
:AfterAskFolder

TcmUploadAssembly.exe dd4t-upload-config.xml "files\DD4T.Templates.merged.dll" /folder:%DD4T_FOLDER% /verbose

if %DD4T_REMOVE_CONFIG%==y (
	echo Removing config file
	del dd4t-upload-config.xml
)
	
:Exit
set DD4T_FOLDER=
set DD4T_REMOVE_CONFIG=
set DD4T_KEEP_CONFIG=
set DD4T_ACCEPT_FOLDER=
