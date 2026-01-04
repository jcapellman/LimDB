# LimDB

## Overview
LimDB, which stands for Lightweight In-Memory Database is a small project to allow a simple JSON file to power Mobile, Desktop or Web applications where an embedded database like SQLite or LiteDb or a full database like Postgres or MongoDb is overkill. This database is meant for static data, less so with frequently changing data. However, basic CRUD operations are implemented right now. Future Transactions queueing and Transaction Log functionality will be implemented.

## Dependencies
Given the goal of being lightweight and versatile, the database has no dependencies outside of .NET 10. 

## Build Status
[![Publish to NuGet](https://github.com/jcapellman/LimDB/actions/workflows/PublishToNuGet.yml/badge.svg)](https://github.com/jcapellman/LimDB/actions/workflows/PublishToNuGet.yml)

## Releases
LimDB is available on NuGet.org and has full CI/CD enabled on GitHub for commits on the Main branch. You can download the latest version here: https://www.nuget.org/packages/LimDB.lib
