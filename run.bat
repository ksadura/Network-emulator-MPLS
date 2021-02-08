@ECHO off
start .\CableCloud\bin\Debug\netcoreapp3.1\CableCloud.exe ".\Resources\Config.xml"
TIMEOUT /T 1
start .\ManagementSystem\bin\Debug\netcoreapp3.1\ManagementSystemGUI.exe ".\Resources\SystemConfig.xml" ".\Resources\iconMS.ico"
TIMEOUT /T 1
start .\Host\bin\Debug\netcoreapp3.1\Host.exe ".\Resources\HostConfigExample.xml"
TIMEOUT /T 2
start .\Host\bin\Debug\netcoreapp3.1\Host.exe ".\Resources\Host2ConfigExample.xml"
TIMEOUT /T 2
start .\Host\bin\Debug\netcoreapp3.1\Host.exe ".\Resources\Host3ConfigExample.xml"
TIMEOUT /T 1
start .\NetworkNode\bin\Debug\netcoreapp3.1\NetworkNode.exe "./Resources/node1_config.xml"
TIMEOUT /T 1
start .\NetworkNode\bin\Debug\netcoreapp3.1\NetworkNode.exe "./Resources/node2_config.xml"
TIMEOUT /T 1
start .\NetworkNode\bin\Debug\netcoreapp3.1\NetworkNode.exe "./Resources/node3_config.xml"
TIMEOUT /T 1
start .\NetworkNode\bin\Debug\netcoreapp3.1\NetworkNode.exe "./Resources/node4_config.xml"
TIMEOUT /T 1
start .\NetworkNode\bin\Debug\netcoreapp3.1\NetworkNode.exe "./Resources/node5_config.xml"

pasue