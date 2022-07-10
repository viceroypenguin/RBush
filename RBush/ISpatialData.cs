namespace RBush;

/// <summary>
/// Exposes an <see cref="Envelope"/> that describes the
/// bounding box of current object.
/// </summary>
public interface ISpatialData
{
	/// <summary>
	/// The bounding box of the current object.
	/// </summary>
	ref readonly Envelope Envelope { get; }
}
