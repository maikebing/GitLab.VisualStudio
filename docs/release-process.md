# Release Process

Use this checklist before publishing a VSIX to GitHub Releases, Open VSIX Gallery, or Visual Studio Marketplace.

## Preflight

- Confirm the working tree is clean except for intentional release changes.
- Confirm `README.md`, `CHANGELOG.md`, and `docs/release-1.4.0.md` describe the same version.
- Confirm `src/GitLab.VisualStudio/source.extension.vsixmanifest` has the intended version, publisher, links, and install targets.
- Confirm the root repository does not contain installer logs, local secrets, `.vs` content, or build output.
- Confirm the MIT license and original attribution are still present.

## Build

```powershell
& 'D:\Program Files\VS18\MSBuild\Current\Bin\MSBuild.exe' GitLabVS.sln /restore /p:Configuration=Release /p:VsInstallRoot='D:\Program Files\VS18' /v:minimal
```

## Package

- Release artifact: `build\Release\GitLab.VisualStudio.vsix`
- Compute checksum:

```powershell
Get-FileHash build\Release\GitLab.VisualStudio.vsix -Algorithm SHA256
```

## Smoke Test

- Install the VSIX into Visual Studio 2026.
- Sign in to a private GitLab server with a personal access token.
- Load merge requests in Team Explorer.
- Load issues in Team Explorer.
- Verify filters for author, labels, state, and search.
- Verify issue and merge request project settings.
- Verify default branch setting and right-click menu text.

## Publish Options

- Internal/team use: attach the VSIX and SHA256 checksum to a GitHub Release or internal file share.
- Open community use: publish a GitHub Release and optionally submit the VSIX to Open VSIX Gallery.
- Formal Marketplace listing: publish under a new publisher/listing unless the original publisher transfers ownership or grants access.

