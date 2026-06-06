using ASTPasses;
using CodeGen;
using Errors;
using IR;
using Lexing;
using Parsing;
using Runner;
using System.IO;





Tokenizer t = new(File.ReadAllText("main.tph"));



var tokens = t.MakeTokens();



foreach(Token tok in tokens)
{
    Console.WriteLine(tok);
}


Console.WriteLine("--- AST ---");

Parser p = new(tokens);
List<ASTNode> nodes = p.ParseModule();


foreach(ASTNode node in nodes)
{
    node.Show(0);
}


SymbolResolver symbResolver = new();
TypeResolver typeResolver = new();

foreach(ASTNode node in nodes)
{
    node.AcceptVisitor(symbResolver);
   node.AcceptVisitor(typeResolver);
}

symbResolver.DisplayScopes();


if(ErrorHandler.HasErrors())
{
    ErrorHandler.ThrowAll();
}

Console.WriteLine("--- Stage 1 IR ---");

IRBuilder builder = new();

foreach(ASTNode node in nodes)
{
    node.MakeInstruction(builder);
}


builder.ShowInstructions();


Console.WriteLine("--- Code Gen ---");

CodeGenerator generator = new(builder.GetInstructions());
File.WriteAllBytes("code.tpc",generator.Generate().ToArray());


//Console.WriteLine("--- Executing ---");
//VM vm = new VM(builder.GetInstructions());
//vm.Run();
//Console.WriteLine(p.ParseExpression().MakeInstruction());*/
