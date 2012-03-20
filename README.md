Zysl: Super easy persistance for .NET
=

Zysl is an extremely simple to use persistent data store. It is able to quickly store and load any serializable class written in .NET.

Zysl is zero-configuration: If you don’t feel like it, you won’t have to deal with setup at all. On the other hand, configuration for advanced use cases is still simple.

Unlike regular databases, Zysl stores its data in a separate file per row. Since the files are XML-based, it is possible to easily check or edit stored values manually. Exporting data is as simple as copying the folder.

Usage
-

Let's start with the easiest way to use it.

    var store = new KVStore<int, string>();
    store[42] = "test";
    Console.WriteLine (store[42]);

Pretty simple, huh? Actually, a few things happened in the background automatically: Zysl _created a backing store_ in your current working directory, _named it_ according to the types it can hold (int and string, in this case), then automatically _serialized_ the data you put in the store and _deserialized_ them next instant.

You like to have more control? I'm glad you asked!

Let's assume you want to store the data in a different place.

    var backing = new FileStore ("./data/myStore");

Instead of a local path, this can be a network share as well. You will need permission to create, read, write and delete files in there.

The FileStore is a very simple class that only stores blobs of binary data with a string key. Since we want to use Zysls automatic serialization features, we need to create a mapper from your classes to the store:

    var store = new KVStore<string, MyClass> (backing);

And you're all set! You can use the store just like a regular .NET dictionary:

    store["some id"] = new MyClass ();
    Console.WriteLine (store["some id"].ToString ());

If performance is important, you'll want to make use of caching. Let's assume you like to have some data cached in RAM with the local file system as backing store.

    var backing = new GenericBackedCache (
        new DictionaryStore (),
        new FileStore ("./myBackingStore"));
    var store = new KVStore<string, DateTime> (backing);

As you can see, we set up a cached store using a simple RAM cache in combination with the local file store we looked at earlier. Instead of the local backend, you could also connect to a remote share, or to an FTP server. Or any combination of as many of those as you like.

By the way, to limit the number of elements the cache is allowed to hold at any time, simply use

    backing.MaxSize = 10000;

The default is 1024. The cache is pretty smart and will let go of rarely accessed elements but hold on to the commonly used ones.

Possibilities
-

* Set up a store and replace your existing dictionary store in 1 line of code.
* Automatic serialization of complex classes, optionally easy to control via DataContract.
* Create RAM-based caches to avoid frequent disk access (by default there is no caching), or use local files as cache for remote FTP storage.
* Accessing the store from multiple threads? You won’t have to deal with transactions, simple use the provided thread-safe wrapper.
* You thought file storage as backend is not reliable? With Zysl, it is - regardless of the technology and file system of the backing store. Be it NTFS, FAT or a remote SMB or FTP share. Why, yes, it’s magic.
* Extend the ecosystem easily - the API is lean and simple to implement. For instance, connect Zysl to your legacy system as a data source.

Strengths and weaknesses
-

You should consider using Zysl when:

* You want to avoid steep learning curves and SQL
* Keeping things simple is worth more than having tons of detailed options
* The data won't be used in advanced queries (filtering, ordering, joining etc.)
* You do not expect very high data throughput
