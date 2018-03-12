using System;
using System.Collections.Generic;
using System.Linq;

namespace RBush
{
	public partial class RBush<T>
	{
		internal class Node : ISpatialData
		{
			private Envelope _envelope;

			public Node(List<ISpatialData> items, int height)
			{
				this.Height = height;
				this.Children = items;
				ResetEnvelope();
			}

			public void Add(ISpatialData node)
			{
				Children.Add(node);
				_envelope = Envelope.Extend(node.Envelope);
			}

			public void ResetEnvelope()
			{
				_envelope = GetEnclosingEnvelope(Children);
			}

			public List<ISpatialData> Children { get; }
			public int Height { get; }
			public bool IsLeaf => Height == 1;
			public ref readonly Envelope Envelope => ref _envelope;
		}
	}
}
