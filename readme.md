Small hello world implementation using [LLVMSharp](https://github.com/dotnet/LLVMSharp)

Running (via `dotnet run`) produces `out.o`, which can then be linked and run. You can also pass in `jit` to run a just-in time engine after compilation, e.g. `dotnet run -- jit`