# 01-update-projects-to-net10: Update project TargetFrameworks to net10.0

Update each project file's TargetFramework (or TargetFrameworks) to `net10.0`. Where projects currently multi-target, add `net10.0` alongside existing targets if needed and prefer single-targeting `net10.0` when safe.

Affected items: All projects reported by assessment.md as targeting net8.0

## Scope Inventory

- Projects affected:
  - BusinessNewEnvironment\BusinessNewEnvironment.csproj

- Key package references to validate/upgrade (from project files):
  - BCrypt.Net-Next 4.0.3
  - Microsoft.EntityFrameworkCore 8.0.0
  - Microsoft.EntityFrameworkCore.Tools 8.0.0
  - Microsoft.IdentityModel.Tokens 8.6.0
  - Microsoft.VisualStudio.Azure.Containers.Tools.Targets 1.21.0
  - Npgsql.EntityFrameworkCore.PostgreSQL 8.0.0
  - Swashbuckle.AspNetCore 6.6.2
  - System.IdentityModel.Tokens.Jwt 8.6.0

- Distinct concerns:
  - Project TFM replacement (net8.0 -> net10.0)
  - NuGet package compatibility with net10.0
  - Potential API changes from framework or package upgrades

Done when: All project files reference `net10.0` and solution builds (or builds with known package-related errors documented).
