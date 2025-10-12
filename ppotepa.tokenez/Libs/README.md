# Libs Folder Structure

## Overview
The `Libs` folder contains PowerScript library files that are always copied to the output directory.

## Location
- **Source**: `ppotepa.tokenez/Libs/`
- **Output**: `bin/Debug/net8.0/Libs/` (and `bin/Release/net8.0/Libs/`)

## Configuration
The `.csproj` file is configured to automatically copy all `.ps` files from the Libs folder:

```xml
<None Update="Libs\**\*.ps">
  <CopyToOutputDirectory>Always</CopyToOutputDirectory>
</None>
```

## Current Libraries

### StdLib.ps
The PowerScript Standard Library containing common utility functions:
- **Location**: `Libs/StdLib.ps`
- **Purpose**: Provides output functions and helpers
- **Functions**:
  - `out(INT value)[]` - Output value to console
  - `outln(INT value)[]` - Output value with newline
  - `outNum(INT number)[]` - Output number
  - `outStr(STRING text)[]` - Output string
  - `outMulti(INT a, INT b)[]` - Output multiple values
  - `outThree(INT a, INT b, INT c)[]` - Output three values
  - `newline(INT dummy)[]` - Print newline
  - `space(INT dummy)[]` - Print space

## Usage

### Linking Libraries in Scripts
```powershell
// Link from the Libs folder
LINK "Libs/StdLib.ps"

// Now you can use library functions
FLEX x = 42
PRINT x
```

### Auto-linking in Tests
Test files are configured to automatically link StdLib.ps:

```csharp
var stdLibPath = Path.Combine("..", "..", "..", "..", "ppotepa.tokenez", "Libs", "StdLib.ps");
if (File.Exists(stdLibPath))
{
    _interpreter.LinkLibrary(stdLibPath);
}
```

## Adding New Libraries
1. Create a new `.ps` file in the `Libs` folder
2. The build system will automatically copy it to the output directory
3. Link it in your scripts using: `LINK "Libs/YourLibrary.ps"`

## Notes
- The Libs folder structure is preserved in the output directory
- All `.ps` files in `Libs` and its subdirectories are automatically copied
- Libraries are expanded inline during preprocessing (before tokenization)
- Uses the LINK statement file linking feature
