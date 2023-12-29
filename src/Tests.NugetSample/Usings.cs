global using Xunit;
global using Xunit.Abstractions;
global using FluentAssertions;
global using Calebs.Data.KeyValueRepo;
global using Calebs.Data.KeyValueRepo.SqlLite;
global using Moq;
global using Microsoft.Extensions.Logging;
global using System.Diagnostics;

// nuget tests are on hold moving from 0.1.0 to 0.2.0
// These are breaking changes from exisisting nuget package moving to Task Async
// ahhh! I will be adding back in the unit test linked page after the upgrade
// Need a better way to manage intentional breaking changes. 