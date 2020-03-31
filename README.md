# .NET CanonicalJson

.Net library for producing JSON in canonical format as specified by [https://gibson042.github.io/canonicaljson-spec/](https://gibson042.github.io/canonicaljson-spec/). The provided interface matches that of native JSON object.

## Installation

Use the NuGet package manager console to install CanonicalJson. Note that this library is [.NET Standard 2.0](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) compatible.

```bash
Install-Package Stratumn.CanonicalJson
```

## Usage

```csharp
import Stratumn.CanonicalJson;

string obj = Canonicalizer.Canonicalize("{ \"a\": 12 }"));
```

## Development

Download the [.NET Core SDK](https://dotnet.microsoft.com/download) if you don't already have it.

```sh
# Install .NET dependencies
dotnet restore

# Fetch git submodule
git submodule init
git submodule update

# Start testing
dotnet test CanonicalTest/CanonicalJsonTest.csproj
```

