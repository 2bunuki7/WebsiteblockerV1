# Website Blocker

A Windows desktop application built with C# and .NET that allows users to block selected websites directly from their computer.

## Features

- Add websites to a block list
- Remove websites from the block list
- Enable or disable blocking
- Automatically requests Administrator privileges
- Blocks websites using the Windows hosts file
- Supports subdomains
- Saves settings locally
- DNS cache flushing after changes
- Logging support
- Automated tests
- Windows background service support


## Project Structure

```text
WebsiteBlocker/
│
├── src/
│   ├── WebsiteBlocker.App/
│   ├── WebsiteBlocker.Core/
│   └── WebsiteBlocker.Service/
│
├── data/
├── tests/
│
├── .gitignore
├── README.md
└── LICENSE
```
## Tech Stack

- C#
- .NET 10
- WPF
- XAML
- Windows Services
- JSON
- xUnit
- Git / GitHub

## More in depth explanation of how it works and how it's built

Website Blocker is built as a modular Windows application using **C# and .NET 10**. Instead of being a browser extension, the application works at the **Windows system level** by modifying the Windows `hosts` file. This allows the blocker to affect websites outside of a single browser.

The project is separated into multiple components so that the user interface, blocking logic, configuration management, and background service are not all mixed together.

### 1. Overall architecture

The application is divided into three main projects:

```text
WebsiteBlocker
│
├── WebsiteBlocker.App
│   └── WPF desktop application
│
├── WebsiteBlocker.Core
│   └── Main application logic
│
└── WebsiteBlocker.Service
    └── Windows background service
```

There is also a test project:

```text
tests/
└── WebsiteBlocker.Tests
```

The general flow of the application looks like this:

```text
                 ┌──────────────────────┐
                 │      WPF App          │
                 │ WebsiteBlocker.App   │
                 └──────────┬───────────┘
                            │
                            │ User actions
                            ▼
                 ┌──────────────────────┐
                 │    Core Library      │
                 │ WebsiteBlocker.Core │
                 └───────┬───────┬──────┘
                         │       │
             ┌───────────┘       └─────────────┐
             ▼                                 ▼
   ┌────────────────────┐           ┌────────────────────┐
   │ Configuration      │           │ Blocker Service    │
   │ Service            │           │                    │
   └─────────┬──────────┘           └─────────┬──────────┘
             │                                │
             ▼                                ▼
   ┌────────────────────┐           ┌────────────────────┐
   │ settings.json      │           │ Windows hosts file │
   └────────────────────┘           └────────────────────┘
```

The Windows Service uses the same Core library:

```text
             ┌──────────────────────┐
             │ WebsiteBlocker.Service│
             └──────────┬───────────┘
                        │
                        ▼
             ┌──────────────────────┐
             │   WebsiteBlocker.Core│
             └──────────┬───────────┘
                        │
                        ▼
             ┌──────────────────────┐
             │ Windows hosts file   │
             └──────────────────────┘
```

This structure makes it possible to change the user interface without rewriting the actual blocking system.

---

### 2. WebsiteBlocker.App

`WebsiteBlocker.App` is the graphical user interface of the application.

It is built using:

* C#
* WPF
* XAML
* .NET 10
* Windows desktop APIs

The main window allows the user to:

* Add a website
* Remove a website
* View currently blocked websites
* Enable the blocker
* Disable the blocker
* See whether the blocker is currently enabled

The UI itself does not contain all of the blocking logic.

Instead, it communicates with the Core project.

For example, when the user enters:

```text
youtube.com
```

and presses the add button, the application roughly performs this sequence:

```text
User enters website
        │
        ▼
MainWindow receives button click
        │
        ▼
Website is cleaned/normalized
        │
        ▼
ConfigurationService loads settings
        │
        ▼
Domain is added to BlockedSites
        │
        ▼
ConfigurationService saves settings
        │
        ▼
UI refreshes the website list
        │
        ▼
If blocker is enabled:
        │
        ▼
BlockerService updates hosts file
```

This keeps the UI relatively simple while the actual logic remains inside the Core project.

---

### 3. WPF and XAML

The application uses **WPF (Windows Presentation Foundation)** for the desktop interface.

WPF separates the visual interface from the C# logic.

The `.xaml` file defines the interface:

```xml
<Window>
    ...
</Window>
```

while the corresponding `.xaml.cs` file contains the behavior.

For example:

```text
MainWindow.xaml
        │
        │ defines the UI
        ▼
MainWindow.xaml.cs
        │
        │ handles events
        ▼
WebsiteBlocker.Core
```

The application uses event handlers for actions such as clicking the Add Website or Enable Blocker buttons.

