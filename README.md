# URIScheme

# Usage
```csharp
const string key = @"ssa";

// Creates a IURISchemeService object of the correct type for each supported OS. 
var service = URISchemeServiceFactory.GetURISchemeSerivce(key, @"URL:ssa Protocol", @"D:\MyAppPath\MyApp.exe --openurl");
// var service = URISchemeServiceFactory.GetURISchemeSerivce(key, @"URL:ssa Protocol", @"D:\MyAppPath\MyApp.exe --openurl", RegisterType.LocalMachine);

// Check if the protocol is registered to any application.
var hasProtocol = service.CheckAny();

// Check if the protocol is registered to the current application.
var isSet = service.Check();

// Register the service.
service.Set();

// Unregister the service.
service.Delete();
```

# Notes

This library supports Windows and all Linux distros that follows the FreeDesktop specification.

MacOS is not supported, as there seems to be no API to register URI schemes programatically on this platform.
Registering must be done by saving some in kind of Info file in a application package. See more information [here](https://stackoverflow.com/questions/19453829/register-a-mono-application-to-an-uri-scheme-on-osx).

Feel free to create issues here for any bugs you encounter.

If you know how to register URI schemes on other platforms, pull requests are welcome.
