#!/usr/bin/env bash

set -e

dotnet test NDjango.Admin.sln \
--configuration Release \
--logger trx \
--logger "console;verbosity=normal" \
--settings "./runsettings.xml"
