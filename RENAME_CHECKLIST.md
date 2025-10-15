# Tokenez → PowerScript Rename Checklist

## Summary
This document lists all places where "Tokenez" or "tokenez" needs to be renamed to "PowerScript" or "powerscript".

## 1. Directory/Folder Names to Rename

### Source Code Directories
- [ ] `src/Tokenez.Core` → `src/PowerScript.Core`
- [ ] `src/Tokenez.Common` → `src/PowerScript.Common`
- [ ] `src/Tokenez.Parser` → `src/PowerScript.Parser`
- [ ] `src/Tokenez.Compiler` → `src/PowerScript.Compiler`
- [ ] `src/Tokenez.Runtime` → `src/PowerScript.Runtime`
- [ ] `src/Tokenez.Interpreter` → `src/PowerScript.Interpreter`
- [ ] `src/Tokenez.CLI` → `src/PowerScript.CLI`

### Test Directories
- [ ] `tests/Tokenez.Integration.Tests` → `tests/PowerScript.Integration.Tests`

### Root Directory
- [ ] `d:\git\ppotepa\tokenez` → `d:\git\ppotepa\powerscript`
- [ ] `d:\git\ppotepa\tokenez\tokenez` → `d:\git\ppotepa\powerscript\powerscript`

## 2. File Names to Rename

### Project Files (.csproj)
- [ ] `src/Tokenez.Core/PowerScript.Core.csproj` - Already renamed, but path needs update
- [ ] `src/Tokenez.Common/PowerScript.Common.csproj` - Already renamed, but path needs update
- [ ] `src/Tokenez.Parser/PowerScript.Parser.csproj` - Already renamed, but path needs update  
- [ ] `src/Tokenez.Compiler/PowerScript.Compiler.csproj` - Already renamed, but path needs update
- [ ] `src/Tokenez.Runtime/Tokenez.Runtime.csproj` → `PowerScript.Runtime.csproj`
- [ ] `src/Tokenez.Interpreter/PowerScript.Interpreter.csproj` - Already renamed, but path needs update
- [ ] `src/Tokenez.CLI/PowerScript.CLI.csproj` - Already renamed, but path needs update
- [ ] `tests/Tokenez.Integration.Tests/Tokenez.Integration.Tests.csproj` → `PowerScript.Integration.Tests.csproj`

### Solution Files
- [ ] `Tokenez.sln` → `PowerScript.sln` (Note: PoweScript.sln exists but has typo)
- [ ] `ppotepa.tokenez.slnx` → `ppotepa.powerscript.slnx`

## 3. Namespace Updates Required

### In ALL .cs files, replace namespaces:
- [ ] `namespace Tokenez.Core` → `namespace PowerScript.Core`
- [ ] `namespace Tokenez.Common` → `namespace PowerScript.Common`
- [ ] `namespace Tokenez.Parser` → `namespace PowerScript.Parser`
- [ ] `namespace Tokenez.Compiler` → `namespace PowerScript.Compiler`
- [ ] `namespace Tokenez.Runtime` → `namespace PowerScript.Runtime`
- [ ] `namespace Tokenez.Interpreter` → `namespace PowerScript.Interpreter`
- [ ] `namespace Tokenez.CLI` → `namespace PowerScript.CLI`
- [ ] `namespace Tokenez.Integration.Tests` → `namespace PowerScript.Integration.Tests`

### In ALL .cs files, replace using statements:
- [ ] `using Tokenez.Core` → `using PowerScript.Core`
- [ ] `using Tokenez.Common` → `using PowerScript.Common`
- [ ] `using Tokenez.Parser` → `using PowerScript.Parser`
- [ ] `using Tokenez.Compiler` → `using PowerScript.Compiler`
- [ ] `using Tokenez.Runtime` → `using PowerScript.Runtime`
- [ ] `using Tokenez.Interpreter` → `using PowerScript.Interpreter`

## 4. Project File Updates

### All .csproj files - Update RootNamespace:
- [ ] `src/Tokenez.Core/PowerScript.Core.csproj`: `<RootNamespace>Tokenez.Core</RootNamespace>` → `<RootNamespace>PowerScript.Core</RootNamespace>`
- [ ] `src/Tokenez.Common/PowerScript.Common.csproj`: `<RootNamespace>Tokenez.Common</RootNamespace>` → `<RootNamespace>PowerScript.Common</RootNamespace>`
- [ ] `src/Tokenez.Parser/PowerScript.Parser.csproj`: `<RootNamespace>Tokenez.Parser</RootNamespace>` → `<RootNamespace>PowerScript.Parser</RootNamespace>`
- [ ] `src/Tokenez.Compiler/PowerScript.Compiler.csproj`: `<RootNamespace>Tokenez.Compiler</RootNamespace>` → `<RootNamespace>PowerScript.Compiler</RootNamespace>`
- [ ] `src/Tokenez.Runtime/Tokenez.Runtime.csproj`: Add `<RootNamespace>PowerScript.Runtime</RootNamespace>`
- [ ] `src/Tokenez.Interpreter/PowerScript.Interpreter.csproj`: `<RootNamespace>Tokenez.Interpreter</RootNamespace>` → `<RootNamespace>PowerScript.Interpreter</RootNamespace>`
- [ ] `src/Tokenez.CLI/PowerScript.CLI.csproj`: `<RootNamespace>Tokenez.CLI</RootNamespace>` → `<RootNamespace>PowerScript.CLI</RootNamespace>`
- [ ] `tests/Tokenez.Integration.Tests/Tokenez.Integration.Tests.csproj`: `<RootNamespace>Tokenez.Integration.Tests</RootNamespace>` → `<RootNamespace>PowerScript.Integration.Tests</RootNamespace>`

