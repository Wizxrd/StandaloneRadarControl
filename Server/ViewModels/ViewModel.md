# ViewModels
There is one `ViewModel` per `View`.

There are multiple `Models` per `ViewModel`.

`Views` ONLY bind to `ViewModels`.

If something in a `View` needs to be Bound to a value present in a `Model`,
an `Interface` should be used in a `ViewModel` to proxy it.