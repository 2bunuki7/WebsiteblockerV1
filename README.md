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

I built Website Blocker as a Windows desktop application using **C# and .NET 10**. The main idea is pretty simple: you add websites that you don't want accessible, turn the blocker on, and the application handles the rest.

Instead of making this as a browser extension, I wanted to make it work at the **Windows level**. Because of that, the application uses the Windows `hosts` file to block domains.

I also wanted the project to be more than just one C# file with everything inside it, so I split it into different projects and services. This makes the code easier to understand, test, and expand later.

---

### 1. How the project is structured

The project currently has three main applications/projects:

```text
WebsiteBlocker
│
├── WebsiteBlocker.App
│   └── WPF desktop application
│
├── WebsiteBlocker.Core
│   └── Main blocking and configuration logic
│
└── WebsiteBlocker.Service
    └── Windows background service
```

There is also a separate test project:

```text
tests/
└── WebsiteBlocker.Tests
```

The reason I separated these is so each part has its own job.

The WPF application handles what the user sees and interacts with, the Core project handles the actual logic, and the Service project is responsible for running the blocker in the background.

The basic relationship looks like this:

```text
                 ┌──────────────────────┐
                 │      WPF App         │
                 │  WebsiteBlocker.App  │
                 └──────────┬───────────┘
                            │
                            ▼
                 ┌──────────────────────┐
                 │    Core Library      │
                 │ WebsiteBlocker.Core  │
                 └───────┬───────┬──────┘
                         │       │
                         ▼       ▼
              ┌────────────┐  ┌──────────────┐
              │ Settings   │  │ Blocker      │
              │ Service    │  │ Service      │
              └─────┬──────┘  └──────┬───────┘
                    │                │
                    ▼                ▼
             settings.json      Windows hosts
                                   file
```

The Windows Service uses the same Core project:

```text
WebsiteBlocker.Service
        │
        ▼
WebsiteBlocker.Core
        │
        ▼
Windows hosts file
```

This means I don't have to duplicate the blocking logic in multiple places.

---

### 2. The WPF application

`WebsiteBlocker.App` is the part of the project that the user actually sees.

I'm using **WPF and XAML** for the interface.

The application lets the user:

* Add websites to the block list
* Remove websites
* Enable or disable the blocker
* See which websites are currently blocked
* See whether the blocker is currently enabled

The important thing here is that the WPF application doesn't do everything itself.

For example, if I enter:

```text
youtube.com
```

the button click doesn't directly start editing the hosts file.

Instead, the process is more like:

```text
User enters website
        │
        ▼
Add Website button
        │
        ▼
MainWindow.xaml.cs
        │
        ▼
DomainMatcher
        │
        ▼
ConfigurationService
        │
        ▼
settings.json
        │
        ▼
BlockerService
        │
        ▼
Windows hosts file
```

This makes the application much easier to work on because the UI doesn't need to know every detail about how website blocking works.

---

### 3. WPF and XAML

The graphical interface is built using WPF.

WPF lets me separate the actual interface from the C# code behind it.

For example:

```text
MainWindow.xaml
        │
        │ Defines what the window looks like
        ▼
MainWindow.xaml.cs
        │
        │ Defines what the window does
        ▼
WebsiteBlocker.Core
        │
        │ Handles the actual logic
```

The XAML contains things like buttons, text boxes, lists, labels, colors, and layout.

The C# code handles things like:

```csharp
private void AddWebsite_Click(...)
```

So when the user clicks **Add Website**, the event handler runs and starts the process of adding that website.

---

### 4. Why the application needs Administrator privileges

One of the first problems I had to deal with was permissions.

The Windows hosts file is located at:

```text
C:\Windows\System32\drivers\etc\hosts
```

Windows protects this file because changing it can affect how the computer connects to websites.

Normally, a regular application can't just modify it.

Because Website Blocker needs to edit this file, I added an `app.manifest` to the WPF application.

The manifest contains:

```xml
<requestedExecutionLevel
    level="requireAdministrator"
    uiAccess="false" />
```

This tells Windows that the application needs administrator privileges.

