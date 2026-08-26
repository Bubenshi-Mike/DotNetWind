# DotNetWind Compatibility Matrix

| Project type | SDK / detection rule | Default input | Default output | Host file behavior | CSS reference |
| --- | --- | --- | --- | --- | --- |
| Blazor WebAssembly | `Microsoft.NET.Sdk.BlazorWebAssembly`, WebAssembly package reference, or `wwwroot/index.html` | `Styles/tailwind.css` | `wwwroot/css/style.css` | Looks for `wwwroot/index.html` | `<link href="css/style.css" rel="stylesheet" />` |
| Blazor Web App | `Microsoft.NET.Sdk.Web` with `Components/App.razor` or `Components/Routes.razor` | `Styles/tailwind.css` | `wwwroot/css/style.css` | Looks for `Components/App.razor`, then `App.razor` | `<link href="css/style.css" rel="stylesheet" />` |
| Blazor Server | `Microsoft.NET.Sdk.Web` with `Pages/_Host.cshtml` | `Styles/tailwind.css` | `wwwroot/css/style.css` | Looks for `Pages/_Host.cshtml`, then `Pages/_Layout.cshtml` | `<link href="css/style.css" rel="stylesheet" />` |
| ASP.NET Core MVC | `Controllers/` and `Views/` directories | `Styles/tailwind.css` | `wwwroot/css/style.css` | Looks for `Views/Shared/_Layout.cshtml` | `<link href="~/css/style.css" rel="stylesheet" />` |
| Razor Pages | `Pages/` plus `Pages/Shared/_Layout.cshtml` | `Styles/tailwind.css` | `wwwroot/css/style.css` | Looks for `Pages/Shared/_Layout.cshtml` | `<link href="~/css/style.css" rel="stylesheet" />` |
| Razor Class Library | `Microsoft.NET.Sdk.Razor` | `Styles/tailwind.css` | `wwwroot/css/style.css` | No local app host file is expected | `<link href="_content/{ProjectName}/css/style.css" rel="stylesheet" />` from the consuming app |

## Notes

- Use `--framework` when project detection is ambiguous.
- Use `--input` and `--output` to override generated Tailwind paths.
- Razor Class Libraries expose files under `_content/{ProjectName}/` through static web assets.
- `doctor` validates project-local files and gives host-file guidance based on the detected project type.
