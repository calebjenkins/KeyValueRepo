# KeyValueRepo
A generic interface for Key Value storage operations. Ideal for simple key-values that might migrate across in memory, SQL Server or NoSQL

By default, this library ships with an in-memory implementation. `KeyValueInMemory` which implements the `IKeyValueRepo` interface.

## To Install
Use nuget package manager

## To Contribute
PRs should be against the `Develop` branch.

Merged PRs to `Develop` will trigger a `[version]-ci-[buildnumber]` deployment to nuget, assuming all unit tests pass.

Merged PRs to `Main` will trigger a [version] deployment to nuget, assuming all of the tests pass.

## Versioning
The package `version` is defined in the `KeyValueRepo.csproj` file, using .NET SDK style structure. We follow `semantic versioning` for this package.


## Methods in IKeyValueRepo

- Get<T>(string Id)
- Get<T>(int Id)
    - Note: This method a default implementation that performs a `ToString()` on the Id and calls the `Get<T>(string Id)` method.
- GetAll<T>()
- Update<T>(string Id, T object)
- Update<T>(int Id, T object)
    - Note This method has a default implementation that performs a `ToSTring()` on the int Id and calls the `Update<T>(string Id, T object)` method.
