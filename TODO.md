# DONE

- handle Azure exceptions gracefully and inform the user (for example if she/he does not have permissions to List/Get secret(s))
- secrets found (XXX):
- Esc - first press should clean the filter

# MUST 2.1

- when a subscription or a key vault does not contain items display "(no items)"
- when error happened while querying list of key vaults or secrets display "(error)" on an item containing the list
- operation/context menu on secret/KV/subscription
  - Set secret value
  - Get/set secret value using base64
  - full info in Json about Subscription(s)/KeyVault/Secret
    - URLs to Azure Portal resources in output Json
  - Generate command to get/set value using scripts like Powershell, bash
  - or instead of everything above - open browser with the proper url - let the user do anything there
- ready to use distribution (1.0 requires .NET SDK and building)
- Ctrl-R hangs sometimes

# SHOULD

- Issue: Ctrl-C does not work if a lengthy operation that queries Azure is under way
- consider deconstruction instead of OneOf dependency
- corner cases handled (string too long)
- code cleanup
- work from inside Docker including seamless Azure credentials handling
- show age of cache (and update periodically)
- fix bug: Ctrl-C on the message does not work immediately but after exiting the message using another key
- auto write/save cache periodically 
- Recent
- Favorites

# COULD

- provide CHANGELOG.md, versioning and tags as good projects do
- clarify Feedback / contribution / communication
  - is my e-mail accessible?
  - is this valid: [this way](https://stackoverflow.com/a/49277449/669692)?
- functionality to display list of licenses of third party libraries used
- replace places with old style cast of objects to Subscription/KeyVault/Secret
- parametrize app settings
- enter/browse versions of a secret
- UX - F1/Help and/or documentation in Sphinx
- unit tests
- model tests
- implement kvs-cache as an extension to VS Code
- set an environment variable (as an alternative to clipboard)
- search through all objects (I bet instant filtering is more than enough for kvs-cache)

# WILL NOT

- N/A