### All .csproj files - Update ProjectReference paths:
- [ ] All references to `Tokenez.Core\PowerScript.Core.csproj` paths
- [ ] All references to `Tokenez.Common\PowerScript.Common.csproj` paths
- [ ] All references to `Tokenez.Parser\PowerScript.Parser.csproj` paths
- [ ] All references to `Tokenez.Compiler\PowerScript.Compiler.csproj` paths
- [ ] All references to `Tokenez.Runtime\Tokenez.Runtime.csproj` → `PowerScript.Runtime\PowerScript.Runtime.csproj`
- [ ] All references to `Tokenez.Interpreter\PowerScript.Interpreter.csproj` paths
- [ ] All references to `Tokenez.CLI\PowerScript.CLI.csproj` paths
- [ ] All references to `Tokenez.Integration.Tests\Tokenez.Integration.Tests.csproj` paths

## 5. Solution File Updates

### PoweScript.sln (rename to PowerScript.sln and fix):
- [ ] Rename from `PoweScript.sln` to `PowerScript.sln`
- [ ] Update all project paths from `src\Tokenez.*\` to `src\PowerScript.*\`
- [ ] Update project reference: `"src\Tokenez.Runtime\Tokenez.Runtime.csproj"` → `"src\PowerScript.Runtime\PowerScript.Runtime.csproj"`
- [ ] Update project reference: `"tests\Tokenez.Integration.Tests\Tokenez.Integration.Tests.csproj"` → `"tests\PowerScript.Integration.Tests\PowerScript.Integration.Tests.csproj"`

## 6. Documentation Updates

### Documentation Files:
- [ ] `docs/Manual/README.md`: `ppotepa.tokenez.exe` → `powerscript.exe`
- [ ] `docs/Manual/QUICK_REFERENCE.md`: `ppotepa.tokenez.exe` → `powerscript.exe`
- [ ] `docs/Manual/MANUAL_SUMMARY.md`: `d:\git\ppotepa\tokenez\tokenez\Manual\` → update paths
- [ ] `docs/Manual/MANUAL_SUMMARY.md`: `ppotepa/tokenez` → `ppotepa/powerscript`
- [ ] `docs/Manual/LOCALIZATION_BEST_PRACTICES.md`: `ppotepa.tokenez.Resources` → `PowerScript.Resources`
- [ ] `scripts/stdlib/README.md`: `ppotepa.tokenez/Libs/` → `PowerScript/Libs/`

## 7. Code File Updates

### src/Tokenez.CLI/Program.cs (after renaming to PowerScript.CLI):
- [ ] Line 48: `// 1. Compile from file: tokenez.exe script.ps` → `// 1. Compile from file: powerscript.exe script.ps`
- [ ] Line 60: `"Usage: tokenez.exe <script.ps>"` → `"Usage: powerscript.exe <script.ps>"`

## 8. Git Repository

- [ ] Consider renaming GitHub repository from `tokenez` to `powerscript`
- [ ] Update all documentation references to new repository name
- [ ] Update README.md with new project name

## 9. Binary/Output Names

- [ ] Executable output name: `ppotepa.tokenez.exe` → `powerscript.exe`
- [ ] DLL output names: `Tokenez.*.dll` → `PowerScript.*.dll`

## 10. Generated/Build Files (will auto-update after rebuild)

These will automatically update after renaming and rebuilding:
- [ ] `obj/` directories - delete and rebuild
- [ ] `bin/` directories - delete and rebuild  
- [ ] `.vs/` directory - delete and reopen solution

## Recommended Approach

1. **Backup everything** - Commit current state to git
2. **Close Visual Studio/VS Code**
3. **Rename directories** (source, tests)
4. **Rename files** (.csproj, .sln)
5. **Update file contents** (namespaces, using statements, project references)
6. **Delete obj and bin directories**
7. **Reopen solution and rebuild**
8. **Run tests to verify everything works**
9. **Update git repository name** (if desired)

## PowerShell Script Helper

```powershell
# Navigate to project root
cd "d:\git\ppotepa\tokenez\tokenez"

# Delete all obj and bin directories
Get-ChildItem -Recurse -Directory -Filter "obj" | Remove-Item -Recurse -Force
Get-ChildItem -Recurse -Directory -Filter "bin" | Remove-Item -Recurse -Force

# Find all occurrences (for verification)
Get-ChildItem -Recurse -File -Include *.cs,*.csproj,*.sln,*.md | Select-String -Pattern "Tokenez|tokenez" -CaseSensitive
```
