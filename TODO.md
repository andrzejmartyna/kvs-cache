# MUST 2.1

- ready to use distribution (1.0 requires .NET SDK and building)
- operation/context menu on secret/KV/subscription
  - Set secret value
  - Get/set secret value using base64
  - full info in Json about Subscription(s)/KeyVault/Secret
    - URLs to Azure Portal resources in output Json
  - Generate command to get/set value using scripts like Powershell, bash
  - or instead of everything above - open browser with the proper url - let the user do anything there
- Issue: Ctrl-C does not work if a lengthy operation that queries Azure is under way
- Issue: Ctrl-R hangs sometimes
- Issue: Ctrl-C on the message does not work immediately but after exiting the message using another key

# SHOULD

- Recent
- Favorites
- work from inside Docker including seamless Azure credentials handling
- auto write/save cache periodically 
- provide CHANGELOG.md, versioning and tags as good projects do
- parametrize app settings

# COULD

- enter/browse versions of a secret
- unit tests
- model tests
- UX - F1/Help and/or documentation in Sphinx
- set an environment variable (as an alternative to clipboard)
- implement kvs-cache as an extension to VS Code
- when a subscription or a key vault does not contain items display "(no items)"
- when error happened while querying list of key vaults or secrets display "(error)" on an item containing the list
- clarify Feedback / contribution / communication
  - is my e-mail accessible?
  - is this valid: [this way](https://stackoverflow.com/a/49277449/669692)?
- functionality to display list of licenses of third party libraries used
- search through all objects (I bet instant filtering is more than enough for kvs-cache)
- update age of cache periodically
- consider deconstruction instead of OneOf dependency
- distinguish cached items visually (would it be useful?)

# WILL NOT

- N/A
