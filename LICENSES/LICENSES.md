The following libraries are used by KvsCache.  
Thank you to all authors and contributors for your great work!  
All dependencies in case I missed any: [https://github.com/andrzejmartyna/kvs-cache/network/dependencies](https://github.com/andrzejmartyna/kvs-cache/network/dependencies)

1. Azure.Security.KeyVault.Secrets and Azure.ResourceManager
   - **WHY:** obviously, that's mandatory to deal with Azure secrets and other resources

2. Azure.Identity
   - **WHY:** for **fantastic** simplification of dealing with existing identities from different sources including **great default** DefaultAzureCredential which *just works* for many cases

3. OneOf
   - **WHY:** to provide [tagged/discriminated unions/variants](https://en.wikipedia.org/wiki/Tagged_union) for compile time safety of such a pattern
     - Found on SO: [https://stackoverflow.com/a/39035555/669692](https://stackoverflow.com/a/39035555/669692)
   - Source code: [https://github.com/mcintyre321/OneOf](https://github.com/mcintyre321/OneOf)
     - Source license: [https://github.com/mcintyre321/OneOf/blob/master/licence.md](https://github.com/mcintyre321/OneOf/blob/master/licence.md)
     - Copy of the MIT license downloaded on 2023-12-02: [OneOf/licence.md](OneOf/licence.md)

4. TextCopy
   - **WHY:** because .NET does not provide clipboard operations and it is a challenge to do it yourself for multiple platforms
     - Found on SO: [https://stackoverflow.com/a/51912933/669692](https://stackoverflow.com/a/51912933/669692)
   - Source code: [https://github.com/CopyText/TextCopy](https://github.com/CopyText/TextCopy)
     - Source license: [https://github.com/CopyText/TextCopy/blob/main/license.txt](https://github.com/CopyText/TextCopy/blob/main/license.txt)
     - Copy of the MIT license downloaded on 2023-12-05: [TextCopy/license.txt](TextCopy/license.txt)

5. Newtonsoft.Json
   - **WHY:** because it's the best JSON handling library, period
