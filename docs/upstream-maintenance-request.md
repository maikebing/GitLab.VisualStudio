# Upstream Maintenance Request Draft

Hi,

I have been using this extension with newer Visual Studio and private GitLab deployments, and the original extension currently needs several updates for that workflow.

I prepared a maintained fork with the following changes:

- Visual Studio 2026 installation/runtime support.
- VSIX direct-install fix for newer Visual Studio versions.
- Private GitLab sign-in fixes so the configured host is used instead of hardcoded `gitlab.com` URLs.
- GitLab 17.x API v4 OAuth and personal access token validation.
- Removal of API v3 discovery, because modern GitLab no longer supports API v3.
- In-IDE Team Explorer pages for merge request and issue lists, including filters for author, assignee, labels, state, search text, and target branch where applicable.
- Settings for default branch, issue project, and merge request project.
- Default branch context-menu text fix so it displays configured branches such as `develop`.

The fork keeps the MIT license and original attribution. I would like to know whether you would prefer one of these paths:

1. Review and merge the changes into the original repository.
2. Add me as a maintainer so I can help keep the extension current.
3. Transfer maintenance / publishing access if you are no longer maintaining this extension.
4. Keep the original project as-is, in which case I will continue maintaining the fork transparently as a community-maintained version.

Thank you for the original work on this extension.

