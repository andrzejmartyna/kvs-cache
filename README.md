# Introduction

kvs-cache is a command line tool for **browsing Azure Key Vault secrets** as quickly and as easily as possible.  
Information about secrets, once cached locally, can be instantly browsed.  
Pressing Enter on a given secret calls Azure to get the secret value and copy it to the clipboard.  
Note that:
- secret values are not cached locally nor displayed on the screen
- you can get the clipboard contents and paste it wherever you need

kvs-cache is written using .NET 7 and C# 11.  

# Usage

1. Clone the repo
1. Execute a command line / shell window
1. Check if you have .NET 7: `dotnet --version`, if not, **install .NET 7 SDK first**
1. Go to the repo folder in the command line / shell window
1. Login to Azure `az login`
1. Go down one more folder `cd kvs-cache`
1. Restore packages using `dotnet restore` 
1. Run kvs-cache using `dotnet run`

# General functionalities

1. Works as a console application
1. Information browsing / searching
   - At start it finds all Azure subscriptions you can access
   - Allows to browse and drill down/up through subscriptions, key vaults, and secrets 
   - Allows for instant filtering by entering word or words to be searched for
1. Caching
   - Caches places you visit for immediate access next time
   - Displays age of cache
   - Allows to refresh current level (Ctrl-R)
1. Get secret values
   - Get value of a selected secret and copy it to clipboard

# More information

- [Licenses of libraries used by KvsCache (thank you to all authors and contributors!)](LICENSES/LICENSES.md)
- [Licence of KvsCache](LICENSE)
- [ReleaseNotes](ReleaseNotes.md)
- [TO DO](TODO.md)
