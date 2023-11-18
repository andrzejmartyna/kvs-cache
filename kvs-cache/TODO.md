# MUST

- refresh - asynchronous on demand + progress indicator
- asynchronous progress indicator while acquiring credentials
- fix Json produced
- seamless Azure Credentials inside docker container
- ReleaseNotes.md
- README.md
- Code cleanup
- version

# SHOULD

- Ctrl-D to full info about Subscription(s)/KeyVault/Secret - only then put it officially in documentation
- UX - F1/Help
- corner cases handled
- URLs to Azure Portal resources in output Json
- show age of cache

# COULD

- parametrize app settings
- documentation in Sphinx
- unit tests
- model tests

# WILL NOT

- search through all objects (probably very good, useful instant filter is more than enough for kvs-cache)
