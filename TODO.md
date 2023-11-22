# MUST 1.1

- rename folders and namespaces to avoid kvs_cache
- lazy loading, cache and refresh fragments independently
  - handle Azure exception gracefully and inform the user (for example if she/he does not have permissions to List/Get secret(s))
  - secrets found (XXX):
  - when a subscription or a key vault does not contain items display "(no items)"
  - when error happened while querying list of key vaults or secrets display "(error)" on an item containing the list
- operation/context menu on secret/KV/subscription
  - Set secret value
  - Get/set secret value using base64
  - full info in Json about Subscription(s)/KeyVault/Secret
    - URLs to Azure Portal resources in output Json
  - Generate command to get/set value using scripts like Powershell, bash
- clipboard
  - is it possible to get clipboard and compare? 
  - fix clipboard issues
    - https://stackoverflow.com/questions/44205260/net-core-copy-to-clipboard
- ready to use distribution (1.0 requires .NET SDK and building)

# SHOULD

- corner cases handled (string too long)
- code cleanup
- provide CHANGELOG.md, versioning and tags as good projects do
- clarify Feedback / contribution / communication
  - is my e-mail accessible?
  - is this valid: [this way](https://stackoverflow.com/a/49277449/669692)?
- work from inside Docker including seamless Azure credentials handling
- show age of cache
- fix bug: Ctrl-C on the message does not work immediately but after exiting the message using another key
- Esc - first press should clean the filter

# COULD

- replace places with old style cast of objects to Subscription/KeyVault/Secret
- parametrize app settings
- enter/browse versions of a secret
- Recent
- Favorites
- UX - F1/Help and/or documentation in Sphinx
- unit tests
- model tests
- implement kvs-cache as an extension to VS Code

# WILL NOT

- search through all objects (I bet instant filtering is more than enough for kvs-cache)
