# About

Proto.Actor & ASP.Net & SignalR(WebSocket streaming) sample

Sample chat app that aggregates all user's messages.

# Prerequisites

## install .Net SDK

Please see: https://docs.microsoft.com/ja-jp/dotnet/core/install/

For Amazon Linux:

```shell
sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
sudo yum install -y dotnet-sdk-6.0
```

For WSL2 (Ubuntu 20.04):

```shell
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O
packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y apt-transport-https
sudo apt-get update
sudo apt-get install -y dotnet-sdk-6.0
```



# How to run the server

## for development

```shell
dotnet watch
```

## for benchmark

```shell
dotnet run --configuration Release
```

# How to run the client

open URL with web browser:

`https://(Server IP):7030/Chat`