For example, an event such as:

```csharp
private void AddWebsite_Click(...)
```

is triggered when the user presses the Add Website button.

The handler then communicates with the Core services.

---

### 4. Administrator privileges

The Windows `hosts` file is a protected system file.

Its normal location is:

```text
C:\Windows\System32\drivers\etc\hosts
```

Normal applications generally cannot modify this file without elevated privileges.

Because Website Blocker needs to modify the file, the application includes an `app.manifest`.

The manifest requests:

```xml
<requestedExecutionLevel
    level="requireAdministrator"
    uiAccess="false" />
```

This tells Windows that the application should request administrator privileges when it starts.

As a result, Windows displays the User Account Control prompt when necessary.

The application therefore does not depend on the user remembering to manually select:

```text
Run as administrator
```

every time.

---

### 5. WebsiteBlocker.Core

`WebsiteBlocker.Core` contains the main logic of the application.

This project is intentionally separated from the WPF interface.

It contains components such as:

```text
Models/
    BlockedSite.cs
    BlockerSettings.cs

Services/
    BlockerService.cs
    DomainMatcher.cs
    ConfigurationService.cs
    Logger.cs
```

The Core project can therefore be reused by:

* The WPF application
* The Windows Service
* Automated tests
* Potential future interfaces

This is an important part of the architecture because the blocking engine does not depend directly on the graphical interface.

---

### 6. BlockerSettings

`BlockerSettings` represents the application's configuration.

It contains information such as:

```csharp
public bool Enabled { get; set; }

public List<string> BlockedSites { get; set; }

public bool BlockSubdomains { get; set; }

public bool EnableLogging { get; set; }
```

Conceptually, the settings look like:

```text
BlockerSettings
│
├── Enabled
│
├── BlockedSites
│   ├── youtube.com
│   ├── reddit.com
│   └── example.com
│
├── BlockSubdomains
│
└── EnableLogging
```

This means the rest of the application does not need to manually manage individual configuration values.

Instead, it works with one settings object.

---

### 7. ConfigurationService

The `ConfigurationService` is responsible for loading and saving application settings.

The settings are stored as JSON in the user's local application data directory.

Conceptually, the configuration looks similar to:

```json
{
    "Enabled": true,
    "BlockedSites": [
        "youtube.com",
        "reddit.com",
        "example.com"
    ],
    "BlockSubdomains": true,
    "EnableLogging": true
}
```

The application does not hard-code the blocked websites directly into the program.

Instead:

```text
Application
     │
     ▼
ConfigurationService
     │
     ▼
settings.json
```

When the application starts, it loads the existing settings.

When the user changes the block list, the settings are saved again.

This means the user's configuration remains available after restarting the application.

---

### 8. DomainMatcher

The `DomainMatcher` is responsible for normalizing website addresses.

Users might enter a website in several different ways:

```text
youtube.com
www.youtube.com
https://youtube.com
https://www.youtube.com/
https://youtube.com/watch?v=123
```

These all contain the same basic domain:

```text
youtube.com
```

The DomainMatcher removes unnecessary parts of the input so the application can work with a consistent domain format.

For example:

```text
https://www.youtube.com/watch?v=123
```

can be normalized into:

```text
youtube.com
```

This prevents duplicate entries and makes domain matching more predictable.

---

### 9. How website blocking actually works

The actual blocking mechanism uses the Windows `hosts` file.

The hosts file is a local mapping between domain names and IP addresses.

For example:

```text
127.0.0.1 example.com
```

tells Windows to resolve:

```text
example.com
```

to:

```text
127.0.0.1
```

instead of the real server address.

`127.0.0.1` is the computer's local loopback address.

In simplified terms:

```text
Normal request:

Browser
   │
   ▼
DNS
   │
   ▼
Real website server
```

With a hosts entry:

```text
Browser
   │
   ▼
Windows name resolution
   │
   ▼
hosts file
   │
   ▼
127.0.0.1
```

The browser therefore does not reach the real website through that hostname.

---

### 10. How Website Blocker modifies the hosts file

Website Blocker does not rewrite the entire hosts file.

Instead, it adds a clearly identifiable section.

For example:

```text
# WEBSITE_BLOCKER_START
127.0.0.1 youtube.com
127.0.0.1 www.youtube.com
127.0.0.1 reddit.com
127.0.0.1 www.reddit.com
# WEBSITE_BLOCKER_END
```

The markers are important:

```text
# WEBSITE_BLOCKER_START
```

and:

```text
# WEBSITE_BLOCKER_END
```

They tell the application exactly which lines belong to Website Blocker.

This means the application can later remove its own entries without accidentally deleting unrelated entries from the hosts file.

