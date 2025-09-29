Create a bash script that usign grep extracts the value of each <Key></Key> tags and store it in a new file links.txt

The following is an example of the content of the xml file
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