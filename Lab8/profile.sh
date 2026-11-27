set -eu
cd -- "$(dirname -- "$0")"
dotnet build Lab8.csproj -c Release
dotnet tool restore
mkdir -p artifacts
dotnet tool run dotnet-trace -- collect --format Speedscope --output artifacts/before.nettrace --show-child-io -- dotnet bin/Release/net10.0/Lab8.dll --profile baseline --count 30000 --iterations 1
dotnet tool run dotnet-trace -- collect --format Speedscope --output artifacts/after.nettrace --show-child-io -- dotnet bin/Release/net10.0/Lab8.dll --profile optimized --count 30000 --iterations 200
dotnet tool run dotnet-trace -- report artifacts/before.nettrace topN -n 12 > artifacts/before-top.txt
dotnet tool run dotnet-trace -- report artifacts/after.nettrace topN -n 12 > artifacts/after-top.txt
