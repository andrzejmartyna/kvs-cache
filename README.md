# Introduction

kvs-cache is a command line tool for **browsing Azure Key Vault secrets** as quickly and as easily as possible.  
Information about secrets, once cached locally, can be instantly browsed.  
Pressing Enter on a given secret calls Azure to get the secret value and copy it to the clipboard.  
Note that:
- secret values are not cached locally nor displayed on the screen
- you can get the clipboard contents and paste it wherever you need

kvs-cache is written using .NET 7 and C# 11.  

## Important known issue in 1.0

For version 1.0 I made a design mistake to cache all information upfront.  
That's why if you have access to hundreds or thousands secrets you need to wait several minutes at first run or more...  (though if you eventually get it cached the speed of local browsing is brilliant :).  
I plan to introduce lazy loading in next version but before that I disabled automatic refresh not to irritate anyone.   

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
1. Finds all subscriptions, key vaults and secrets accessible
1. Caches the information for immediate browsing
1. Browsing and drill down/up through subscriptions, key vaults, and secrets
1. Instant filter by entering word or words to be searched for
1. Get value of a selected secret and copy it to clipboard

# More information

- [ReleaseNotes.md](ReleaseNotes.md)
- [TODO.md](TODO.md)
