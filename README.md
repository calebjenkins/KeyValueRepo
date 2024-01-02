# KeyValueRepo
![KeyValueRepo Logo](./assets/logo/KeyValueRepoLogo.png)

[![.github/workflows/dev-ci.yml](https://github.com/calebjenkins/KeyValueRepo/actions/workflows/dev-ci.yml/badge.svg?branch=develop)](https://github.com/calebjenkins/KeyValueRepo/actions/workflows/dev-ci.yml)
[![.github/workflows/main-publish.yml](https://github.com/calebjenkins/KeyValueRepo/actions/workflows/main-publish.yml/badge.svg?branch=main)](https://github.com/calebjenkins/KeyValueRepo/actions/workflows/main-publish.yml)
[![NuGet](https://img.shields.io/nuget/dt/calebs.keyvaluerepo.svg)](https://www.nuget.org/packages/Calebs.KeyValueRepo) 
[![NuGet](https://img.shields.io/nuget/vpre/calebs.keyvaluerepo.svg)](https://www.nuget.org/packages/Calebs.KeyValueRepo)
[![.github/workflows/working-branches.yml](https://github.com/calebjenkins/KeyValueRepo/actions/workflows/working-branches.yml/badge.svg)](https://github.com/calebjenkins/KeyValueRepo/actions/workflows/working-branches.yml)

A generic interface for Key Value storage operations. Ideal for simple key-values that might migrate across in memory, SQL Server or NoSQL - a quick and easy way to create a KeyValue store.

By default, this library ships with an in-memory implementation. `KeyValueInMemory` which implements the `IKeyValueRepo` interface.

## Installing KeyValueRepo

You should install the KeyValueRepo [with NuGet](https://www.nuget.org/packages/Calebs.KeyValueRepo):

    Install-Package Calebs.KeyValueRepo
    
Or via the .NET Core command line interface:

    dotnet add package Calebs.KeyValueRepo

Either command, from Package Manager Console or .NET Core CLI, will download and install Calebs.KeyValueRepo and all required dependencies.

## Contributing
- PRs should be against the `Develop` branch.
- Merged PRs to `Develop` will trigger a `[version]-ci-[buildnumber]` deployment to nuget, assuming all unit tests pass.
- Merged PRs to `Main` will trigger a [version] deployment to nuget, assuming all of the tests pass.

## Versioning
The package `version` is defined in the `KeyValueRepo.csproj` file, using .NET SDK style structure. We follow `semantic versioning` for this package.


## Methods in IKeyValueRepo

- Get<T>(string Id)
- Get<T>(int Id) => Get<T>(string Id)
    - Note: This method has a _default implementation_ that performs a `ToString()` on the Id and calls the `Get<T>(string Id)` method.
- GetAll<T>()
- Update<T>(string Id, T object)
- Update<T>(int Id, T object) => Update(string Id, T object)
    - Note This method has a _default implementation_ that performs a `ToSTring()` on the int Id and calls the `Update<T>(string Id, T object)` method.


## Release History & Development Goals
- 0.1.0 - initial release (in Memory)
- 0.2.0 - added SqlLite Implementation
- ✔ SQLite [Nuget Package](https://www.nuget.org/packages/Calebs.KeyValueRepo.SQLite/)
### Future Goals
---
- In Progress: 0.3.0 - New Feature: KV Meta Objects
- In Progress: Seperate and publish base unit tests
- Consider: Adding Remove (id) and RemoveAll methods
- New Repo: Azure Tables Implementation
- New Repo: Sql Server Implementation
- New Repo: Azure CosmoDB Implementation