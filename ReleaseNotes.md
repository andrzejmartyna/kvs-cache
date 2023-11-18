# Future

A rough plan for future versions is here: [TODO.md](TODO.md)

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

_The version 1.0 was made from 2023-10-28 to 2023-11-18._  
_Working hours spent: 34._