So instead of having to right-click the program and select:

```text
Run as administrator
```

Windows automatically asks for permission when the application starts.

This also means that the application can directly update the hosts file when the user enables or changes the block list.

---

### 5. The Core project

`WebsiteBlocker.Core` is basically the brain of the application.

This is where the important logic lives.

It contains things like:

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

The biggest reason I made a separate Core project is so that the actual blocking logic isn't tied to the WPF interface.

That means the same Core code can be used by:

* The WPF application
* The Windows Service
* Tests
* Future versions of the application

For example, if I eventually replace the WPF interface with another interface, I don't need to completely rewrite how blocking works.

---

### 6. BlockerSettings

`BlockerSettings` is basically the object that stores the application's current configuration.

It contains values such as:

```csharp
public bool Enabled { get; set; }

public List<string> BlockedSites { get; set; }

public bool BlockSubdomains { get; set; }

public bool EnableLogging { get; set; }
```

So instead of having random variables spread throughout the application, the settings are kept together.

Conceptually:

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

This makes it much easier for the rest of the application to know what the current configuration is.

---

### 7. Saving settings with JSON

I didn't want the blocked websites to disappear every time the application closes.

That's why I created `ConfigurationService`.

It handles saving and loading the configuration as JSON.

A simplified version of the configuration looks like:

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

So when the application starts, it loads the existing settings.

If I add or remove a website, the configuration gets updated.

The flow is basically:

```text
Application
     │
     ▼
ConfigurationService
     │
     ▼
settings.json
```

This means I can close the application, open it again later, and still have the same websites in my block list.

---

### 8. DomainMatcher

Another part of the Core project is `DomainMatcher`.

The reason this exists is because users don't always enter websites in exactly the same format.

For example, these are all possible inputs:

```text
youtube.com

www.youtube.com

https://youtube.com

https://www.youtube.com/

https://youtube.com/watch?v=123
```

I don't want the application to treat every one of those as a completely different website.

The DomainMatcher cleans the input and extracts the domain.

For example:

```text
https://www.youtube.com/watch?v=123
```

becomes:

```text
youtube.com
```

This also helps prevent duplicate entries in the block list.

---

### 9. How the actual blocking works

This is probably the most important part of the project.

Website Blocker currently uses the Windows **hosts file** to block websites.

The hosts file allows Windows to map a domain to a specific IP address.

For example:

```text
127.0.0.1 example.com
```

This tells Windows that `example.com` should resolve to:

```text
127.0.0.1
```

instead of the real IP address of the website.

`127.0.0.1` is the computer's own loopback address.

So, simplified, a normal website request looks something like:

```text
Browser
   │
   ▼
DNS lookup
   │
   ▼
Real website server
```

When the domain is in the hosts file:

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

The request is therefore redirected locally instead of resolving to the website's normal server.

---

### 10. How I modify the hosts file

I didn't want the application to rewrite or replace the entire hosts file.

That could potentially mess with other entries already on the computer.

Instead, Website Blocker creates its own section inside the file.

For example:

```text
# WEBSITE_BLOCKER_START
127.0.0.1 youtube.com
127.0.0.1 www.youtube.com
127.0.0.1 reddit.com
127.0.0.1 www.reddit.com
# WEBSITE_BLOCKER_END
```

The two comments:

```text
# WEBSITE_BLOCKER_START
```

and:

```text
# WEBSITE_BLOCKER_END
```

act as markers.

The application knows that everything between those two markers belongs to it.

This is important because when I disable the blocker, I don't want to delete unrelated entries from the hosts file.

---

### 11. What happens when I enable the blocker

When the user presses **Enable Blocker**, the application does several things.

The process is roughly:

```text
Load settings
      │
      ▼
Get blocked websites
      │
      ▼
Remove old Website Blocker entries
      │
      ▼
Add WEBSITE_BLOCKER_START
      │
      ▼
Add blocked domains
      │
      ▼
Add WEBSITE_BLOCKER_END
      │
      ▼
Save hosts file
      │
      ▼
Flush DNS cache
```

For example, if the block list contains:

```text
youtube.com
reddit.com
```

