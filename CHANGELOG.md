# Road map

- [x] A feature that has been completed
- [ ] A feature that has NOT yet been completed

Features that have a checkmark are complete and available for
download in the
[CI build](http://vsixgallery.com/extension/54803a44-49e0-4935-bba4-7d7d91682273/).

# Change log

These are the changes to each version that has been released
on the official Visual Studio extension gallery.

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
