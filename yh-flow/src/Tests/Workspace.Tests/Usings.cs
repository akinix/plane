// Additional project-wide using directives for the Workspace.Tests project.
// Mirrors Identity.Tests/GlobalUsings.cs but adds the Finbuckle + AppTenantInfo namespaces the
// Q1 spike relies on. (Identity.Tests has no Finbuckle dependency; this project does.)
global using Finbuckle.MultiTenant;
global using Finbuckle.MultiTenant.Abstractions;
global using YH.Framework.Shared.Multitenancy;
