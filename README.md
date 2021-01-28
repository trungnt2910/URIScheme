# URIScheme

# Usage
```csharp
const string key = @"ssa";
var service = new URISchemeService(key, @"URL:ssa Protocol", @"D:\MyAppPath\MyApp.exe --openurl");
// var service = new URISchemeService(key, @"URL:ssa Protocol", @"D:\MyAppPath\MyApp.exe --openurl", RegisterType.LocalMachine);

var isSet = service.Check();

service.Set();

service.Delete();
```

# Notes

This library currently supports only Windows. However, efforts are being made to port this, first to Linux, then to MacOS.

If you know how to register URI schemes on other platforms, feel free to make a pull request.
