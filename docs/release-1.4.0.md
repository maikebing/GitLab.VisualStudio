# Release Draft: 1.4.0

## Title

GitLab Extension for Visual Studio 1.4.0 - VS2026 maintained fork

## Summary

This release prepares a maintained fork of the GitLab Extension for Visual Studio with Visual Studio 2026 support, private GitLab sign-in fixes, and in-IDE merge request / issue browsing.

## Highlights

- Supports Visual Studio 2022 and Visual Studio 2026 (`[17.0,19.0)` install target).
- Fixes VS2026 direct VSIX installation by removing the MSI-only install marker.
- Uses the configured GitLab host for sign-in, sign-up, and password-reset links instead of hardcoding `gitlab.com`.
- Supports GitLab 17.x API v4 OAuth and personal access token validation through `/api/v4/user`.
- Removes API v3 automatic probing.
- Adds Team Explorer merge request and issue lists with filters.
- Adds settings for default branch, issue project, and merge request project.
- Fixes the GitLab editor context menu label so it shows the configured default branch.
- Removes a committed VSIX installer log from the repository.

## Recommended Personal Access Token Scopes

- `api`
- `read_api`
- `read_user`
- `read_repository`
- `write_repository`, if users need repository write operations from the extension

## Known Limitations

- Merge request and issue detail pages still open in the browser.
- Merge request creation is not implemented in the GitLab Team Explorer page yet.
- Visual Studio built-in pull request UI integration is not wired to GitLab yet.
- The VSIX is unsigned unless the release explicitly says otherwise.

## Artifact

- `GitLab.VisualStudio.vsix`
- Size: `12205498` bytes
- SHA256: `0EF4E47F27E46645C76E784A263F30DFEA8BFF761446E3EEA2B243A0D31B33B4`

## Smoke Test Checklist

- Install VSIX into Visual Studio 2026.
- Sign in to a private GitLab deployment with a personal access token.
- Open Team Explorer GitLab home.
- Load merge requests from the configured merge request project.
- Load issues from the configured issue project.
- Filter issues and merge requests by author, labels, state, and search text.
- Set `Default branch` to `develop`.
- Verify the editor GitLab context menu displays `develop` and opens the correct URL.
