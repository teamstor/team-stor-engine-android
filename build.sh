#!/bin/bash
# Build for Linux
xbuild /p:Configuration=Release TeamStor.Engine/TeamStor.Engine.csproj
xbuild /p:Configuration=Release TeamStor.Engine.TestGame/TeamStor.Engine.TestGame.csproj
