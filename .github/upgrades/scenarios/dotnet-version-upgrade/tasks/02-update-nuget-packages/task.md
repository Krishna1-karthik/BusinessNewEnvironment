# 02-update-nuget-packages: Upgrade NuGet packages to net10-compatible versions

For each project, review NuGet packages flagged by assessment.md as incompatible or recommended for upgrade. Update package references to the latest compatible stable versions that support net10.0. Favor non-breaking minor/patch upgrades when available; if a major upgrade is required, document breaking changes and add remediation steps to the task.md.

Done when: All upgraded packages restore successfully and the solution builds or remaining errors are only due to known code-breaking API changes documented in task progress.
