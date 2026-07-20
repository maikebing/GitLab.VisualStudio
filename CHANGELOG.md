# Road map

- [x] A feature that has been completed
- [ ] A feature that has NOT yet been completed

Features that have a checkmark are complete and available for
download in the
[CI build](http://vsixgallery.com/extension/54803a44-49e0-4935-bba4-7d7d91682273/).

# Change log

These are the changes to each version that has been released
on the official Visual Studio extension gallery.

## V1.4.0

- Prepared the maintained fork release metadata for Visual Studio 2026 distribution.
- Fixed private GitLab sign-in flows so the login dialog uses the configured host instead of opening `gitlab.com`.
- Updated GitLab API usage for GitLab 17.x: API v4 OAuth token flow, personal access token authentication, and current-user validation.
- Removed API v3 probing from automatic discovery because modern GitLab no longer supports API v3.
- Added in-IDE Team Explorer pages for merge request and issue lists instead of only opening the corresponding web pages.
- Added merge request and issue filters for project, state, scope, author, assignee, labels, search text, and target branch where applicable.
- Added GitLab settings for default branch, issue project, and merge request project.
- Hid the graph navigation item because it did not have a working in-IDE implementation.
- Fixed the GitLab editor context menu default-branch label so it displays the configured branch such as `develop` instead of the static `master` text.
- Removed an accidentally committed VSIX installer log from the repository.

## V1.3.1

- Fixed VSIX install failure on Visual Studio 2026 (`InstallByMsiException`): the manifest no longer sets `InstalledByMsi="true"`, so the extension can be installed directly through the Extensions installer / Manage Extensions instead of requiring an MSI.

## V1.3.0

- Visual Studio 2026 (18.x) support; installs on Visual Studio 2022 and 2026.
- VSIX install targets updated to `[17.0,19.0)`; architectures switched to amd64 + arm64 (x86 removed).
- Visual Studio SDK / VSSDK Build Tools upgraded to 17.14; Newtonsoft.Json 13.0.3, Microsoft.Xaml.Behaviors.Wpf 1.1.142, EmbedIO 3.5.2.
- Team Explorer assemblies resolved from the building VS instance (`$(VsInstallRoot)`) instead of a hard-coded VS2022 path.
- Fixed Newtonsoft.Json binding redirect (was pointing to 12.0.0.0).

## V1.0.183

 - Fix for " cannot connect to custom gitlab server with different port #50"


## V1.0.182

- Fix Login issue within new visual studio project #43
- Fixed some bugs

## V1.0.168
- Fix URL generation for master branch -- by FurkanKambay
 - Fix Translation was wrong - by chrgraefe

## V1.0.167
-Fix #40 Publish option available while tracking remote repository.

## V1.0.165

Automatically detects the API version of Gitlab

## V1.0.156
 Visual Studio 2019 support

## V1.0.150
-  [x] AddOpen URL from clipboard
-  [ ] Fix load error

## V1.0.0.12 

-  [x]Fix HttpUtility.UrlEncode processing username or email causing problems that cannot be logged in

## V1.0.0.119 

-  [x]Now update login mode is OAuth2, which can't be logon before because the new version of GitLab's API session has been discarded.

-  [x]The two API login methods are supported in the login interface, and the old version of GitLab needs to be selected manually. The default is that the login mode is OAuth2 and V4 !


## V1.0.0.115 

-  [x]You can select GitLab Api version .

## V1.0.0.112 

-  [x].modify "Open On GitLab" to "GitLab"

## V1.0.0.95 

-  [x] French, Japanese, German and other languages have been added, but these are Google's translations, so we need human translation!
-  [x] Open on GitLab move to  submenu!
-  [x] Fixed issue #3,Thanks luky92!
-  [x] The selected code can create code snippets directly
-  [x] When you create a project, you can select namespases.
-  [x] GitLab's Api is updated from V3 to V4.


## V1.0.0.70 

-  [x]GitLab login information associated with the solution, easy to switch GitLab server.
-  [x]Enter the password and press enter to login GitLab server.
-  [x] Now, We can login   with two  factor authentication.just enter the personal access token into the password field.

## V1.0.0.58

-  [x] Support for Visual Studio 2017 
2-  [x] Fix bus.


**  V1.0.0.40
 - [x]  Right click on editor, if repository is hosted on GitLab Server , you can jump to master/current branch/current revision's blob page and blame/commits page. If selecting line(single, range) in editor, jump with line number fragment.
-  [x]   Fix [#4](https://www.gitlab.com/maikebing/GitLab.VisualStudio/issues/4) [#5](https://www.gitlab.com/maikebing/GitLab.VisualStudio/issues/5) [#6](https://www.gitlab.com/maikebing/GitLab.VisualStudio/issues/6)
Official builds of this extension are available at [the official website](http://visualstudio.gitclub.cn).
