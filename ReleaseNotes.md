# Future

A rough plan for future versions is here: [TODO.md](TODO.md)

# 2.2

1. NEW: "Go To Portal" function using ">" which opens default browser with Azure Portal (the URL is copied into the clipboard too)
1. FIXED: long secret names take too much space - they distort right edge of the application box
1. IMPROVED: error handling reading and backuping the cache and its schema version

# 2.1
 
1. NEW: Identify base64 encoded value and decode

# 2.0.1

1. FIXED: Refresh ends with NullReferenceException if a list is empty due to filtering

# 2.0

1. Important change in caching
   - reading all subscriptions / key vaults / secrets at once was withdrawn as it took long minutes for a big set of items and slow network
   - instead what is cached in this version is only the contents a user visits
   - reload (Ctrl-R) is only done on the current level instead of clearing and rereading the whole cache
   - it means also that statistics displays numbers of items cached instead of all items 
1. Usage of open source third-party libraries
   - OneOf to provide [tagged/discriminated unions/variants](https://en.wikipedia.org/wiki/Tagged_union) for compile time safety of such a pattern
   - TextCopy because .NET does not provide clipboard operations and it is a challenge to do it yourself for multiple platforms
   - see [LICENSES.md](LICENSES/LICENSES.md) for details
1. Improvements in error handling, browsing experience, information provided
1. Technical stuff
   - code cleanup
   - started to use git flow

_The version 2.0 has been made in 14 days, in November, December 2023. Time spent: 24h._  
_Other 2.x versions made in 8 days, in December 2023 - February 2024. Time spent: 8h (2.1 - 3h, 2.2 - 5h)._

# 1.0

This initial version of the tool:

1. Works as a console application
1. Finds all Subscriptions, Key vaults and secrets accessible
   - you must be logged into your Azure account
   - the information is cached for reuse on next kvs-cache runs
   - total number of objects found is displayed
1. Browse
   - browse lists of Subscriptions / Key Vaults / Secrets
   - drill down/up throughout hierarchy
   - instant filter lists by entering word(s) to be searched for
1. Get value of a secret and copy it to clipboard
1. Cache
   - the first run takes longer due to building the cache for the first time
   - every next run starts immediately by using existing cache
   - information is displayed of how old the cache is
   - there are a few ways to refresh
     - by pressing Ctrl-R
     - by removing the cache file and rerun kvs-cache
     - automatically after hard coded value of 24h

_The version 1.0 has been made in 19 days, in October, November 2023. Time spent: 34h._  
