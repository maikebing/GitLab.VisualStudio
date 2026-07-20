# 交接文档：GitLab.VisualStudio 升级至 VS2026

> 本文档用于把项目当前状态、已做改动、待解决问题完整交接给后续开发者 / AI Agent。
> 最后更新对应分支：`upgrade/vs2026-support`，最新提交 `b78a3f3`。

---

## 0. TL;DR（一句话现状）

升级目标是让这个 **Visual Studio 的 GitLab 扩展（VSIX）支持 VS2026 + 最新 GitLab**。
目前：**能编译、能安装到 VS2026**，但**运行时打开"团队资源管理器(Team Explorer)"会抛 MEF 契约不匹配异常**，GitLab 的界面加载不出来。
根因：扩展用的是**旧版 Team Explorer 扩展模型**，而 CI 是在 **VS2022(17.x)** 上编译的，跑到 **VS2026(18.x)** 上 `ITeamExplorerSection` 契约对不上。
**下一步**：在装有 VS2026 的机器上**本地编译**（工程已改为按当前 VS 安装目录解析 Team Explorer 程序集），验证 VS2026 是否仍保留该 API；若已删除则需把 UI 迁出 Team Explorer。

---

## 1. 项目概览

- **是什么**：Visual Studio 扩展（VSIX），把 GitLab 集成进 Visual Studio 的 Team Explorer。
- **技术栈**：C# / **.NET Framework 4.7.2** / WPF / MEF / VS SDK。**仅能在 Windows + Visual Studio + VS SDK 下编译**（Linux/macOS、纯 .NET SDK 都不行）。
- **仓库**：`Starkxim/GitLab.VisualStudio`
- **工作分支**：`upgrade/vs2026-support`（PR #2）
- **原始上游**：`maikebing/GitLab.VisualStudio`
- **GitLab 访问**：通过 NuGet 包 `NGitLab.Plus 2.0.44`，使用 **REST API v4 + OAuth2/PAT**。

### 解决方案结构（`GitLabVS.sln` 中实际参与构建的 4 个工程）

| 工程 | 作用 |
|---|---|
| `src/GitLab.VisualStudio` | **VSIX/Package 主工程**，含 `GitLabPackage.cs`、`source.extension.vsixmanifest`、`WebService.cs`（GitLab API 调用） |
| `src/GitLab.VisualStudio.Shared` | 共享服务、Models、MEF 接口（`IWebService`、`IStorage` 等） |
| `src/GitLab.VisualStudio.UI` | WPF 视图/视图模型（登录框、克隆、新建项目、新建 Snippet）；依赖 `EmbedIO`（OAuth 回调用的本地 http server） |
| `src/GitLab.TeamFoundation.17` | **Team Explorer 集成程序集**。注意：它自己几乎没有源码，而是通过 `<Compile Include="..\GitLab.TeamFoundation.14\...">` **链接引用 `GitLab.TeamFoundation.14` 目录下的 21 个源文件**来编译。 |

### 重要：被链接的源码实际在 `.14` 目录

`GitLab.TeamFoundation.14/15/16` 这几个目录是**历史的按 VS 版本分别编译的工程**（VS2015/2017/2019），**不在解决方案里、不参与构建**。但 `.17` 工程**复用 `.14` 目录里的源文件**。所以：
- **要改 Team Explorer 的逻辑/界面 → 改 `src/GitLab.TeamFoundation.14/` 下的文件**（它们会被 `.17` 编译进去）。
- `.14/.15/.16` 工程分别引用仓库里已提交的 `lib/14.0`、`lib/15.0`、`lib/16.0` 中的 Team Explorer DLL；`.17` 工程则引用**当前 VS 安装目录**里的程序集（见下文）。

### UI 全部基于 Team Explorer（关键认知）

