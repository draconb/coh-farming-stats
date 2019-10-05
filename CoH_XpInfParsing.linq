<Query Kind="Program">
  <Output>DataGrids</Output>
</Query>

void Main()
{
	//
	// You need to modify the path below to the chat log you want to parse data from
	// EG: @"C:\Coh\Superman\chatlog 2019-08-05.txt";
  
	var file = @"C:\CoH\CHARACTERNAME\chatlog date.txt";
	
	//var amounts = new List<XpInfCollection>();

	var runs = new List<XpInfRun>();
	var mode = RunMode.Waiting;

	var run = new XpInfRun();
	Console.WriteLine("Starting new Run");

	foreach (var line in lines)
	{
		if (mode == RunMode.Waiting)
		{
			// For now lets keep it simple and just go with the first inf time
		}

		var xpMatch = XpAndInfRegex.Match(line);
		if (xpMatch.Success)
		{
			var xpInf = new XpInf();
			xpInf.DateTime = DateTime.Parse(xpMatch.Groups["datetime"].Value);
			xpInf.Xp = string.IsNullOrWhiteSpace(xpMatch.Groups["xp"].Value) ? 0 : int.Parse(xpMatch.Groups["xp"].Value, System.Globalization.NumberStyles.AllowThousands);
			xpInf.Influence = int.Parse(xpMatch.Groups["influence"].Value, System.Globalization.NumberStyles.AllowThousands);
			run.Add(xpInf);
			if (!runs.Contains(run))
			{
				runs.Add(run); // TODO: Optimize this lookup call
			}
			continue;
		}

		var completedMatch = MissionCompleteRegex.Match(line);
		if (completedMatch.Success)
		{
			Console.WriteLine("Finished Run");
			run.RunEnd = DateTime.Parse(completedMatch.Groups["datetime"].Value);
			run = new XpInfRun();
			Console.WriteLine("Starting new Run");
			continue;
		}
	}

	runs.Dump();

	//	amounts.Dump("All");
	//	amounts.TotalInfluence.Dump("Total Inf");
	//	amounts.TotalXp.Dump("Total Xp");
	//	amounts.TotalTime.Dump("Total Time");
	//	amounts.MinuteResults.Dump("All Minutes");
	//
	//	var groupedAmounts = amounts.GroupBy(x => new { Time = x.DateTime.ToString("HH:mm") });
	//	groupedAmounts.Select(x => new { x.Key.Time, TotalXp = x.Sum(y => y.Xp), TotalInf = x.Sum(y => y.Influence) }).Dump();

}

enum RunMode
{
	Running = 0,
	Waiting = 1
}

public class XpInf
{
	public DateTime DateTime { get; set; }
	public int Xp { get; set; }
	public int Influence { get; set; }
}

public class XpInfRun : List<XpInf>
{
	public DateTime RunStart { get { return this.OrderBy(y => y.DateTime).First().DateTime; } }
	public DateTime RunEnd { get; set; }
	public TimeSpan RunTime { get { return RunEnd > RunStart ? RunEnd.Subtract(RunStart) : this.OrderBy(y => y.DateTime).Last().DateTime.Subtract(RunStart); } }

	public int TotalXp { get { return this.Sum(y => y.Xp); } }
	public int TotalInfluence { get { return this.Sum(y => y.Influence); } }
	public string XpPerMinute { get { return (this.TotalXp / RunTime.TotalMinutes).ToString("N0"); } }
	public string InfluencePerMinute { get { return (this.TotalInfluence / RunTime.TotalMinutes).ToString("N0"); } }
	public int Kills { get { return this.Count(); } }
}

public class XpInfCollection : List<XpInf>
{
	public int TotalXp { get { return this.Sum(y => y.Xp); } }
	public int TotalInfluence { get { return this.Sum(y => y.Influence); } }
	public TimeSpan TotalTime { get { return TimeSpan.FromSeconds(this.Select(y => y.DateTime).Distinct().Count()); } } // Get all unique seconds

	public List<XpInfResult> MinuteResults
	{
		get
		{
			return this.GroupBy(x => new { Time = x.DateTime.ToString("HH:mm") })
				.Select(x => new XpInfResult { TimePeriod = x.Key.Time, Xp = x.Sum(y => y.Xp), Influence = x.Sum(y => y.Influence) }).ToList();
		}
	}
}

public class XpInfResult
{
	public string TimePeriod { get; set; }
	public int Xp { get; set; }
	public string TotalXp { get { return Xp.ToString("N0"); } }
	public int Influence { get; set; }
	public string TotalInfluence { get { return Influence.ToString("N0"); } }
}

// Define other methods and classes here
Regex XpAndInfRegex = new Regex(@"^(?<datetime>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})( You gain )((?<xp>[0-9,]+)( experience and ))?(?<influence>[0-9,]+)( influence| infamy.)", RegexOptions.Singleline);
Regex MissionCompleteRegex = new Regex(@"^(?<datetime>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})( Team task completed.)");
Regex MissionStartingRegex = new Regex(@"^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})( \w+)+ MISSES!( [\w\.\%]+)+ chance to hit");
