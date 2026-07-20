# Building GitLab Extension for Visual Studio

This is a Visual Studio VSIX extension targeting **.NET Framework 4.7.2**. It can only
be built on **Windows** with Visual Studio and the Visual Studio SDK installed — it
cannot be compiled on Linux/macOS or with the .NET (Core) SDK alone.

## Prerequisites

- Windows 10/11
- **Visual Studio 2022 (17.x)** or **Visual Studio 2026 (18.x)**, any edition
  (Community / Professional / Enterprise)
- Workloads / individual components:
  - **Visual Studio extension development** (VSSDK)
  - **.NET Framework 4.7.2 targeting pack**
  - **Team Explorer** (the extension integrates with the Team Explorer panes)

## Supported Visual Studio versions

The VSIX manifest install target is `[17.0,19.0)`, so the produced package installs on
both Visual Studio 2022 and Visual Studio 2026. The extension is built as **Any CPU**
and ships **amd64** and **arm64** install targets (the legacy 32-bit `x86` target was
removed because Visual Studio 2022+ is a 64-bit process).

## Build from the IDE

1. Open `GitLabVS.sln`.
2. Select the `Release` (or `Debug`) configuration, `Any CPU` platform.
3. Restore NuGet packages (automatic on first build).
4. Build the solution. The VSIX is produced at
   `build\<Configuration>\GitLab.VisualStudio.vsix`.

To debug, set `GitLab.VisualStudio` as the startup project and press F5; this launches
the Visual Studio Experimental Instance (`/rootsuffix Exp`) with the extension loaded.

## Build from the command line

Run from a **Developer Command Prompt / Developer PowerShell** for your VS version so
that `MSBuild`, `$(VsInstallRoot)` and `$(DevEnvDir)` are available (these are used to
locate the Team Explorer assemblies):

```cmd
nuget restore GitLabVS.sln
msbuild GitLabVS.sln /p:Configuration=Release /p:DeployExtension=false /v:m
```

## Required local file: `GitApp.cs` (OAuth credentials)

`src/GitLab.VisualStudio.UI/Properties/GitApp.cs` holds the GitLab OAuth application
credentials and is intentionally **git-ignored** (see `.gitignore`). A clean checkout
therefore does not contain it, and the build fails with
`CS2001: Source file '...\Properties\GitApp.cs' could not be found` until you create it.

Create the file locally with this shape:

```csharp
namespace GitLab.VisualStudio.UI.Properties
{
    internal static class GitApp
    {
        public const string client_id = "<your GitLab OAuth application id>";
        public const string client_secret = "<your GitLab OAuth application secret>";
    }
}
```

The CI workflow (`.github/workflows/build.yml`) generates a placeholder version of this
file automatically so it can validate compilation without access to real credentials.

## Notes for the VS2026 upgrade

- `Microsoft.VisualStudio.SDK` / `Microsoft.VSSDK.BuildTools` are on the **17.14** line.
  Per Microsoft's extension compatibility model, an extension built against the 17.x SDK
  runs unchanged on Visual Studio 2026, so a separate 18.x build is not required.
- Team Explorer references in `GitLab.TeamFoundation.17.csproj` resolve via
  `$(VsInstallRoot)` / `$(DevEnvDir)` instead of a hard-coded
  `...\2022\Professional\...` path, so the project builds against whichever VS edition
  and version is performing the build.