扩展**没有独立窗口**，所有界面都挂在 Team Explorer：
- **连接(Connect)页**：`GitLabConnectSection`（登录入口）
- **主页(Home)页**：`GitLabHomeSection` + 导航项 `MergeRequests/Issues/Wiki/Graphs/Builds/Pipelines/Snippets`
- **发布(Sync)页**：`GitLabPublishSection`（发布新仓库到 GitLab）
- 这些类继承 `TeamExplorerSectionBase` / 实现 `ITeamExplorerSection`、`ITeamExplorerNavigationItem`，通过 `[TeamExplorerSection]` / `[TeamExplorerNavigationItem]` 用 **MEF** 导出。
- **导航项点击行为 = `OpenInBrowser("merge_requests"/"issues"/...)`**，即**用浏览器打开 GitLab 网页**。扩展**不在 VS 内渲染 MR/Issue 列表**，本质是"快捷入口 + 新建项目/克隆/Snippet/发布"工具。

### 登录方式（`LoginViewModel` / `ConnectSectionViewModel`）

- 字段：**Host**（默认 `https://gitlab.com`）、Email/用户名、Password。
- 用 **Personal Access Token** 时：勾选 **2FA**，把 token 填进密码框（勾 2FA 时密码框值被当 token）。
- **API 版本** 默认 `AutoDiscovery`。
- 代码里有 `gitlab.com/oauth/authorize` 的 OAuth 流程，但**回调换 token 部分是半成品（有 TODO 未完成）**，实际请用 Host + PAT/密码登录。

---

## 2. 本次升级已完成的改动（分支 `upgrade/vs2026-support`）

### 2.1 VSIX 清单 `src/GitLab.VisualStudio/source.extension.vsixmanifest`
- `InstallationTarget` 版本范围 `[17.0,18.0)` → **`[17.0,19.0)`**（覆盖 VS2026=18.x）。
- 移除不再支持的 **x86** 目标；保留 **amd64** 并新增 **arm64**。
- `Dependencies`（MPF.17.0、TeamExplorer.Extensions）与 `Prerequisite`（CoreEditor）上限同步抬到 `19.0`。
- 版本号 `1.2.202` → `1.3.0` → **`1.3.1`**。
- **移除 `<Installation InstalledByMsi="true">` 的 `InstalledByMsi` 属性**（这是"安装失败"的根因，见 3.2）。

### 2.2 NuGet 包版本（4 个工程一致）
| 包 | 旧 | 新 | 备注 |
|---|---|---|---|
| Microsoft.VisualStudio.SDK | 17.1.32210.191 | **17.14.40265** | 17.x 向前兼容 VS2026 |
| Microsoft.VSSDK.BuildTools | 17.1.4057 | **17.14.2142** | |
| Microsoft.VisualStudio.Shell.Framework | 17.1.32210.191 | **17.14.40264** | ⚠️ `.40265` 在 NuGet 不存在，最高 `.40264`（曾导致 `NU1102`） |
| Newtonsoft.Json | 13.0.1 | **13.0.3** | |
| Microsoft.Xaml.Behaviors.Wpf | 1.1.39 | **1.1.142** | |
| EmbedIO | 3.4.3 | **3.5.2** | 仅 UI 工程 |
| NGitLab.Plus | 2.0.44 | 2.0.44 | 已是最新，未动 |
| LibGit2Sharp | 0.26.2 | 0.26.2 | 保守不动，避免原生二进制打包问题 |
- 同时移除了主工程里过时的 `Microsoft.VSSDK.BuildTools.15.1.192` props 导入。

### 2.3 修复 Newtonsoft.Json 绑定重定向
`app.config` 里 `newVersion` 原误写为 `12.0.0.0`（包其实是 13.x），已改为 **`13.0.0.0`**（主工程 / UI / TF.17 三处）。

