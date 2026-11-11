# Unreleased

- Changed target frameworks from .NET 8 to .NET 10 and .NET 11
- Updated MailKit to 5.0.0, Microsoft.Graph to 6.10.0, and test dependencies
- Updated the required SDK to .NET 11.0.100 with patch roll-forward and automatic `global.json` discovery
- Removed explicit Microsoft.SourceLink.GitHub package references

# 2.4.2

- Fixed Microsoft Graph NuGet package generation for Release builds [#49](https://github.com/trenz-gmbh/TRENZ.Lib.RazorMail/pull/49)
- Bumped the package version to publish the packaging fix to NuGet [#50](https://github.com/trenz-gmbh/TRENZ.Lib.RazorMail/pull/50)

# 2.4.1

- Fixes publish TRENZ.Lib.RazorMail.MicrosoftGraph to NuGet

# 2.4.0

- Added support for the Microsoft Graph API

# 2.3.1

- Security: Updated `MailKit` to 4.15.1

# 2.3.0

- Added support for setting a message importance (`Importance`, `X-Priority`) [#26](https://github.com/trenz-gmbh/TRENZ.Lib.RazorMail/pull/26) by @chucker
- Fixed a NRE when sending mails without attachments [#29](https://github.com/trenz-gmbh/TRENZ.Lib.RazorMail/pull/30) by @ricardoboss

# 2.2.0

- Added some badges to the README
- Marked `AddRazorEmailRenderer` obsolete, use `AddRazorMailRenderer` instead
- Marked `AddMailKitRazorMailClient` obsolete, use `AddMailKitMailClient` instead
- Marked `AddSystemNetRazorMailClient` obsolete, use `AddSystemNetMailClient` instead
- Added `configureClient` parameter to `AddMailKitMailClient` and `AddSystemNetMailClient` so you can configure the
  client instance (e.g. default headers)

# 2.1.1

- Initial nuget.org release
- No feature changes, just a version bump
