## Getting Started

This project should be used to convert a `.json` file to `.xml` file.

### Prerequisites

- .NET 9.0 SDK

### Run the API

```bash
cd Ubiminds.API
dotnet run
```

## Testing with Swagger

When running in development mode, Swagger UI is available at `https://localhost:5001/swagger`

## Algorithm & Design Choices

### Key improvements
- We could use a package like `Newtonsoft` but we need to create a property mapping and property list of properties to omit in the `.xml` file.

### Ubiminds.Data
- Responsible for doing the conversion of `.json` file to `.xml`
- LoggerExtension


## Feedback
- The file `SampleInput.json` has value in a key `CountryIds` but the file `SampleResult.xml` is empty.
- The `CountryIds` should be a List<int>, but has a List<string>
- `SampleResult.xml` is invalid because the key `<ContactInformation>` has no closing tag and should have `</ContactInformation>`
- The output `.xml` file should omit the `.json` properties:
  - Id
  - PublishDate
  - Status
  - TestRun 
  - `CountryIds` should be named as `Countries`

## Author
Carlos Eduardo Augusto Aguiar