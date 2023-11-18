# MUST 1.1

- handle PageUp/PageDown
- lazy loading, cache and refresh fragments independently
- operation/context menu on secret/KV/subscription
  - Set secret value
  - Get/set secret value using base64
  - full info in Json about Subscription(s)/KeyVault/Secret
    - URLs to Azure Portal resources in output Json
  - Generate command to get/set value using scripts like Powershell, bash
- corner cases handled (string too long)
- code cleanup
- ready to use distribution (1.0 requires .NET SDK and building)
- provide CHANGELOG.md, versioning and tags as good projects do
- clarify Feedback / contribution / communication
  - is my e-mail accessible?
  - is this valid: [this way](https://stackoverflow.com/a/49277449/669692)?
- handle Azure exception gracefully and inform the user (for example if she/he does not have permissions to List/Get secret(s))

# SHOULD

- parametrize app settings
- work from inside Docker including seamless Azure credentials handling
- show age of cache
- fix bug: Ctrl-C on the message does not work immediately but after exiting the message using another key
- replace places with old style cast of objects to Subscription/KeyVault/Secret

# COULD

- enter/browse versions of a secret
- Recent
- Favorites
- UX - F1/Help and/or documentation in Sphinx
- unit tests
- model tests
- implement kvs-cache as an extension to VS Code

# WILL NOT

- search through all objects (I bet instant filtering is more than enough for kvs-cache)
