dotnet build -c Release
# .\src\KeyValueRepo.Benchmarks\bin\Release\net7.0\KeyValueRepo.Benchmarks.exe --exporters json --filter *
dotnet ./src/KeyValueRepo.Benchmarks/bin/Release/net7.0/KeyValueRepo.Benchmarks.dll --exporters json --filter "KeyValueRepo.Benchmarks.InMemoryBenchmarks.*"
dotnet ./src/KeyValueRepo.Benchmarks/bin/Release/net7.0/KeyValueRepo.Benchmarks.dll --exporters json --filter "KeyValueRepo.Benchmarks.SQLiteBenchmarks.*"
pause