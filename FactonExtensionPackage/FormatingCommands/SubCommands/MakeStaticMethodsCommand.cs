namespace FactonExtensionPackage.FormatingCommands
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using EnvDTE;
	using FactonExtensionPackage.Extensions;
	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp;
	using Microsoft.CodeAnalysis.CSharp.Syntax;

	public class MakeStaticMethodsCommand
	{
		public static void Execute(ProjectItem projectItem)
		{
			SyntaxTree tree = CSharpSyntaxTree.ParseText(projectItem.ReadAllText());
			var root = (CompilationUnitSyntax)tree.GetRoot();

			var compilation = CSharpCompilation.Create("HelloWorld")
				.AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))				.AddSyntaxTrees(tree);
			var semanticModel = compilation.GetSemanticModel(tree);

			IEnumerable<MethodDeclarationSyntax> methods =
				root.DescendantNodes()					
					.OfType<MethodDeclarationSyntax>()
					.Where(m => m.DescendantTokens().All(x => x.Kind() != SyntaxKind.StaticKeyword))
					.ToList();

			foreach (var method in methods)
			{
				if (MethodShouldBeStatic(method, semanticModel))
				{
					var firstToken = method.GetFirstToken();
					var firstTokenTrivia = firstToken.LeadingTrivia;

					var formattedMethodDeclaration = method.ReplaceToken(firstToken, firstToken.WithLeadingTrivia(SyntaxFactory.TriviaList()));

					SyntaxTriviaList syntaxTriviaList = firstTokenTrivia.ToSyntaxTriviaList();
					var staticToken = SyntaxFactory.Token(syntaxTriviaList, SyntaxKind.StaticKeyword, new SyntaxTriviaList());

					var newModifiers = SyntaxFactory.TokenList(new[] { staticToken }.Concat(formattedMethodDeclaration.Modifiers));

					var newMethodDeclaration = formattedMethodDeclaration.Update(
						formattedMethodDeclaration.AttributeLists,
						newModifiers,
						formattedMethodDeclaration.ReturnType,
						formattedMethodDeclaration.ExplicitInterfaceSpecifier,
						formattedMethodDeclaration.Identifier,
						formattedMethodDeclaration.TypeParameterList,
						formattedMethodDeclaration.ParameterList,
						formattedMethodDeclaration.ConstraintClauses,
						formattedMethodDeclaration.Body,
						formattedMethodDeclaration.SemicolonToken);

					var formattedLocalDeclaration = CodeActionAnnotations.FormattingAnnotation.AddAnnotationTo(newMethodDeclaration);

					var newRoot = root.ReplaceNode(method, formattedLocalDeclaration);
					return _editFactory.CreateTreeTransformEdit(_document.Project.Solution,
						tree, newRoot, cancellationToken: cancellationToken);
				}
				
			}
		}

		private static bool MethodShouldBeStatic(MethodDeclarationSyntax method, SemanticModel semanticModel)
		{
			if (method.Body == null) return false;

			var dataFlow = semanticModel.AnalyzeDataFlow(method.Body);
			var variablesDeclared = dataFlow.VariablesDeclared.ToList();
			var variablesRead = dataFlow.ReadInside.Union(dataFlow.ReadOutside).ToList();
			if (variablesDeclared.Any(c => c.Name == "this"))
			{
				return false;
			}
			if (variablesRead.Any(c => c.Name == "this"))
			{
				return false;
			}
			return true;
		}	
	}
}