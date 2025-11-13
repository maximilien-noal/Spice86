# RTC Integration Tests - Build Required

The ASM source files have been created but the .COM binaries need to be compiled.

## To build the tests:

1. Install NASM: https://www.nasm.us/
   - Windows: `choco install nasm`
   - Linux: `sudo apt-get install nasm`
   - macOS: `brew install nasm`

2. Run the build script:
   - Windows: `build.bat`
   - Linux/macOS: `./build.sh`

3. Run the tests:
   ```bash
   dotnet test --filter "FullyQualifiedName~RtcIntegrationTests"
   ```

## Temporary Skip

Until the COM files are built, the RtcIntegrationTests will be skipped.
The test file checks for the existence of the COM files and throws a clear
error message if they're not found.