---

### 11. Enabling the blocker

When the user presses:

```text
Enable Blocker
```

the application performs approximately these operations:

```text
1. Load current settings
        │
        ▼
2. Check BlockedSites
        │
        ▼
3. Remove the application's old hosts entries
        │
        ▼
4. Add WEBSITE_BLOCKER_START
        │
        ▼
5. Add 127.0.0.1 entries
        │
        ▼
6. Add WEBSITE_BLOCKER_END
        │
        ▼
7. Save the hosts file
        │
        ▼
8. Flush Windows DNS cache
```

Before adding new entries, the application removes its previous block section.

This prevents old domains from remaining in the hosts file after the user modifies the block list.

---

### 12. Disabling the blocker

When the user disables the blocker, the application searches for:

```text
# WEBSITE_BLOCKER_START
```

and:

```text
# WEBSITE_BLOCKER_END
```

Everything between those markers belongs to the application.

That section is removed.

For example:

Before:

```text
127.0.0.1 some-other-entry

# WEBSITE_BLOCKER_START
127.0.0.1 youtube.com
127.0.0.1 www.youtube.com
# WEBSITE_BLOCKER_END

127.0.0.1 another-entry
```

After disabling:

```text
127.0.0.1 some-other-entry

127.0.0.1 another-entry
```

The application therefore only removes the entries that it created.

---

### 13. DNS cache flushing

Windows can temporarily cache DNS information.

Because the hosts file has changed, Website Blocker calls:

```text
ipconfig /flushdns
```

after modifying the hosts file.

This asks Windows to clear its local DNS cache.

The simplified sequence is:

```text
Modify hosts file
       │
       ▼
Flush DNS cache
       │
       ▼
Windows performs fresh name resolution
```

Without clearing cached information, a previously resolved domain may continue behaving normally for a period of time.

---

### 14. Why the application blocks more than one browser

A browser extension generally works inside one browser.

For example:

```text
Chrome Extension
      │
      ▼
Chrome
```

The Website Blocker architecture is different:

```text
Website Blocker
      │
      ▼
Windows hosts file
      │
      ├── Chrome
      ├── Firefox
      ├── Edge
      └── Other applications
```

Because the hosts file is part of Windows name resolution, the blocking mechanism is not tied to one particular browser.

However, hosts-file blocking is not equivalent to a complete network firewall or DNS filtering system. Modern applications and websites can use additional domains, alternative network mechanisms, DNS-over-HTTPS, IPv6, caching, or other techniques that can affect how effective a simple hosts-file block is.

The current version intentionally uses the hosts file because it is simple, local, transparent, and useful for the first version of the project.

---

### 15. WebsiteBlocker.Service

The project also contains a separate Windows Service:

```text
WebsiteBlocker.Service
```

The purpose of this component is to allow the blocker to operate in the background without requiring the graphical application to remain open.

The service uses the .NET Worker Service infrastructure.

Its basic architecture is:

```text
Windows
   │
   ▼
Website Blocker Service
   │
   ▼
Worker
   │
   ├── ConfigurationService
   │
   ├── BlockerService
   │
   └── Logger
```

The worker periodically checks the configuration.

Conceptually:

```text
Start service
     │
     ▼
Load settings
     │
     ▼
Is Enabled true?
    / \
  Yes  No
   │    │
   ▼    ▼
Block  Remove
sites  blocks
   │    │
   └────┘
      │
      ▼
Wait
      │
      ▼
Check again
```

The current worker checks the configuration periodically rather than continuously monitoring every network request.

This is intentionally lightweight.

---

### 16. Why there is a separate Windows Service

The graphical application and the background service have different responsibilities.

The GUI is responsible for:

```text
User interaction
        │
        ├── Add website
        ├── Remove website
        ├── Enable
        └── Disable
```

The service is responsible for:

```text
Background operation
        │
        ├── Load configuration
        ├── Maintain blocking
        └── Write logs
```

This separation makes the application easier to expand later.

For example, the GUI could eventually be closed while the service continues enforcing the configuration.

---

### 17. Logging

The application contains a `Logger` service.

Logs are stored inside the user's local application data directory.

The logger records events such as:

```text
Website Blocker service started.
```

or errors encountered while applying the configuration.

A log entry has a timestamp:

```text
[2026-09-28 18:30:12] Website Blocker service started.
```

This makes it easier to diagnose problems without displaying every technical detail to the user.

---

### 18. Error handling

The application uses exception handling around operations that can fail.

For example, modifying the hosts file can fail if:

* Administrator privileges are missing
* The file cannot be accessed
* Another process is using the file
* The configuration is invalid
* A filesystem operation fails

