# GitLab Extension for Visual Studio

Maintained fork of `maikebing/GitLab.VisualStudio` with Visual Studio 2026 support and refreshed GitLab API flows.

This fork is intended for teams that still want the classic Team Explorer GitLab workflow inside Visual Studio, especially with private GitLab deployments. It is not the original Visual Studio Marketplace listing unless it is explicitly published from this repository.

## Supported Visual Studio Versions

- Visual Studio 2022, 17.x
- Visual Studio 2026, 18.x

The current release package is smoke-tested on Visual Studio 2026. The VSIX install target is `[17.0,19.0)`.

## Features

- Connect to GitLab.com or a private GitLab server.
- Sign in with GitLab OAuth password grant, or with a GitLab personal access token.
- Browse merge requests inside Team Explorer.
- Browse issues inside Team Explorer.
- Filter merge requests and issues by state, scope, author, assignee, labels, search text, and target branch where applicable.
- Configure separate projects for issues and merge requests. This supports setups where product issues live in a dedicated project such as `group/product-bugs`.
- Configure the default branch used by the editor context menu, for example `develop` instead of `master`.
- Open GitLab URLs for the selected file, current branch, configured default branch, revision, blame, commits, and selected line ranges.
- Clone, publish, create projects, and create snippets from the existing extension workflow.

## Install

1. Download the `.vsix` package from the repository release.
2. Close Visual Studio.
3. Run the `.vsix` installer and select the target Visual Studio instance.
4. Restart Visual Studio.
5. Open Team Explorer and connect GitLab.

For internal distribution, share the VSIX together with its SHA256 checksum so users can verify the package they received.

## Sign In

Use the GitLab panel in Team Explorer.

Recommended token flow:

1. In GitLab, create a personal access token.
2. Use the private GitLab host URL, for example `https://gitlab.example.com`.
3. Enter your GitLab username.
4. Paste the personal access token into the password field.
5. Check `Personal Access Token`.
6. Sign in.

Recommended token scopes:

- `api`
- `read_api`
- `read_user`
- `read_repository`
- `write_repository`, if you use repository write operations from the extension

Some GitLab versions treat `api` as the broad API permission, but the explicit read scopes make the intended usage clearer for managed deployments.

## Settings

Open Team Explorer, select the GitLab home page, then open `Settings`.

Available settings:

- `Default branch`: branch used by the GitLab context menu item for the default branch URL. Example: `develop`.
- `Issues project`: project path or ID used when loading issues. Example: `prod_dev/etstudio-bugs`.
- `Merge requests project`: project path or ID used when loading merge requests. Leave empty to use the current repository project.

## Known Limitations

- Merge request and issue detail pages still open in the browser.
- Merge request creation is not implemented in the GitLab Team Explorer page yet.
- Visual Studio built-in pull request UI integration is not wired to GitLab yet. GitLab merge requests are the equivalent workflow concept, but Visual Studio's built-in provider integration requires a deeper Git hosting provider implementation.
- The VSIX is not signed unless a release explicitly says otherwise.

## Build

Build from a Developer PowerShell or normal PowerShell with Visual Studio 2026 installed:

```powershell
& 'D:\Program Files\VS18\MSBuild\Current\Bin\MSBuild.exe' GitLabVS.sln /restore /p:Configuration=Release /p:VsInstallRoot='D:\Program Files\VS18' /v:minimal
```

The release VSIX is produced at:

```text
build\Release\GitLab.VisualStudio.vsix
```

If the build fails with Team Explorer or Shell.Framework assembly version conflicts, rebuild with `/restore` and set `VsInstallRoot` to the Visual Studio instance you are targeting.

## Release Notes

See [CHANGELOG.md](CHANGELOG.md).

## License and Attribution

This project remains under the MIT License.

Original project: [maikebing/GitLab.VisualStudio](https://github.com/maikebing/GitLab.VisualStudio)

This fork preserves the original license and attribution while adding maintenance updates for modern Visual Studio and GitLab deployments.
