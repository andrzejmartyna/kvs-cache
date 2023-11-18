# MUST 1.0

- refresh - asynchronous on demand with progress indicator
- refresh - asynchronous on startup while able to use cache with progress indicator
- fix Json produced
- README.md

# SHOULD

- parametrize app settings
- Set secret value
- Get/set secret value using base64
- Ctrl-D to full info about Subscription(s)/KeyVault/Secret - only then put it officially in documentation
  - URLs to Azure Portal resources in output Json
- Code cleanup
- seamless Azure Credentials inside docker container
- corner cases handled (string too long)

# COULD

- Generate command to get/set value using scripts like Powershell, bash
- show age of cache
- enter/browse versions of a secret
- Recent
- Favorites
- UX - F1/Help and/or documentation in Sphinx
- unit tests
- model tests
- implement kvs-cache as an extension to VS Code

# WILL NOT

- search through all objects (probably very good, useful instant filter is more than enough for kvs-cache)
