# Introduction

kvs-cache is a command line tool for **browsing Azure Key Vault secrets** as quickly and as easily as possible.  
Information about secrets, once cached locally, can be instantly browsed.  
Pressing Enter on a given secret calls Azure to get the secret value and copy it to the clipboard.  
Note that:
- secret values are not cached locally nor displayed on the screen
- you can get the clipboard contents and paste it wherever you need the secret value

kvs-cache is written using .NET 7 and C# 11.  

# License

kvs-cache is distributed under MIT license.  
More over, this is my first tool exposed through GitHub, so you should take "as-is" even more seriously as I may not follow many good practices of open source yet.  
Apologize for any inconveniences.    
You have been warned.  

# Usage

1. Clone the repo
1. Execute a command line / shell window
1. Check if you have .NET 7: `dotnet --version`, if not, **install .NET 7 SDK first**
1. Go to the repo folder in the command line / shell window
1. Login to Azure `az login`
2. Go down one more folder `cd kvs-cache`
1. Run kvs-cache using `dotnet run`

# General functionalities

1. Works as a console application
1. Finds all Subscriptions, Key vaults and secrets accessible
1. Caches the information for immediate browsing
1. Browsing and drill down/up through Subscriptions, Key Vaults, and Secrets
1. Instant filter by entering word or words to be searched for
1. Get value of a selected secret and copy it to clipboard

# More information

- [ReleaseNotes.md](ReleaseNotes.md)
- [TODO.md](TODO.md)
