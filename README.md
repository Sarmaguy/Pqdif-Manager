# Device Data Acquisition Service

## Windows Service for Parallel Measurement Device Communication, Data Acquisition, and Analysis
Powered by Gemstone.PQDIF

## 📌 Overview
- This solution is a C# Windows Service for high-volume measurement device data acquisition, analytics, and management.
- Core data flows: device communication (FTP/TCP) → data ingestion → SQL database storage → analytics (7-day sliding window) → WCF remote management.
- Uses the Gemstone.PQDIF library for parsing/generating PQDIF files (see `src/Gemstone.PQDIF/`).
- Configuration is loaded from an encrypted local file and further device/system config is fetched from SQL Server at startup.

## Key Components
- `PqdifSaver/`: Main service logic, entry points, and orchestration.
- `DbHandling/`: Data access, table builders, and repository patterns for SQL Server and DuckDB.
- `Helpers/`: Utility classes for compression, config, file visiting, and value manipulation.
- `FTPClient/`: FTP communication logic.
- `src/Gemstone.PQDIF/`: PQDIF file parsing/generation (logical/physical layers).
- `src/UnitTests/`: Unit tests for core logic.

## Developer Workflows
- **Build:** Use Visual Studio or `dotnet build Pqdif.sln` from the root.
- **Test:** Run `dotnet test src/Gemstone.PQDIF.sln` for unit tests.
- **Debug:** Main entry: `PqdifSaver/Program.cs`. Attach to service process for live debugging.
- **DocGen:** PQDIF library docs generated via Sandcastle (`src/DocGen/`).

## Patterns & Conventions
- Data access uses repository and builder patterns (see `DbHandling/Measurements/` and `DbHandling/DataBuilders/`).
- Prefer interfaces for extensibility (e.g., `IMeasurementRepository`, `IDataTableBuilder`).
- Configuration: Use `Helpers/Config/ConfigBuilder.cs` for config access.
- PQDIF parsing: Use `Logical/LogicalParser.cs` and `Physical/PhysicalParser.cs`.
- Tests: Place in `src/UnitTests/`, follow existing test structure.

## Integration & Dependencies
- External: SQL Server, DuckDB, FTP servers, Gemstone.PQDIF NuGet package.
- All cross-component communication is via interfaces and dependency injection.
- PQDIF file handling is isolated to the Gemstone.PQDIF library.

## Examples
- To add a new measurement type: implement `IMeasurementRepository` and update `MeasurementConstants.cs`.
- To add a new data table: implement `IDataTableBuilder` and register in `DbHandling`.

## References
- See `pqdif/docs/README.md` for PQDIF library details.