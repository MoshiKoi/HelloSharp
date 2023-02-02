Small hello world implementation using [LLVMSharp](https://github.com/dotnet/LLVMSharp)

Running (via `dotnet run`) produces `out.o`, which is then be linked with the standard library by invoking `lld-link`. The resulting executable can then be run.

You can also pass in `jit` to run a just-in time engine after compilation, e.g. `dotnet run -- jit`