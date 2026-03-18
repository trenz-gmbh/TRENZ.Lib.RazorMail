# Unreleased

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
