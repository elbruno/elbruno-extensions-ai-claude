# NuGet Publishing Steps for elbruno.Extensions.AI.Claude

## Current Status

✅ Step 1: Package metadata updated (Description and Tags now reference Microsoft Foundry and Claude 4.x models)

## Remaining Steps

### 2. Get Your NuGet API Key

1. Go to <https://www.nuget.org>
2. Sign in or create an account
3. Navigate to your profile → API Keys
4. Create a new API key with "Push" permissions
5. Save the API key securely (you'll need it for step 5)

### 3. Build and Pack the Project

```powershell
cd c:\src\elbruno-extensions-ai-claude
dotnet build --configuration Release
dotnet pack src/elbruno.Extensions.AI.Claude/elbruno.Extensions.AI.Claude.csproj --configuration Release --output ./nupkg
```

This will create: `./nupkg/elbruno.Extensions.AI.Claude.0.1.0-preview.1.nupkg`

### 4. Test the Package Locally (Optional but Recommended)

```powershell
# Create a test project
dotnet new console -n TestPackage
cd TestPackage
dotnet add package elbruno.Extensions.AI.Claude --source ..\nupkg

# Test the package
# Add code to Program.cs to test the library
dotnet run

# Clean up
cd ..
Remove-Item -Recurse -Force TestPackage
```

### 5. Publish to NuGet

```powershell
dotnet nuget push ./nupkg/elbruno.Extensions.AI.Claude.0.1.0-preview.1.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

**Replace `YOUR_API_KEY` with the actual API key from step 2**

### 6. Verify Publication

- Go to <https://www.nuget.org/packages/elbruno.Extensions.AI.Claude>
- It may take a few minutes (5-10 minutes) for the package to be indexed and searchable
- The package will be marked as "pre-release" because of the `-preview.1` suffix

## Package Information

- **Package ID**: elbruno.Extensions.AI.Claude
- **Version**: 0.1.0-preview.1
- **License**: MIT
- **Repository**: <https://github.com/elbruno/elbruno-extensions-ai-claude>

## Notes

- This is a preview release (`0.1.0-preview.1`)
- Supports Claude Sonnet 4.5, Haiku 4.5, and Opus 4.1 in Microsoft Foundry
- Implements Microsoft.Extensions.AI abstractions
- Targets .NET 8.0

## After Publishing

Consider:

1. Creating a GitHub release with the same version tag (v0.1.0-preview.1)
2. Announcing the package on social media or relevant communities
3. Monitoring for issues and feedback from early adopters
4. Planning the next release (bug fixes, features, or moving to stable 0.1.0)

## Future Versions

- `0.1.0-preview.2`, `0.1.0-preview.3`, etc. - More preview releases with fixes
- `0.1.0` - First stable release
- `0.2.0`, `1.0.0` - Major feature additions

To update and republish:

1. Update version in `elbruno.Extensions.AI.Claude.csproj`
2. Repeat steps 3 and 5 with the new version number
