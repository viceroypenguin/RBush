using System.Collections.Generic;

namespace RBush
{
	public interface ISpatialDatabase<T> : ISpatialIndex<T>
	{
		void Insert(T item);
		void Delete(T item);

		void BulkLoad(IEnumerable<T> items);
	}
}