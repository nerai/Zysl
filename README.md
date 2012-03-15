Zysl: Super easy persistance for .NET

Zysl is an extremely simple to use persistent data store. It is able to quickly store and load any serializable class written in .NET. Zysl is zero-configuration: If you don’t feel like it, you won’t have to deal with setup at all. Unlike regular databases, Zysl stores its data in separate files per entry.

To use it, you first decide on where to store your data. Typically, you’ll use a file based store like this:

    var backing = new FileStore ("myStore");

Then you create the mapper from your classes to the store:

    var store = new KVStore<string, MyClass> (backing);

And setup is complete already! You can use the store just like a regular dictionary:

    store["some id"] = new MyClass ();
    Console.WriteLine (store["some id"].ToString ());

This should be sufficient for simple use cases,  but if you need more, Zysl offers quite a few options:

* Create RAM-based caches to avoid frequent disk access (by default there is no caching).
* Or use local files as cache for remote FTP storage.
* If you’re afraid of storing all persistence files in a single folder, have Zysl split it up into subfolders automatically.
* Accessing the store from multiple threads? You won’t have to deal with transactions, simple use the provided thread-safe wrapper.
* You thought file storage is not atomic? With Zysl, it is, regardless of the technology and file system of the backing store. Why, yes, it’s magic.