### 2.4 Team Explorer 程序集引用改为版本无关
`src/GitLab.TeamFoundation.17/GitLab.TeamFoundation.17.csproj`：原本硬编码 `C:\Program Files\Microsoft Visual Studio\2022\Professional\...\Team Explorer\`，已改为：
```xml
<TeamExplorerDir Condition="'$(VsInstallRoot)' != ''">$(VsInstallRoot)\Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer\</TeamExplorerDir>
<TeamExplorerDir Condition="'$(TeamExplorerDir)' == '' and '$(DevEnvDir)' != ''">$(DevEnvDir)CommonExtensions\Microsoft\TeamFoundation\Team Explorer\</TeamExplorerDir>
```
**含义**：在哪个版本的 VS 里编译，就绑定哪个版本的 Team Explorer 程序集。**这正是本地用 VS2026 编译可能修好运行时报错的关键**（见 3.3）。

### 2.5 其他
- `src/common/SolutionInfo.cs`：版本 `1.3.0.0`，修正版权字符。
- `.sln` 头 `Visual Studio Version 18`；`appveyor.yml` 镜像改 `Visual Studio 2022`；`GitVersion.yml` → `1.3.0`。
- `README.md` / `CHANGELOG.md` 更新；`docs/build.md` 写入完整构建说明。

### 2.6 CI 工作流 `.github/workflows/build.yml`
- `runs-on: windows-2022`（**GitHub 暂无 VS2026 runner 镜像，所以 CI 实际用 VS2022(17.14) 编译**——这点很重要，是运行时报错的根源之一）。
- 步骤：`setup-msbuild` → 用 `vswhere` 定位 VS 并探测 Team Explorer 目录 → **生成占位 `GitApp.cs`** → `nuget restore` → `msbuild /p:Configuration=Release /p:VsInstallRoot=...` → 上传 VSIX artifact。

---

## 3. 关键问题与排障历史

### 3.1 [已解决] CI 编译失败 `NU1102`
`Microsoft.VisualStudio.Shell.Framework` 被钉成 `17.14.40265`（不存在），改为 `17.14.40264`。

### 3.2 [已解决] 安装失败 `InstallByMsiException`
VSIX 清单含 `InstalledByMsi="true"`，VSIXInstaller 拒绝直接安装（除非由 MSI 提供清单）。已移除该属性。日志确认用户是在 **VS2026 Community（`D:\Program Files\VS18\`）** 上安装——说明 `[17.0,19.0)` 目标被 VS2026 接受。

### 3.3 [⚠️ 当前未解决 / 最重要] 运行时 MEF 契约不匹配
打开团队资源管理器时报：
```
System.ComponentModel.Composition.CompositionContractMismatchException:
无法将类型 ...MefV1ExportProvider+ComposablePartForExportFactory 的基础导出值
强制转换为类型 Microsoft.TeamFoundation.Controls.ITeamExplorerSection
  ... 在 Microsoft.TeamFoundation.Controls.WPF.TeamExplorer.Framework.TeamExplorerSectionHost.Create()
```
**根因分析**：
1. 扩展 UI 全用旧版 Team Explorer 扩展模型（`ITeamExplorerSection` 等，来自 `Microsoft.TeamFoundation.Controls.dll`）。
2. CI 在 **VS2022(17.x) runner** 上编译 → 产物引用 **v17** 的 `Microsoft.TeamFoundation.Controls`。
3. 在 **VS2026(18.x)** 运行时，宿主用的是**另一版本**的该程序集 → VS-MEF 把我们的 Section 强转成宿主的 `ITeamExplorerSection` 时类型对不上 → 抛异常 → GitLab 的 Section 加载失败。
4. 微软"VS2022 扩展直接在 VS2026 可用"只覆盖**受支持的 API**；**Team Explorer 这套老扩展模型不在其列**（VS2022/2026 默认启用"新的 Git 用户体验"，已替换 Team Explorer 的 Git 页）。

---

## 4. 建议的解决路径（按优先级）

### ✅ 路径 A（首选，本地验证）：在 VS2026 上本地编译
因 2.4 的改动，工程会按**当前 VS**解析 Team Explorer 程序集。在 VS2026 上编译，产物即引用 **18.x**，契约理论上与宿主匹配。
- **若能编译通过** → 说明 VS2026 仍保留该 API，重编后大概率修好运行时报错。
- **若编译报"找不到类型/接口"** → 说明 VS2026 删/改了该 API，必须走路径 C（迁移 UI）。

### 🔍 路径 B（免费快速排查，先试）
1. `工具 → 选项 → 环境 → 预览功能` → 取消勾选 **"新的 Git 用户体验 / New Git user experience"** → 重启 VS（切回经典 Team Explorer）。
2. 清 MEF 缓存：关 VS，删 `%LocalAppData%\Microsoft\VisualStudio\18.0_<id>\ComponentModelCache`，重启。

### 🛠 路径 C（长久方案，若 API 已不可用）
把登录 / MR / Issue 等入口从 Team Explorer **迁移到独立的工具窗口（ToolWindowPane）或 VS 新扩展模型**，不再依赖 `ITeamExplorerSection`。这是较大改造，但在 VS2026 上才是可持续的。

### 📦 路径 D（让 CI 也能出 VS2026 包）
按仓库多版本套路新增 **`GitLab.TeamFoundation.18`** 工程，引用 VS2026 的 Team Explorer 程序集。需把下列 DLL 从 VS2026 安装目录拷到仓库 `lib/18.0`（这样连 VS2022 的 CI runner 也能编译出引用 18.x 的程序集）：
```
D:\Program Files\VS18\Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer\
  Microsoft.TeamFoundation.Controls.dll   ← 最关键
  Microsoft.TeamFoundation.Common.dll  /  .Client.dll
  Microsoft.TeamFoundation.Git*.dll（Client/Controls/Provider/Git/Contracts）
  Microsoft.VisualStudio.TeamFoundation.dll（及 InitializationPackage / VersionControl 等）