the hosts file will get:

```text
# WEBSITE_BLOCKER_START
127.0.0.1 youtube.com
127.0.0.1 www.youtube.com
127.0.0.1 reddit.com
127.0.0.1 www.reddit.com
# WEBSITE_BLOCKER_END
```

Before doing this, the application removes its previous block section.

This is important when the user changes the list.

For example, if I remove Reddit, I don't want the old Reddit entries to remain in the hosts file.

---

### 12. What happens when I disable it

When the blocker is disabled, the application searches for:

```text
# WEBSITE_BLOCKER_START
```

and:

```text
# WEBSITE_BLOCKER_END
```

It then removes everything between them.

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

After:

```text
127.0.0.1 some-other-entry

127.0.0.1 another-entry
```

Only the entries created by Website Blocker are removed.

---

### 13. Flushing the DNS cache

After changing the hosts file, the application runs:

```text
ipconfig /flushdns
```

Windows can cache DNS information, so simply changing the hosts file doesn't always mean everything immediately starts using the new information.

Flushing the DNS cache tells Windows to clear its cached DNS records.

The basic process is:

```text
Change hosts file
       │
       ▼
Flush DNS cache
       │
       ▼
Windows performs fresh name resolution
```

This helps the new blocking rules take effect immediately.

---

### 14. Why this can work across different browsers

One of the reasons I chose the hosts-file approach is that it isn't specifically tied to Chrome, Firefox, Edge, or another browser.

A browser extension would look more like:

```text
Chrome Extension
      │
      ▼
Chrome
```

while Website Blocker works closer to:

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

Because the hosts file is handled by Windows, the application isn't dependent on one specific browser.

However, this doesn't mean the hosts file is a perfect network-level blocker.

Modern websites can use multiple domains, DNS-over-HTTPS, IPv6, caching, QUIC/HTTP3, and other networking techniques.

For example, blocking:

```text
example.com
```

doesn't automatically mean every other domain used by that website is blocked.

That's one of the biggest limitations of the current version.

---

### 15. The Windows Service

The project also contains:

```text
WebsiteBlocker.Service
```

This is designed to allow the blocker to run in the background as a Windows Service.

The idea is that the graphical application is mainly for managing the block list, while the service can continue maintaining the blocking configuration in the background.

The architecture looks like:

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
   ├── BlockerService
   └── Logger
```

The Worker periodically checks the configuration.

Conceptually:

```text
Start service
     │
     ▼
Load settings
     │
     ▼
Is blocker enabled?
     │
    / \
   /   \
 Yes    No
  │      │
  ▼      ▼
Block   Remove
sites   blocks
  │      │
  └──┬───┘
     │
     ▼
   Wait
     │
     ▼
Check again
```

The service doesn't need to constantly inspect every network connection.

Instead, it makes sure the hosts file matches the current configuration.

---

### 16. Why I separated the Service from the GUI

The GUI and the service have different jobs.

The GUI is for the user:

```text
User
 │
 ├── Add website
 ├── Remove website
 ├── Enable blocker
 └── Disable blocker
```

The service is for background operation:

```text
Windows Service
 │
 ├── Load configuration
 ├── Maintain blocking
 └── Write logs
```

Keeping these separate means I can eventually close the GUI while the background service continues running.

It also gives me a better foundation for adding features later.

---

### 17. Logging

I also added a `Logger` service.

The purpose is pretty simple: if something goes wrong, I want to be able to find out what happened.

The logger records events with timestamps, for example:

```text
[2026-09-28 18:30:12] Website Blocker service started.
```

It can also record errors that happen while the service is running.

This is especially useful for the Windows Service because there isn't always a GUI open where an error message can be displayed.

Instead of silently failing, the service can write the problem to a log file.

---

### 18. Error handling

There are several things that can go wrong with an application like this.

For example:

* The hosts file might not be accessible
* Administrator permissions might not be available
* A configuration file might be invalid
* A filesystem operation might fail
* Something could go wrong while the service is running

Because of that, important operations are wrapped in exception handling.

The basic idea is:

```text
Try operation
      │
      ├── Success
      │     │
      │     ▼
      │   Continue
      │
      └── Error
            │
            ├── GUI → Show error message
            │
            └── Service → Write to log
