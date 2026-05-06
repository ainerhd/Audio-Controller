#!/bin/bash
set -e

if ! command -v mono >/dev/null 2>&1; then
  echo "Installing Mono..."
  sudo apt-get update
  sudo apt-get install -y mono-complete
fi

NUGET=nuget.exe
if [ ! -f "$NUGET" ]; then
  echo "Downloading NuGet..."
  wget -qO "$NUGET" https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
fi

# Restore packages
mono "$NUGET" restore "Audio Controller.sln"

# Build solution
xbuild "Audio Controller.sln"
