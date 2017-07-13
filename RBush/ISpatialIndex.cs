using System.Collections.Generic;

namespace RBush
{
	public interface ISpatialIndex<out T>
	{
		IReadOnlyList<T> Search();
		IReadOnlyList<T> Search(Envelope boundingBox);
	}
}