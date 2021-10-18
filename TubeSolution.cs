using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace WaterSortPuzzle
{
	public class TubeSolution
	{
		private readonly List<Tube> _tubes;

		public TubeSolution()
		{
			_tubes = new List<Tube>();
			InitTubes();
		}

		private void InitTubes()
		{
			_tubes.AddRange(new List<Tube> {
				new(1,  Color.DarkBrown,   Color.Orange,      Color.Grey,       Color.DarkBrown),			// DarkBrown 1,   Orange 1,      Grey 1,       DarkBrown 2
				new(2,  Color.Purple,      Color.LightBlue,   Color.Hunter,     Color.Pink),    			// Purple 1,      LightBlue 1,   Hunter 1,     Pink 1
				new(3,  Color.Red,         Color.Orange,      Color.Hunter,     Color.Grey),				// Red 1,         Orange 2,      Hunter 2,     Grey 2
				new(4,  Color.Pink,        Color.DarkBrown,   Color.Red,        Color.DarkBlue),			// Pink 2,        DarkBrown 3,   Red 2,        DarkBlue 1
				new(5,  Color.ForestGreen, Color.DarkBlue,    Color.Yellow,     Color.Orange),  			// ForestGreen 1, DarkBlue 2,    Yellow 1,     Orange 3
				new(6,  Color.DarkBlue,    Color.Orange,      Color.LightGreen, Color.Yellow),  			// DarkBlue 3,    Orange 4,      LightGreen 1, Yellow 2
				new(7,  Color.LightBlue,   Color.LightGreen,  Color.LightGreen, Color.DarkBlue),			// LightBlue 2,   LightGreen 2,  LightGreen 3, DarkBlue 4
				new(8,  Color.Hunter,      Color.ForestGreen, Color.Purple,     Color.Pink),				// Hunter 3,      ForestGreen 2, Purple 2,     Pink 3
				new(9,  Color.Yellow,      Color.ForestGreen, Color.LightGreen, Color.Hunter),				// Yellow 3,      ForestGreen 3, LightGreen 4, Hunter 4
				new(10, Color.LightBlue,   Color.DarkBrown,   Color.Grey,       Color.Purple),  			// LightBlue 3,   DarkBrown 4,   Grey 3,       Purple 3
				new(11, Color.LightBlue,   Color.Grey,        Color.Purple,     Color.ForestGreen),			// LightBlue 4,   Grey 4,        Purple 4,     ForestGreen 4
				new(12, Color.Pink,        Color.Red,         Color.Red,        Color.Yellow),				// Pink 4,        Red 3,         Red 4,        Yellow 4
				new(13),
				new(14)
			});
		}

		public (bool, IEnumerable<Tube>) Solve() => Solve(_tubes, 0);

		private static (bool, IEnumerable<Tube>) Solve(List<Tube> tubes, int depth)
		{
			//Console.WriteLine($"{new string('.', depth)}[10] {depth,4}.  Dbg Bfr: Solve-tubes:\n{Program.ToString(tubes)}");
			var currTubes = new List<Tube>(tubes.Select(t => new Tube(t))).ToList();

			// foreach tube from top-color
			// foreach tube to (not = from)
			// if from-tube can pour into to-tube
			foreach (var frTube in currTubes)
			{
				var (frTopColor, frTopCount) = frTube.GetTopColor();
				if (frTopColor == Color.Empty) continue;		// If tube is empty it cannot be used as from-tube
				if (frTopCount == Tube.LayerCount) continue;	// If tube is full of the same color then it cannot be used as from-tube

				List<(Tube toTube, (int emptyCount, Color topColor) emp)> toEmpties =
					currTubes
					.Select(tot => (tot, tot.GetTopEmptyCount()))
					.Where(t => {
						var toT = t.tot;						// toTube
						var emp = t.Item2;						// empty details
						if (toT == frTube) return false;		// If to-tube is the same as from-tube then we cannot move from the tube to itself
						if (emp.emptyCount == 0) return false;	// If to-tube has no empty layers then we cannot use it
						if (emp.topColor != frTopColor && emp.topColor != Color.Empty) return false;	// If top colors of to-tube and fr-tube are not the same then we cannot move colors
						return true;
					})
					// If 2  tubes are similar then take the first one only
					.GroupBy(t => t.tot, t => t, new LinqComparer<Tube>(Tube.IsSimilar, t => t.SimilarHashCode())).Select(t => t.First())
					.ToList();

				var totalPotentialEmpty = toEmpties.Aggregate(0, (a, i) => a + i.emp.emptyCount);
				if (frTopCount > totalPotentialEmpty) continue;

				// If frTopCount > totalPotentialEmpty do nothing
				// If frTopCount == totalPotentialEmpty then just go through toTubes
				// If frTopCount < totalPotentialEmpty then for each of the toTubes distribute the frTube layers
				if (frTopCount > totalPotentialEmpty) continue;
				if (frTopCount == totalPotentialEmpty)
				{
					Console.WriteLine($"{new string('.', depth)}[20] {depth,4}.  Dbg Bfr: Solve-tubes:\n{Program.ToString(tubes)}");
					var layersMoved = 0;
					foreach (var emp in toEmpties)
						layersMoved += Tube.Pour(frTube, emp.toTube);
					if (layersMoved != frTopCount)
						throw new Exception($"Total layers (of color {frTopColor}) that frTube ({frTube.Id}) could move is: {frTopCount}.  Total moved: {layersMoved}");

					if (IsDone(currTubes)) return (true, currTubes);
					if (IsStuck(currTubes)) return (false, currTubes);
					Solve(currTubes, depth + 1);
					//Console.WriteLine($"{new string('.', depth)}[30] {depth,4}.  Dbg Aft: Solve-tubes:\n{Program.ToString(currTubes)}");
				}
				else                        // frTopCount < totalPotentialEmpty
				{
					var permutations = toEmpties.GetPermutations().ToList();
					foreach (var empties in permutations)
					{
						currTubes = new List<Tube>(tubes.Select(t => new Tube(t))).ToList();
						Console.WriteLine($"{new string('.', depth)}[40] {depth,4}.  Dbg Bfr: Solve-tubes:\n{Program.ToString(tubes)}");
						var layersMoved = 0;
						foreach (var emp in empties)
							layersMoved += Tube.Pour(frTube, emp.toTube);
						if (layersMoved != frTopCount)
							throw new Exception($"Total layers (of color {frTopColor}) that frTube ({frTube.Id}) could move is: {frTopCount}.  Total moved: {layersMoved}");

						if (IsDone(currTubes)) return (true, currTubes);
						if (IsStuck(currTubes)) return (false, currTubes);
						Solve(currTubes, depth + 1);
						//Console.WriteLine($"{new string('.', depth)}[50] {depth,4}.  Dbg Aft: Solve-tubes:\n{Program.ToString(currTubes)}");
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
	}
}