```

This prevents a single error from immediately crashing the whole application.

---

### 19. Automated tests

The project also has a separate testing project:

```text
tests/
└── WebsiteBlocker.Tests/
```

I'm using **xUnit** for the tests.

One of the areas where testing is particularly useful is domain normalization.

For example, I want these:

```text
example.com
www.example.com
https://example.com
https://www.example.com/
```

to be handled consistently.

Automated tests let me check that behavior whenever I change the code.

The tests mainly target the Core project rather than the WPF interface.

That's another advantage of having the Core logic separated from the UI.

---

### 20. The full process from the user's perspective

If I put the whole thing together, a typical interaction looks like this:

```text
┌──────────────────────────────┐
│ User opens Website Blocker   │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ Windows asks for admin       │
│ permission                   │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ WPF application starts      │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ Load settings.json           │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ User adds example.com        │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ DomainMatcher cleans domain  │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ Save updated settings        │
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
│ the hosts file               │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ DNS cache is flushed         │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ Windows resolves the domain  │
│ to 127.0.0.1                 │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ The browser can't reach the │
│ normal server through that   │
│ hostname                     │
└──────────────────────────────┘
```

So even though the user only sees a button that says **Enable Blocker**, quite a few things happen behind that button.

---

### 21. Why I didn't put everything into one project

It would have been possible to make the whole application inside one project and put everything into a few classes.

I chose not to do that because the project would become harder to maintain as more features are added.

Instead, each part has a specific responsibility:

| Component                | What it does                       |
| ------------------------ | ---------------------------------- |
| `WebsiteBlocker.App`     | WPF user interface                 |
| `WebsiteBlocker.Core`    | Main application logic             |
| `BlockerService`         | Modifies the hosts file            |
| `ConfigurationService`   | Saves and loads settings           |
| `DomainMatcher`          | Cleans and normalizes domains      |
| `Logger`                 | Records events and errors          |
| `WebsiteBlocker.Service` | Runs the blocker in the background |
| `WebsiteBlocker.Tests`   | Tests the application logic        |

This is basically **separation of concerns**.

Instead of one giant class trying to do everything, each part has a specific job.

---

### 22. Current limitations

The current version is still a **V1**.

The hosts-file approach works well as a starting point, but it isn't the same thing as building a complete firewall or DNS filtering system.

Some of the limitations are:

* A website can use multiple domains
* DNS-over-HTTPS can affect hostname resolution
* IPv6 can behave differently
* Browsers can cache information
* QUIC/HTTP3 can change how traffic is handled
* Some applications don't behave like normal web browsers
* Blocking one domain doesn't automatically block every service related to that website

Because of these limitations, the current hosts-file system is more of a foundation for the project.

If I continue developing it, I can eventually move toward a more advanced DNS or network filtering system.

---

### 23. What I want to add later

The current architecture also gives me a good starting point for future features.

Some things I want to experiment with are:

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
    ├── System tray support
    ├── Blocking statistics
    ├── Custom blocked page
    ├── Improved DNS filtering
    ├── Category-based blocking
    ├── Better IPv6 support
    └── More advanced network filtering
```

Because the blocking logic is separated from the UI, these features can be added without having to completely rebuild the project.

---

### 24. In simple terms

If I had to explain the whole project without all the technical details, it works like this:

```text
I add a website
       │
       ▼
The app cleans up the domain
       │
       ▼
The domain is saved to my settings
       │
       ▼
I enable the blocker
       │
       ▼
The app adds the domain to
the Windows hosts file
       │
       ▼
Windows points that domain
to 127.0.0.1
       │
       ▼
The normal website isn't reached
```

The main idea behind the project is simple, but I built it in a way that gives me room to keep expanding it.

The **WPF application** handles the interface, the **Core project** handles the logic, the **hosts file** handles the current blocking method, and the **Windows Service** provides the foundation for background operation.

That separation is probably the most important part of the project because it means I can keep adding features without turning the whole application into one huge file.







