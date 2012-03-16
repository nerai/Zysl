Zysl: Super easy persistance for .NET
=

Zysl is an extremely simple to use persistent data store. It is able to quickly store and load any serializable class written in .NET (typically using DataContract attributes, but blank classes are fine, too).

Zysl is zero-configuration: If you don’t feel like it, you won’t have to deal with setup at all. On the other hand, configuration for advanced use cases is still simple.

Unlike regular databases, Zysl stores its data in a separate file per row. Since the files are XML-based, you'll easily be able to check or edit stored values. Exporting data is as simple as copying the folder.

Usage
-

To use it, first decide on where to store your data. Typically, you’ll use a file based store like this:

    var backing = new FileStore ("data/myStore");

Then, create the mapper from your classes to the store:

    var store = new KVStore<string, MyClass> (backing);

And you're all set already! You can use the store just like a regular .NET dictionary:

    store["some id"] = new MyClass ();
    Console.WriteLine (store["some id"].ToString ());

Possibilities
-

The earlier example should be sufficient for simple use cases,  but if you need more, Zysl offers quite a few options:

* Create RAM-based caches to avoid frequent disk access (by default there is no caching).
* Or use local files as cache for remote FTP storage.
* If you’re afraid of storing all persistence files in a single folder, have Zysl split it up into subfolders automatically.
* Accessing the store from multiple threads? You won’t have to deal with transactions, simple use the provided thread-safe wrapper.
* You thought using file storage as backend is not atomic? With Zysl, it is - regardless of the technology and file system of the backing store. Be it NTFS, FAT or a remote SMB or FTP share. Why, yes, it’s magic.

Strengths and weaknesses
-

You should consider using Zysl when:
* You want to avoid steep learning curves and SQL
* Keeping things simple and controllable is worth more than having tons of detailed options
* The data won't be used in advanced queries (filtering, ordering, joining etc.)
* You do not expect extremely high data throughput
