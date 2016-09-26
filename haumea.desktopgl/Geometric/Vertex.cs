using Microsoft.Xna.Framework;

namespace Haumea.Geometric
{
	struct Vertex
	{
        public Vector2 Position { get; }
        public short   Index { get; }

        public Vertex(Vector2 position, short index)
		{
			Position = position;
			Index = index;
		}

		public override bool Equals(object obj)
		{
            return obj is Vertex && Equals((Vertex)obj);
		}

		public bool Equals(Vertex obj)
		{
            // Duplicate vertexes are now allowed, so no need to check index.
			return obj.Position.Equals(Position);
		}

		public override int GetHashCode()
		{
			return Position.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("{0} ({1})", Position, Index);
		}
	}
}