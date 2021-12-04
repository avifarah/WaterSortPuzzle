#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WaterSortPuzzle
{
    public class TubeSolution
	{
		private readonly List<Tube> _tubes;

		private static readonly Stack<List<Tube>> _stack = new Stack<List<Tube>>();

		public TubeSolution()
		{
			_tubes = new List<Tube>();
			InitTubes();
		}

		private void InitTubes()
		{
			_tubes.AddRange(new List<Tube> {
				new(1,  Color.Orange,      Color.Grey,        Color.Yellow,      Color.Purple),			// Orange:		 1,  3,  5,  8
				new(2,  Color.LightGreen,  Color.Grey,        Color.LightBlue,   Color.DarkBrown),		// Grey:		 1,  2,  3,  9
				new(3,  Color.Grey,        Color.Red,         Color.Orange,      Color.LightBlue),		// Yellow:		 1,  5,  8, 10
				new(4,  Color.DarkBrown,   Color.DarkBlue,    Color.Purple,      Color.DarkBrown),		// Purple:		 1,  4,  5, 12
				new(5,  Color.Purple,      Color.Yellow,      Color.LightBlue,   Color.Orange),			// LightGreen:	 2,  6,  9, 10
				new(6,  Color.Pink,        Color.LightGreen,  Color.ForestGreen, Color.DarkBlue),		// LightBlue:	 2,  3,  5,  7
				new(7,  Color.Red,         Color.Pink,        Color.ForestGreen, Color.LightBlue),		// DarkBrown:	 2,  4,  4,  9
				new(8,  Color.DarkBlue,    Color.Yellow,      Color.Orange,      Color.Chateau),		// Red:			 3,  7, 11, 11
				new(9,  Color.LightGreen,  Color.Grey,        Color.DarkBrown,   Color.Chateau),		// DarkBlue:	 4,  6,  8, 11
				new(10, Color.Chateau,     Color.LightGreen,  Color.Yellow,      Color.ForestGreen),	// Pink:		 6,  7, 11, 12
				new(11, Color.Red,         Color.DarkBlue,    Color.Pink,        Color.Red),			// ForestGreen:	 6,  7, 10, 12
				new(12, Color.Pink,        Color.ForestGreen, Color.Chateau,     Color.Purple),			// Chateau:		 8,  9, 10, 12
				new(13),
				new(14)
			});
		}

		public (bool, IEnumerable<Tube>) Solve() => Solve(_tubes, 0);

		private static (bool, IEnumerable<Tube>) Solve(List<Tube> tubes, int depth)
		{
			bool IsTubeEmpty(Color topColor) => topColor == Color.Empty;
			bool IsTubeFull(int layersFull) => layersFull == Tube.LayerCount;

			if (depth > 200)
            {
				Console.WriteLine();
				Console.WriteLine($"Depth {depth} exceeded max");
				return (false, tubes);
            }

			foreach (var frTube in tubes)
			{
				var currTubes = Clone(tubes);                   // Work on a new set of tubes that are apropriate for this recursive pass

				var (frTopColor, frTopCount) = frTube.GetTopColor();
				if (IsTubeEmpty(frTopColor)) continue;
				if (IsTubeFull(frTopCount)) continue;

				// Get ToTubes to go through
				List<(Tube toTube, (int emptyCount, Color topColor) emp)> toEmpties =
					currTubes
					.Select(tot => (ToTube: tot, EmptyCount: tot.GetTopEmptyCount()))
					.Where(t => {
						var toT = t.ToTube;						// toTube
						var emp = t.EmptyCount;					// empty details
						if (toT == frTube) return false;		// If to-tube is the same as from-tube then we cannot move from the tube to itself
						if (emp.emptyCount == 0) return false;	// If to-tube has no empty layers then we cannot use it
						if (emp.topColor != frTopColor && emp.topColor != Color.Empty) return false;	// If top colors of to-tube and fr-tube are not the same then we cannot move colors
						return true;
					})
					// If 2 tubes are similar then take the first one only
					.GroupBy(t => t.ToTube, t => t, new LinqComparer<Tube>(Tube.IsSimilar, t => t.SimilarHashCode())).Select(t => t.First())
					.ToList();

				var totalPotentialEmpty = toEmpties.Aggregate(0, (a, i) => a + i.emp.emptyCount);

				// If frTopCount > totalPotentialEmpty then there is no room to pour out the entire top layer of the same color into
				// If frTopCount == totalPotentialEmpty then just go through toTubes
				// If frTopCount < totalPotentialEmpty then for each of the toTubes distribute the frTube layers
				if (frTopCount > totalPotentialEmpty) continue;
				if (frTopCount == totalPotentialEmpty)
				{
					//Console.WriteLine($"({depth,3})[20]{Program.ToString(tubes)}");
					var layersMoved = 0;
					var track = new List<string>();
					foreach (var emp in toEmpties)
					{
						var movedCount = Tube.Pour(frTube, emp.toTube, currTubes);
						layersMoved += movedCount;
						track.Add($"Moved {movedCount} {frTube.Id} -> {emp.toTube.Id}, tmv: {layersMoved}");
					}
					if (layersMoved != frTopCount)
					{
						throw new Exception($"Total layers (of color {frTopColor}) that frTube ({frTube.Id}) could move is: {frTopCount}.  Total moved: {layersMoved}{Environment.NewLine}{string.Join(", ", track.Select(t => $"({t})"))}{Environment.NewLine}"
							  + $"{depth,3}[25]{Program.ToString(currTubes)}");
					}
					Console.WriteLine($"({depth,3})[30]{Program.ToString(currTubes)}");

					if (IsDone(currTubes)) return (true, currTubes);
					if (IsStuck(currTubes)) return (false, currTubes);
					(var isDone, var doneTubes) = Solve(currTubes, depth + 1);
					if (isDone) return (true, doneTubes);
					Console.WriteLine($"[35]Roll depth back to {depth}.  FromTube processed: {frTube}{Environment.NewLine}\t{Program.ToString(tubes)}");
				}
				else                        // frTopCount < totalPotentialEmpty
				{
					var permutations = toEmpties.GetPermutations().ToList();
					foreach (var empties in permutations)
					{
						//Console.WriteLine($"({depth,3})[40]{Program.ToString(workWithTubes)}");
						var workWithTubes = Clone(currTubes);

						var layersMoved = 0;
						var track = new List<string>();
						foreach (var emp in empties)
						{
							var movedCount = Tube.Pour(frTube, emp.toTube, workWithTubes);
							layersMoved += movedCount;
							track.Add($"Moved {movedCount} {frTube.Id} -> {emp.toTube.Id}, tmv: {layersMoved}");
						}
						if (layersMoved != frTopCount)
						{
							throw new Exception($"Total layers (of color {frTopColor}) that frTube ({frTube.Id}) could move is: {frTopCount}.  Total moved: {layersMoved}{Environment.NewLine}{string.Join(", ", track.Select(t => $"({t})"))}{Environment.NewLine}"
								  + $"{depth,3}[45]{Program.ToString(workWithTubes)}");
						}
						Console.WriteLine($"({depth,3})[50]{Program.ToString(workWithTubes)}");

						if (IsDone(workWithTubes)) return (true, workWithTubes);
						if (IsStuck(workWithTubes)) return (false, workWithTubes);
						(var isDone, var doneTubes) = Solve(workWithTubes, depth + 1);
						if (isDone) return (true, doneTubes);
						Console.WriteLine($"[60]Roll depth back to {depth}.  FromTube processed: {frTube}.  empties: {(string.Join(", ", empties.Select(e => e.toTube.Id)))}{Environment.NewLine}\t{Program.ToString(currTubes)}");
					}
				}
			}

			return (false, tubes);
		}

		private static bool IsDone(List<Tube> tubes)
		{
			foreach (var tube in tubes)
				if (tube.IsUniColor() == Tube.TubeColorStatus.Multicolor)
					return false;
			return true;
		}

		private static bool IsStuck(List<Tube> tubes)
		{
			foreach (var frTube in tubes)
			foreach (var toTube in tubes)
			{
				if (frTube == toTube) continue;
				if (Tube.CanPour(frTube, toTube)) return false;
			}

			return true;
		}

		private static List<Tube> Clone(List<Tube> tubes) => new List<Tube>(tubes.Select(t => new Tube(t))).ToList();

		private static void PushSolution(List<Tube> tubes) => _stack.Push(tubes);

		private static List<Tube> PopSolution() => _stack.Pop();
	}
}
