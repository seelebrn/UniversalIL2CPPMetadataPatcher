# UniversalILCPPMetadataPatcher
Well.. it's a (supposedly) universal Metadata Patcher for Unity games built using IL2CPP. Include this in your BepinEx plugin.

0) Requirements : 
  -BepinEx BE Build (here, I'm using the latest BE build, 668 at the time of writing)
  -Reference to LibCpp2IL from Bepinex\core
  -Reference to AssetRipper.VersionUtilities

1) What does it do ?

The implementation shown here is for a translation mod (CH => EN). Basically, you have to call ProcessMetadata(). 
Basically, it will read from global-metadata.dat, match its string literals based on a translation dict, translate the result and rewrite your global-metadata.dat using the translated results. Note that it creates a .temp file in-between. 

Note that it will only trigger rewrite if processbuffer is not null, meaning that at least one line in the metadata corresponds to a key of your dict (i.e. : likely the game has updated or you've reverted your metadata to an unmodified version). Also note that patching metadata at runtime will likely lead to a game crash. Once. After that, you'll be set. 

Credits : 

The "metadata reading and editing part" is built using CPP2IL, from SamboyCoding. 
The whole "overwriting metadata" part is directly taken from JeremieCHN's code for https://github.com/JeremieCHN/MetaDataStringEditor. 

All credits to that person !  

Have fun <3
