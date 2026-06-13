# Build and Run Scripts

## Quick Build (Development)

```powershell
# From the MacFontRenderer directory
dotnet build -c Debug

# Run
dotnet run --project MacFontRenderer.csproj
```

## Release Build

```powershell
# Build release version
dotnet build -c Release

# Self-contained publish (no .NET SDK required on target machine)
dotnet publish -c Release -r win-x64 --self-contained --no-restore

# Output: bin\Release\net8.0-windows\publish\MacFontRenderer.exe
```

## Create MSI Installer (Optional)

To create an installer, you'll need WiX Toolset:

```powershell
# Install WiX (requires Visual Studio or standalone)
# Then use: candle.exe, light.exe (in MacFontRenderer.wxs file)
```

## Testing in VM

1. Create VM snapshot before testing
2. Run executable with admin privileges
3. Test Apply and Restore buttons
4. Verify registry changes: `regedit` → Navigate to keys above
5. Restart VM
6. Restore from snapshot if needed

## Command-Line Options (Future)

```powershell
# Apply settings without UI
MacFontRenderer.exe --apply --font "SF Pro Text" --gamma 1000

# Restore defaults
MacFontRenderer.exe --restore

# Check status
MacFontRenderer.exe --status
```
