using LLVMSharp.Interop;

// Setup context, etc
using var context = LLVMContextRef.Create();
using var module = context.CreateModuleWithName("main");
using var builder = context.CreateBuilder();

// Create the puts function
var putsRetTy = context.Int32Type;
var putsParamTys = new LLVMTypeRef[] {
    LLVMTypeRef.CreatePointer(context.Int8Type, 0)
};

var putsFnTy = LLVMTypeRef.CreateFunction(putsRetTy, putsParamTys);
var putsFn = module.AddFunction("puts", putsFnTy);

// Create the main function
var mainRetTy = context.VoidType;
var mainParamTys = new LLVMTypeRef[] { };
var mainFnTy = LLVMTypeRef.CreateFunction(mainRetTy, mainParamTys);
var mainFn = module.AddFunction("main", mainFnTy);
var mainBlock = mainFn.AppendBasicBlock("entry");

// Create the body of the main function
builder.PositionAtEnd(mainBlock);
var message = builder.BuildGlobalStringPtr("Hello, World!");
builder.BuildCall2(putsFnTy, putsFn, new LLVMValueRef[] { message }, "");
builder.BuildRetVoid();

Console.WriteLine($"LLVM IR\n=========\n{module}");

// Initialize LLVM
LLVM.InitializeAllTargetInfos();
LLVM.InitializeAllTargets();
LLVM.InitializeAllTargetMCs();
LLVM.InitializeAllAsmParsers();
LLVM.InitializeAllAsmPrinters();

var triple = LLVMTargetRef.DefaultTriple;

Console.WriteLine($"Targeting {triple}");

// Create the target machine
var target = LLVMTargetRef.GetTargetFromTriple(triple);
var targetMachine = target.CreateTargetMachine(
    triple, "generic", "",
    LLVMCodeGenOptLevel.LLVMCodeGenLevelNone,
    LLVMRelocMode.LLVMRelocDefault,
    LLVMCodeModel.LLVMCodeModelDefault);

var outFile = "out.o";

targetMachine.EmitToFile(module, outFile, LLVMCodeGenFileType.LLVMObjectFile);
Console.WriteLine($"Compiled to {outFile}");

// Can also directly set the module triple
// module.Target = triple;
// module.WriteBitcodeToFile("out2.o");

// Run with the Just-In Time engine
if (args.Contains("jit"))
{
    var engine = module.CreateExecutionEngine();
    var main = module.GetNamedFunction("main");
    engine.RunFunctionAsMain(main, 0, Array.Empty<string>(), Array.Empty<string>());
}

using var linkProcess = System.Diagnostics.Process.Start("lld-link", new[] { outFile, "/defaultlib:libcmt" });

await linkProcess.WaitForExitAsync();
Console.WriteLine($"Linked with standard library");