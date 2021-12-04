#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace WaterSortPuzzle
{
    public class Tube : IEquatable<Tube>
    {
        public const int LayerCount = 4;
        public const int DeepBottom = 0;
        public const int MidBottom = 1;
        public const int MidTop = 2;
        public const int HighTop = 3;

        private readonly Color[] _layer = new Color[LayerCount];

        public enum TubeColorStatus { Empty, Unicolor, Multicolor }

        public int Id { get; }

        public Tube(int id) : this(id, Color.Empty, Color.Empty, Color.Empty, Color.Empty) { }

        public Tube(int id, Color deepBottom) : this(id, deepBottom, Color.Empty, Color.Empty, Color.Empty) { }

        public Tube(int id, Color deepBottom, Color midBottom) : this(id, deepBottom, midBottom, Color.Empty, Color.Empty) { }

        public Tube(int id, Color deepBottom, Color midBottom, Color midTop) : this(id, deepBottom, midBottom, midTop, Color.Empty) { }

        public Tube(int id, Color deepBottom, Color midBottom, Color midTop, Color highTop)
        {
	        Id = id;
            _layer[DeepBottom] = deepBottom;
            _layer[MidBottom] = midBottom;
            _layer[MidTop] = midTop;
            _layer[HighTop] = highTop;
        }

        public Tube(Tube other)
        {
	        Id = other.Id;
	        _layer[DeepBottom] = other._layer[DeepBottom];
	        _layer[MidBottom] = other._layer[MidBottom];
	        _layer[MidTop] = other._layer[MidTop];
	        _layer[HighTop] = other._layer[HighTop];
        }

        public Color this[int inx] => _layer[inx];

        public Color TopColor
        {
	        get
	        {
		        if (_layer[HighTop] != Color.Empty) return _layer[HighTop];
		        if (_layer[MidTop] != Color.Empty) return _layer[MidTop];
		        if (_layer[MidBottom] != Color.Empty) return _layer[MidBottom];
		        if (_layer[DeepBottom] != Color.Empty) return _layer[DeepBottom];
		        return Color.Empty;
	        }
        }

        public bool IsEmpty() => _layer[DeepBottom] == Color.Empty;

        // TODO: if a color can be split between tube then implement that possibility
        public static bool CanPour(Tube frTube, Tube toTube)
        {
	        var (frTopColorCount, frTopColor) = frTube.GetTopEmptyCount();

	        // Invariant: there are cnt top color topColor
	        // Are there cnt Empty colors in toTube
	        var (toEmptyCount, toTopColor) = toTube.GetTopEmptyCount();

	        // If toTube is empty and we empty all of frTube then this operation is forbidden
	        if (frTube.IsUniColor() == TubeColorStatus.Unicolor && toTube.IsEmpty()) return false;

	        return toEmptyCount >= frTopColorCount;
        }

        /// <summary>
        /// Rules for pouring:
        ///     1   All top color needs to pour (may be multiple layers
        ///     2   Only top color may pour
        ///     3   From-color can pour only if To-Tube has enough layers Empty
        ///     4   From-color can pour on top of same color or replace bottom most Empty color
        ///     5   Tube cannot house color on top of an empty layer, it must replace it
        ///     6   Cannot move from 1 tube to another with no change.  Ex tube 1 contains 4 layers of DarkBlue
        ///         Tube 2 is Empty therefore, pouring all 4 layers from tube 1 to tube 2 does not move the solution
        ///         forward and therefore this operation is prohibited.
        /// </summary>
        /// <returns>Total number of layers moved</returns>
        public static int Pour(Tube frTube, Tube toTube)
        {
            var (frTopColor, frTopColorCount) = frTube.GetTopColor();

            // frTube is full of the same color then it is immovable
            if ((frTopColor, frTopColorCount) == (Color.Empty, LayerCount)) return 0;

            // Invariant: there are frTopColorCount top color of frTube

            var (toEmptyCount, toEmptyColor) = toTube.GetTopEmptyCount();

            // If colors of toTube and frTube are not (same or empty) then no go
            if (frTopColor != toEmptyColor && toEmptyColor != Color.Empty) return 0;

            var layersToMove = Math.Min(frTopColorCount, toEmptyCount);
            var rc = toTube.Add(frTopColor, layersToMove);
            if (!rc) return 0;
            frTube.Remove(layersToMove);

            Console.WriteLine($"\tDbg: from {frTube.Id,2}({frTopColor}[{layersToMove}]) -> to {toTube.Id,2}");
            return layersToMove;
        }

        public static int Pour(Tube frTube, Tube toTube, List<Tube> workingTubes)
        {
            var (frTopColor, frTopColorCount) = frTube.GetTopColor();

            // frTube is full of the same color then it is immovable
            if ((frTopColor, frTopColorCount) == (Color.Empty, LayerCount)) return 0;

            // Invariant: there are frTopColorCount top color of frTube

            var (toEmptyCount, toEmptyColor) = toTube.GetTopEmptyCount();

            // If colors of toTube and frTube are not (same or empty) then no go
            if (frTopColor != toEmptyColor && toEmptyColor != Color.Empty) return 0;

            var layersToMove = Math.Min(frTopColorCount, toEmptyCount);
            var toWorkingTube = GetWorkingTube(toTube, workingTubes);
            var rc = toWorkingTube.Add(frTopColor, layersToMove);
            if (!rc) return 0;

            var fromWorkingTube = GetWorkingTube(frTube, workingTubes);
            fromWorkingTube.Remove(layersToMove);

            Console.WriteLine($"\tDbg: from {frTube.Id,2}({frTopColor}[{layersToMove}]) -> to {toTube.Id,2}");
            return layersToMove;
        }

        private static Tube GetWorkingTube(Tube tube, List<Tube> workingTubes)
        {
            foreach (var wt in workingTubes)
                if (wt.Id == tube.Id) return wt;

            throw new Exception($"Tube {tube.Id} cannot be found in the workingTubes set");
        }

        private bool Add(Color color, int count)
        {
            var (emptyCount, _) = GetTopEmptyCount();
            if (emptyCount < count) throw new ArgumentException($"Attempting to pour {count} {color} into {emptyCount} layers", nameof(count));
            var inx = LayerCount - emptyCount;
            if (inx > 0)
	            if (_layer[inx - 1] != color)
		            return false;

            for (var i = 0; i < count; ++i)
                _layer[inx + i] = color;
            return true;
        }

        /// <summary>
        /// Assume that count > 0
        /// </summary>
        /// <param name="count"></param>
        private void Remove(int count)
        {
            var (emptyCount, _) = GetTopEmptyCount();
            if (emptyCount + count > LayerCount)
	            throw new ArgumentException($"Attempting to pour {count} layers while we only have {LayerCount - emptyCount} filled layers", nameof(count));

            var inx = LayerCount - emptyCount - 1;
            var prevColor = _layer[inx];
            for (var i = 0; i < count; ++i)
            {
	            if (_layer[inx] != prevColor)
		            throw new Exception($"Color inconsistency: Previous Color: {prevColor} attempting to pour now: {_layer[inx]}");

	            _layer[inx] = Color.Empty;
	            --inx;
            }

            if (inx >= 0)
            {
	            if (_layer[inx] == prevColor)
		            throw new Exception($"Cannot remove {count} layers because we leave at least 1 more layer of {prevColor}");
            }
        }

        public (Color topColor, int count) GetTopColor()
        {
            // Find first color
            var emptyTop = GetTopEmptyCount();
            if (emptyTop.emptyCount == LayerCount)
	            return (Color.Empty, LayerCount);

            // First color is in layer cl.
            var cl = LayerCount - emptyTop.emptyCount - 1;
            var topColor = _layer[cl];

            // Number of items items having topmost Color
            var cnt = 0;
            for (; cnt < LayerCount; ++cnt)
            {
                var inx = cl - cnt - 1;
                if (inx < 0) break;
                if (_layer[inx] != topColor) break;
            }

            return (topColor, cnt + 1);
        }

        public (int emptyCount, Color topColor) GetTopEmptyCount()
        {
            int cl = LayerCount - 1;
            for ( ; cl >= 0; --cl)
                if (_layer[cl] != Color.Empty)
                    break;

            var color = (LayerCount - cl - 1 == LayerCount) ? Color.Empty : _layer[cl];
            return (LayerCount - cl - 1, color);
        }

        /// <summary>
        /// Is tube contains a single color
        /// </summary>
        /// <returns></returns>
        public TubeColorStatus IsUniColor()
        {
	        if (_layer[DeepBottom] == Color.Empty) return TubeColorStatus.Empty;

	        var color = _layer[DeepBottom];
	        for (var depth = MidBottom; depth <= HighTop; ++depth)
	        {
		        // We ended up with an empty layer.  Therefore, all layers are of the same color
		        if (_layer[depth] == Color.Empty) return TubeColorStatus.Unicolor;

		        // We encountered a different color.
		        if (color != _layer[depth]) return TubeColorStatus.Multicolor;
	        }

	        // We traversed the entire tube and found no different color.  Therefore, all layers are of the same color.
	        return TubeColorStatus.Unicolor;
        }

        public static bool IsSimilar(Tube lhs, Tube rhs)
        {
            for (var layer = 0; layer < LayerCount; ++layer)
	            if (lhs._layer[layer] != rhs._layer[layer])
		            return false;
            return true;
        }

        public int SimilarHashCode() =>
	        string.Join(",", Enumerable.Range(0, LayerCount).Select(i => this[i].ToString())).GetHashCode();


        #region IEquatable<Tube>

        public bool Equals(Tube other)
        {
	        if (ReferenceEquals(null, other)) return false;
	        if (ReferenceEquals(this, other)) return true;
	        return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
	        if (ReferenceEquals(null, obj)) return false;
	        if (ReferenceEquals(this, obj)) return true;
	        if (obj.GetType() != GetType()) return false;
	        return Equals((Tube)obj);
        }

        public override int GetHashCode() => Id.GetHashCode();

        public static bool operator ==(Tube lhs, Tube rhs)
        {
	        var isLhsNull = ReferenceEquals(lhs, null);
	        var isRhsNull = ReferenceEquals(rhs, null);
	        if (isLhsNull ^ isRhsNull) return false;
	        if (isLhsNull) return true;
	        return lhs.Equals(rhs);
        }

        public static bool operator !=(Tube lhs, Tube rhs) => !(lhs == rhs);

        #endregion

        public override string ToString() => $"{Id,3}: {ColorPrinted(_layer[DeepBottom])} {ColorPrinted(_layer[MidBottom])} {ColorPrinted(_layer[MidTop])} {ColorPrinted(_layer[HighTop])}";

        private string ColorPrinted(Color color)
            => color == Color.Empty ? $"{new string(' ', 11)}-" : $"{color.ToString(),12}";
    }
}