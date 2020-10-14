# dotnet-cldr-resx
[Dotnet tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) that generates RESX files based on CLDR data. The resulting files could be used in a .NET Core project to display localized names of languages or countries.

# Installation

1. `dotnet pack`
2. `dotnet tool install -g dotnet-cldr-resx --add-source .\nupkg\`

# Usage

Prerequirements: a local copy of the [cldr-localenames-modern](https://www.npmjs.com/package/cldr-localenames-modern) package. If you do not already have it installed, do `npm i cldr-localenames-modern --save-dev` first.

In your .NET Core project root, executing

`cldr-resx --source territories -l da en fi nb sv -o .\Resources\_Countries`

will generate five files

* `.\Resources\_Countries.da.resx`
* `.\Resources\_Countries.en.resx`
* `.\Resources\_Countries.fi.resx`
* `.\Resources\_Countries.nb.resx`
* `.\Resources\_Countries.sv.resx`

Each resource file will contain the ISO code as key, and localized name as value.

A project set up with `services.AddLocalization(options => options.ResourcesPath = "Resources")` could then inject `IStringLocalizer<_Countries> _countryLocalizer` into a controller.

`_countryLocalizer[countryCode]` would then return a localized name of the country specified by `countryCode`.

# Uninstall

`dotnet tool uninstall -g dotnet-cldr-resx`
