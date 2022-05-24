using System;
using System.Collections.Generic;
using System.Linq;

namespace RBush
{
	public partial class RBush<T>
	{
		public class Node : ISpatialData
		{
			private Envelope _envelope;

			internal Node(List<ISpatialData> items, int height)
			{
				this.Height = height;
				this.children = items;
				ResetEnvelope();
			}

			internal void Add(ISpatialData node)
			{
				children.Add(node);
				_envelope = Envelope.Extend(node.Envelope);
			}

			internal void Remove(ISpatialData node)
			{
				children.Remove(node);
				ResetEnvelope();
			}

			internal void RemoveRange(int index, int count)
			{
				children.RemoveRange(index, count);
				ResetEnvelope();
			}

			internal void ResetEnvelope()
			{
				_envelope = GetEnclosingEnvelope(children);
			}

			internal readonly List<ISpatialData> children;

			public IReadOnlyList<ISpatialData> Children => children;
			public int Height { get; }
			public bool IsLeaf => Height == 1;
			public ref readonly Envelope Envelope => ref _envelope;
		}
	}
}
