# DONE

- Issues fixed
  - Issue: Refresh ends with NullReferenceException if a list is empty due to filtering 
- NEW
  - Identify base64 encoded value and decode

# MUST 2.1

- BIG: operation/context menu on secret/KV/subscription
  - Set secret value
  - Get/set secret value using base64
  - Describe - full info in Json about Subscription(s)/KeyVault/Secret
    - URLs to Azure Portal resources in output Json
  - Generate command to get/set value using scripts like Powershell, bash
  - or instead of everything above - open browser with the proper url - let the user do anything there
- Issues
  - Issue: Ctrl-C does not work if a lengthy operation that queries Azure is under way
  - Issue: Ctrl-R hangs sometimes
  - Issue: Ctrl-C on the message does not work immediately but after exiting the message using another key
- ready to use distribution (1.0 requires .NET SDK and building)

# SHOULD

- BIG: Recent
- BIG: Favorites
- work from inside Docker including seamless Azure credentials handling
- auto write/save cache periodically 
- provide CHANGELOG.md, versioning and tags as good projects do
- parametrize app settings

# COULD

- BIG: implement kvs-cache as an extension to VS Code
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
    - this looks like a good github guidance: [https://github.com/andrzejmartyna/kvs-cache/community](https://github.com/andrzejmartyna/kvs-cache/community)
  - functionality to display list of licenses of third party libraries used
- BIG: search through all objects (I bet instant filtering is more than enough for kvs-cache)
- consider deconstruction instead of OneOf dependency

# WILL NOT

- N/A
