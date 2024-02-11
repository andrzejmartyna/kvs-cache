# DONE

- N/A

# MUST 2.3

- consider how to approach clipboard securely - should it be cleaned on exit or after some time? How e.g. 1Password deals with password put in clipboard?
- distribution as a ready to use, compiled tool (currently it requires user to install .NET SDK and build the tool)
- provide CHANGELOG.md, versioning and tags as good projects do
- move this TODO file to "Issues" GitHub functionality

# SHOULD

- better handle situations when user is not logged in to Azure (instead of current red exception on the whole application box)
- work from inside Docker including seamless Azure credentials handling
- auto write/save cache periodically
- BIG: operation/context menu on secret/KV/subscription
  - base64
    - differentiate message based on assessment how likely the secret is base64?
      - probably something simple - like based on length: <10 probably not, <20 might not, <50 may be, <100 probably, >= 100 most probably
      - of course in all cases secret should be decoded but the message should **better prepare user for false positives**
  - Describe - full info in Json about Subscription(s)/KeyVault/Secret
  - Generate command to get/set value using scripts like Powershell, bash
- BIG: Recent
- BIG: Favorites
- Issues
  - Issue: Ctrl-C does not work if a lengthy operation that queries Azure is under way
  - Issue: Ctrl-R hangs sometimes
  - Issue: Ctrl-C on the message does not work immediately but after exiting the message using another key

# COULD

- BIG: implement kvs-cache as an extension to VS Code
- parametrize app settings
- Quality
  - unit tests
  - BIG: model tests
- UX - F1/Help and/or documentation in Sphinx
- operations
  - enter/browse versions of a secret
  - set an environment variable (as an alternative to clipboard)
- presentation
  - when a subscription or a key vault does not contain items display "(no items)"
  - when error happened while querying list of key vaults or secrets display "(error)" on an item containing the list
  - update age of cache periodically
  - distinguish cached items visually (would it be useful?)
- Open Source
  - clarify Feedback / contribution / communication
    - is my e-mail accessible?
    - is this valid: [this way](https://stackoverflow.com/a/49277449/669692)?
    - these looks like good guidance:
      - [https://github.com/andrzejmartyna/kvs-cache/community](https://github.com/andrzejmartyna/kvs-cache/community)
      - [Karl Fogel, Producing Open Source Software - How to Run a Successful Free Software Project](https://producingoss.com/)
  - functionality to display list of licenses of third party libraries used
  - verify if all licenses should be copied and included in my repo - if so, update [LICENSES.md](LICENSES/LICENSES.md)
- BIG: search through all objects (I bet instant filtering is more than enough for kvs-cache)
- C#: consider deconstruction instead of OneOf dependency

# WILL NOT

- Set/change anything e.g. secret value
  - This is because kvs-cache is a purely read-only tool
  - The best kvs-cache can do to support changes is to direct user to Azure Portal or generate code to change something on your own
