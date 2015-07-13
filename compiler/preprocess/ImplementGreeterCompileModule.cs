namespace CompileModuleExample.compiler.preprocess
{
    using System.Diagnostics;
    using System.Linq;

    using Microsoft.CodeAnalysis;
    //using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.Framework.Runtime.Roslyn;

    using T = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree;
    using F = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using K = Microsoft.CodeAnalysis.CSharp.SyntaxKind;

    public class ImplementGreeterCompileModule : ICompileModule
    {
        public void AfterCompile(AfterCompileContext context)
        {
            // NoOp
        }

        public void BeforeCompile(BeforeCompileContext context)
        {
            // Get our Greeter class.
            var syntaxMatch = context.Compilation.SyntaxTrees
                .Select(s => new
                {
                    Tree = s,
                    Root = s.GetRoot(),
                    Class = s.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Where(cs => cs.Identifier.ValueText == "Greeter").SingleOrDefault()
                })
                .Where(a => a.Class != null)
                .Single();

            var tree = syntaxMatch.Tree;
            var root = syntaxMatch.Root;
            var classSyntax = syntaxMatch.Class;

            // Get the method declaration.
            var methodSyntax = classSyntax.Members
                .OfType<MethodDeclarationSyntax>()
                .Where(ms => ms.Identifier.ValueText == "GetMessage")
                .Single();

            // Let's implement the body.
            var returnStatement = F.ReturnStatement(
                F.LiteralExpression(
                    K.StringLiteralExpression,
                    F.Literal(@"""Hello World!""")));

            // Get the body block
            var bodyBlock = methodSyntax.Body;

            // Create a new body block, with our new statement.
            var newBodyBlock = F.Block(new StatementSyntax[] { returnStatement });

            // Get the revised root
            var newRoot = (CompilationUnitSyntax)root.ReplaceNode(bodyBlock, newBodyBlock);

            // Create a new syntax tree.
            var newTree = T.Create(newRoot);

            // Replace the compilation.
            context.Compilation = context.Compilation.ReplaceSyntaxTree(tree, newTree);
        }
    }
}
