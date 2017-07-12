using System.Collections.Generic;

namespace RBush
{
	public interface ISpatialIndex<T>
	{
		IEnumerable<T> Search();
		IEnumerable<T> Search(Envelope boundingBox);
	}
}