```

---

## 5. 如何在本地编译（环境与步骤）

**前置**：Windows + **Visual Studio 2022 或 2026**（任意版本），需勾选工作负载：**"Visual Studio 扩展开发(VSSDK)"**、**.NET Framework 4.7.2 目标包**、**Team Explorer** 组件。

**必须先手动创建一个本地文件**（被 `.gitignore` 忽略，干净检出里没有它，否则编译报 `CS2001: ...Properties\GitApp.cs ... could not be found`）：
`src/GitLab.VisualStudio.UI/Properties/GitApp.cs`
```csharp
namespace GitLab.VisualStudio.UI.Properties
{
    internal static class GitApp
    {
        public const string client_id = "<你的 GitLab OAuth 应用 id（仅 PKCE 登录用，可填源码里已公开的那个）>";
        public const string client_secret = "<你的 GitLab OAuth 应用 secret>";
    }
}
```
> 注：`LoginViewModel.cs` 第 247 行已硬编码了一个公开的 PKCE `client_id`，本地占位可直接用它；CI 工作流也是自动生成这个占位文件来通过编译的。

**编译（VS2026 开发者命令提示符）**：
```cmd
nuget restore GitLabVS.sln
msbuild GitLabVS.sln /p:Configuration=Release
```
产物：`build\Release\GitLab.VisualStudio.vsix`（约 24MB）。安装时若提示"未签名扩展"属正常（`SignatureState: Unsigned`，不阻止安装）。

详见 `docs/build.md`。

---

## 6. 关键文件清单（改动 / 重点）

- `src/GitLab.VisualStudio/source.extension.vsixmanifest` — VSIX 清单（目标版本/架构/InstalledByMsi）
- `src/GitLab.TeamFoundation.17/GitLab.TeamFoundation.17.csproj` — TE 程序集引用方式（`$(VsInstallRoot)`）、链接 `.14` 源码
- `src/GitLab.TeamFoundation.14/**` — **Team Explorer 的真实源码**（Section / NavigationItem / ViewModel / View）
- `src/GitLab.VisualStudio/Services/WebService.cs` — GitLab API 调用（NGitLab）
- `src/GitLab.VisualStudio.UI/ViewModels/LoginViewModel.cs` — 登录逻辑（含半成品 OAuth）
- `src/*/app.config` — 绑定重定向
- `.github/workflows/build.yml` — CI（windows-2022 / 生成 GitApp.cs / 出 VSIX artifact）
- `docs/build.md`、`CHANGELOG.md`、`README.md`

---

## 7. 重要事实速查
- VS2026 = **18.x**；用户机器安装路径 **`D:\Program Files\VS18\`**。
- CI 用 **VS2022(17.14)** 编译（GitHub 暂无 VS2026 runner）→ 这是 3.3 运行时报错的直接来源。
- 微软 VSIX 兼容模型：17.x SDK 构建的扩展**针对受支持 API**可在 VS2026 运行；Team Explorer 老模型**不在受支持范围**。
- 上一次 CI 成功构建产物已可安装到 VS2026，仅运行时 Team Explorer 加载失败。
