using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RBush.Test" + RBush.AssemblyInfo.PublicKey)]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace RBush
{
	internal class AssemblyInfo
	{
#if SIGN_ASSEMBLY
		public const string PublicKey = ", PublicKey=0024000004800000940000000602000000240000525341310004000001000100d10735a2130f015b9b93a504faf99311213e8786c81bfec874fbd7a76017f6356581d679b7799f0254b70f03165f12252c3af31a50f05c9e4db172588d72123b127d39ffea3a288c1e6c68469e19fd362ad1c9e6e14ed7855fcb4e5b6862a23094048c551444798f60348a421f4e364f076febfad902231bd4289e2ba809f8b3";
		public const string PublicKeyToken = "b1d5b814a0c60675";
#else
        public const string PublicKey = "";
        public const string PublicKeyToken = "";
#endif
	}
}
