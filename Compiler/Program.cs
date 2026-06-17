using ASTPasses;
using CodeGen;
using Errors;
using IR;
using Lexing;
using Parsing;
using System.Diagnostics;
using System.Runtime.InteropServices;


string helpMessage =
"Usage: typn [help | INPUT_PATH] [ARGS]"
+ "\nOptions:"
+ "\n-debug              forces compiler to output Tokens,AST,IR."
+ "\n-o [FILE_PATH]      specifies bytecode output file."
+ "\n-run                executes bytecode after compilation.";

string path;
string outputName = "out.tpc"; //Default output name
bool debugMode = false; //Output more information during compilation
bool doRun = false; //Start VM after compiling


if (args.Length == 0)
{
    ErrorHandler.ThrowCLIError("No arguments. Type 'help' for help.");
}

for (int i = 0; i < args.Length; i++)
{
    string arg = args[i];

    if (arg == "help")
    {
        Console.WriteLine(helpMessage);
    }
    else if (arg == "-help")
    {
        Console.WriteLine(helpMessage);
    }
    else if (arg == "--help")
    {
        Console.WriteLine(helpMessage);
    }
    else if (arg == "-debug")
    {
        debugMode = true;
    }
    else if (arg == "-run")
    {
        doRun = true;
    }
    else if (arg == "-o")
    {
        if (i + 1 < args.Length)
        {
            outputName = args[i + 1];
        }
        else
        {

            ErrorHandler.ThrowCLIError("Malformed argument. Expected file path after '-o'.");

        }
    }
}


if (!File.Exists(args[0]))
{
    ErrorHandler.ThrowCLIError($"File '{args[0]}' was not found. Type 'help' for help.");
}

path = args[0];

Tokenizer t = new(File.ReadAllText(path));

List<Token> tokens = t.MakeTokens();


if (debugMode)
{
    foreach (Token tok in tokens)
    {
        Console.WriteLine(tok);
    }
}

Parser p = new(tokens);
List<ASTNode> nodes = p.ParseModule();
// 2 <=
if (debugMode)
{
    Console.WriteLine("--- AST ---");

    foreach (ASTNode node in nodes)
    {
        node.Show(0);
    }
}

SemanticAnalizer general = new();
SymbolResolver symbResolver = new();
TypeResolver typeResolver = new();


foreach (ASTNode node in nodes)
{
    node.AcceptVisitor(general);
    node.AcceptVisitor(symbResolver);
}

foreach (ASTNode node in nodes)
{
    node.AcceptVisitor(typeResolver);
}

if (debugMode)
{
    symbResolver.DisplayScopes();
}


if (ErrorHandler.HasErrors())
{
    ErrorHandler.ThrowAll();
}



IRBuilder builder = new();

IRGeneratePass irGen = new(builder);


foreach (ASTNode node in nodes)
{
    node.AcceptVisitor(irGen);
    //node.MakeInstruction(builder);
}

builder.EndFunction();

if (debugMode)
{
    Console.WriteLine("--- Stage 1 IR ---");

    builder.ShowInstructions();
}

if (debugMode)
{
    Console.WriteLine("--- Code Gen ---");
}

CodeGenerator generator = new(builder.GetInstructions());
byte[] bytecode = generator.Generate().ToArray();

if (debugMode)
{
    Console.WriteLine("--- Done ---");
}

try
{
    File.WriteAllBytes(outputName, bytecode);
}
catch//(IOException e)
{
    ErrorHandler.ThrowCLIError($"Failed to write bytecode. Does output path '{outputName}' exist?");
}


string vmPath = "/Runtime/vm.exe";
if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    vmPath = "./Runtime/vm";
}

if (!Path.Exists(vmPath))
{
    ErrorHandler.ThrowCLIError($"Runtime path: {vmPath} not found.");
}

//Starting VM
if (doRun)
{
    string programPath = outputName;

    Process process = new();
    process.StartInfo.FileName = vmPath;
    process.StartInfo.Arguments = $"\"{programPath}\"";
    process.StartInfo.UseShellExecute = true;

    process.Start();
    process.WaitForExit();
}