Instead of allowing the entire application to crash, errors are caught and handled.

For GUI operations, the user can receive a message explaining what happened.

For background operations, the service writes the error to the log.

The general approach is:

```text
Operation
   │
   ▼
Try
   │
   ├── Success → Continue
   │
   └── Error → Handle exception
                    │
                    ├── GUI → Show message
                    │
                    └── Service → Log error
```

---

### 19. Tests

The project also contains a separate test project:

```text
tests/
└── WebsiteBlocker.Tests/
```

The tests use **xUnit**.

Testing is particularly useful for logic such as domain normalization.

For example, the application should be able to consistently process inputs such as:

```text
example.com
www.example.com
https://example.com
https://www.example.com/
```

Automated tests help make sure changes to the application do not accidentally break existing functionality.

The test project references the Core project rather than testing the WPF interface directly.

This is another reason why separating the application into Core and UI projects is useful.

---

### 20. Complete application flow

Putting everything together, a typical user interaction looks like this:

```text
┌──────────────────────────────┐
│ User opens Website Blocker   │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ Windows requests admin       │
│ privileges                   │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ WPF application starts      │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ ConfigurationService loads   │
│ settings.json                │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ User adds example.com        │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ DomainMatcher normalizes     │
│ the domain                   │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ ConfigurationService saves   │
│ the updated configuration    │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ User enables blocker         │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ BlockerService updates       │
│ Windows hosts file           │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ DNS cache is flushed         │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ Windows resolves the blocked │
│ domain to 127.0.0.1          │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ Connection does not reach    │
│ the real website server      │
└──────────────────────────────┘
```

---

### 21. Why the project is split into multiple projects

The project could have been written as one large C# application, but separating the components makes the code easier to maintain.

The responsibilities are approximately:

| Project / Component      | Responsibility                    |
| ------------------------ | --------------------------------- |
| `WebsiteBlocker.App`     | WPF graphical interface           |
| `WebsiteBlocker.Core`    | Main application logic            |
| `BlockerService`         | Hosts-file blocking               |
| `ConfigurationService`   | Loading and saving settings       |
| `DomainMatcher`          | Domain normalization and matching |
| `Logger`                 | Logging application events        |
| `WebsiteBlocker.Service` | Background Windows Service        |
| `WebsiteBlocker.Tests`   | Automated tests                   |

This follows the general principle of **separation of concerns**.

Each component has a relatively specific responsibility instead of one class doing everything.

---

### 22. Current limitations

The current implementation is intentionally a V1 implementation.

Because it relies on the Windows hosts file, it does not provide the same level of control as a full DNS filtering system, firewall, VPN, or network-level filtering solution.

For example, blocking a website may require blocking multiple domains if the website loads resources or services from different hostnames.

Other limitations can include:

* DNS caching
* IPv6 resolution
* DNS-over-HTTPS
* QUIC/HTTP3
* Websites using multiple domains
* Applications that do not rely on normal hostname resolution
* Browser-specific caching

These are reasons why future versions can move beyond the hosts-file approach.

The hosts-file system is therefore best considered the foundation of the first version rather than the final networking architecture.

---

### 23. Planned improvements

The architecture leaves room for several future improvements.

Possible future features include:

```text
Website Blocker
│
├── Current
│   ├── WPF interface
│   ├── Hosts-file blocking
│   ├── JSON configuration
│   ├── Domain matching
│   ├── Logging
│   └── Windows Service
│
└── Future
    ├── Scheduled blocking
    ├── PIN/password protection
    ├── System tray application
    ├── Blocking statistics
    ├── Custom blocked page
    ├── Improved DNS filtering
    ├── Category-based blocking
    ├── Better IPv6 handling
    └── More advanced network filtering
```

The goal is to keep the existing Core architecture reusable so these features can be added without rebuilding the entire application.

---

### 24. Summary

Website Blocker is essentially a layered Windows application:

```text
                    USER
                     │
                     ▼
              WPF / XAML UI
                     │
                     ▼
            WebsiteBlocker.Core
                     │
          ┌──────────┼──────────┐
          ▼          ▼          ▼
    Configuration  Domain     Blocking
      Service      Matcher     Service
          │                     │
          ▼                     ▼
    settings.json         Windows hosts
                                │
                                ▼
                         DNS cache flush
                                │
                                ▼
                         Windows network
```

The most important design decision is that the **UI is separated from the blocking engine**.

The WPF application handles interaction with the user, while the Core project handles the actual logic. The Windows Service can then reuse the same Core logic to maintain blocking in the background.

This makes the project easier to test, maintain, and expand while also giving the application a clear architecture instead of putting all functionality inside one large file.






