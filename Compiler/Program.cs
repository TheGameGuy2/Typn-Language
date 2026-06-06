using ASTPasses;
using CodeGen;
using Errors;
using IR;
using Lexing;
using Parsing;
using Runner;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;


string path = "";
string outputName = "out.tpc";
bool debugMode = false;


foreach(string arg in args)
{
    if(arg.StartsWith("f:"))
    {
        path = arg.Replace("f:","");
    }

    

    if(arg == "debug")
    {
        debugMode = true;
    }

    if(arg.StartsWith("o:"))
    {
        outputName = arg.Replace("o:","");
    }
}

Console.WriteLine(path);

Tokenizer t = new(File.ReadAllText("main.tph"));



List<Token> tokens = t.MakeTokens();


if(debugMode)
{
    foreach(Token tok in tokens)
    {
        Console.WriteLine(tok);
    }
}

Parser p = new(tokens);
List<ASTNode> nodes = p.ParseModule();

if(debugMode)
{
    Console.WriteLine("--- AST ---");

    foreach(ASTNode node in nodes)
    {
        node.Show(0);
    }
}

SymbolResolver symbResolver = new();
TypeResolver typeResolver = new();

foreach(ASTNode node in nodes)
{
    node.AcceptVisitor(symbResolver);
   node.AcceptVisitor(typeResolver);
}

if(debugMode)
{
    symbResolver.DisplayScopes();
}


if(ErrorHandler.HasErrors())
{
    ErrorHandler.ThrowAll();
}



IRBuilder builder = new();
    
foreach(ASTNode node in nodes)
{
    node.MakeInstruction(builder);
}

if(debugMode)
{
    Console.WriteLine("--- Stage 1 IR ---");
    
    builder.ShowInstructions();
}

if(debugMode)
{   
    Console.WriteLine("--- Code Gen ---");
}

CodeGenerator generator = new(builder.GetInstructions());
File.WriteAllBytes(outputName,generator.Generate().ToArray());


string vmPath = "vm.exe";   
if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    vmPath = "./vm";
}
       // or "./MyVM" on Linux
string programPath = outputName;  // your bytecode file

var process = new Process();
process.StartInfo.FileName = vmPath;
process.StartInfo.Arguments = $"\"{programPath}\"";
process.StartInfo.UseShellExecute = true;

process.Start();
process.WaitForExit();

//Console.WriteLine("--- Executing ---");
//VM vm = new VM(builder.GetInstructions());
//vm.Run();
//Console.WriteLine(p.ParseExpression().MakeInstruction());*/
