# Project Analysis Task

## Required MCP Servers
- playwright: For getting information from the internet

## Task Description
1. Download https://vardefulla-datamangder.bolagsverket.se/arsredovisningar-bulkfiler?prefix=arsredovisningar/ using curl
2. If the file contains a XML tag NextMarker which values contains the next page that havn't been downloaded use curl to download that file by appending constructing an url https://vardefulla-datamangder.bolagsverket.se/arsredovisningar-bulkfiler?prefix=arsredovisningar/&marker=[NextMarkerValue] 
the NextMarker value should only be used once.
3. The downloaded files are XML files of the following structure
```xml
<ListBucketResult xmlns="http://s3.amazonaws.com/doc/2006-03-01/">
<Name>bulkfil-paketering-zipfiler-prod</Name>
<Prefix>arsredovisningar/</Prefix>
<Marker/>
<NextMarker>arsredovisningar/2025/26_2.zip[minio_list:v2,return:]</NextMarker>
<MaxKeys>1000</MaxKeys>
<IsTruncated>true</IsTruncated>
<Contents>
<Key>arsredovisningar/2020/01_1.zip</Key>
<LastModified>2025-02-07T09:11:31.713Z</LastModified>
<ETag>"ab954aceec8537c7b3460d3a30f58f94-7"</ETag>
<Size>31566336</Size>
<Owner>
<ID>02d6176db174dc93cb1b899f7c6078f08654445fe8cf1b6ce98d8855f66bdbf4</ID>
<DisplayName>minio</DisplayName>
</Owner>
<StorageClass>STANDARD</StorageClass>
</Contents>
....
´´´

4. Parse from the structure extract all keys and place the in a links.txt file.
