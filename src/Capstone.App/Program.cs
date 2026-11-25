using Capstone.Core.Model;
using Capstone.Core.Storage;
using SharpQuest.Contracts;

// A small menu over the music library.
// dotnet run --project src/Capstone.App

var recent = new RecentlyUsed<string>(5);
var seedPath = Path.Combine(AppContext.BaseDirectory, "data", "library.json");
var savePath = LibraryFile.DefaultPath;   // shared with the window (Capstone.Desktop)

// S8: what was saved last time, or an empty library on the very first run
Library library;
try
{
    library = await LibraryFile.LoadAsync(savePath);
}
catch (InvalidDataException e)
{
    // Corrupt is not the same as missing: say so, and don't overwrite it on quit.
    Console.WriteLine($"Could not read {savePath}: {e.Message}");
    Console.WriteLine("Starting from the bundled sample instead; your file is left untouched.");
    savePath = savePath + ".recovered.json";
    library = new Library();
}

if (library.Count > 0)
{
    Console.WriteLine($"{library.Count} items loaded from {savePath}.");
}
else
{
    // S6: event — count what actually got added while seeding, then stop listening
    var seeded = 0;
    Action<IItem> countSeeded = _ => seeded++;
    library.ItemAdded += countSeeded;
    SampleLibrary.AddTo(library, seedPath, skipped: json => Console.WriteLine($"skipped invalid track: {json}"));
    library.ItemAdded -= countSeeded;
    Console.WriteLine($"First run: {seeded} items loaded from the bundled sample.");
}

const int pageSize = 5;

while (true)
{
    Console.WriteLine();
    Console.WriteLine("1) List everything   2) Search   3) Open by Id   4) By genre");
    Console.WriteLine("5) Play a track      6) Rate a track   7) Recently opened   8) Stats");
    Console.WriteLine("9) Browse A-Z        s) Save           0) Save and quit");
    Console.Write("> ");
    var choice = Console.ReadLine()?.Trim();
    if (choice is null or "0") break;
    if (choice is "s" or "S")
    {
        await LibraryFile.SaveAsync(library, savePath);   // S8: await, not .Wait()
        Console.WriteLine($"  saved to {savePath}.");
        continue;
    }

    switch (choice)
    {
        case "1":
            // S4: polymorphic loop — each kind describes itself
            foreach (var item in library)
                Console.WriteLine($"  [{Kind(item)}] {((LibraryItem)item).Describe()}");
            break;

        case "2":
            var text = Ask("Search for");
            var hits = library.Matching(text).ToList();   // S5: iterator
            Show(hits);
            break;

        case "3":
            var id = Ask("Id");
            if (library[id] is { } found)                  // S5: string indexer
            {
                recent.Push(id);
                Console.WriteLine("  " + ((LibraryItem)found).Describe());
            }
            else Console.WriteLine("  no such item.");
            break;

        case "4":
            var name = Ask($"Genre ({string.Join(", ", Enum.GetNames<Genre>())})");
            if (Enum.TryParse<Genre>(name, ignoreCase: true, out var genre))
                Show(library.Where(i => i is Track t && t.Genre == genre));   // S6: function as parameter
            else Console.WriteLine("  unknown genre.");
            break;

        case "5":
            if (TrackById() is { } playing)
            {
                playing.PlayCount++;
                recent.Push(playing.Id);
                Console.WriteLine($"  Played '{playing.Title}' — {playing.PlayCount} plays.");
            }
            break;

        case "6":
            if (TrackById() is { } rated)
            {
                var input = Ask("Rating 1-5 (blank = unrated)");
                try
                {
                    rated.Rating = int.TryParse(input, out var stars) ? stars : null;
                    Console.WriteLine("  " + rated.Describe());
                }
                catch (ArgumentOutOfRangeException e) { Console.WriteLine("  " + e.Message); }
            }
            break;

        case "7":
            // S5: generic RecentlyUsed<T>
            var names = recent.Items.Select(rid => library[rid]?.DisplayName ?? rid);
            Console.WriteLine(recent.Items.Count == 0 ? "  nothing yet." : "  " + string.Join("\n  ", names));
            break;

        case "8":
            var tracks = library.Where(i => i is Track).Cast<Track>().ToList();
            var total = TimeSpan.FromTicks(tracks.Sum(t => t.Duration.Ticks));
            Console.WriteLine($"  {tracks.Count} tracks, {total.TotalHours:F1} hours total, {tracks.Sum(t => t.PlayCount)} plays.");
            library.ForEach(i =>                            // S6: action as parameter
            {
                if (i is Album a)
                {
                    var n = tracks.Count(t => t.AlbumId == a.Id);
                    Console.WriteLine($"  {a.DisplayName}: {n} tracks");
                }
            });

            // S7: LINQ
            var stats = library.Summary();
            Console.WriteLine($"  {stats.Count} items, {stats.DistinctInitials} different initials, " +
                              $"average name {stats.AverageNameLength:F1} chars, longest: '{stats.LongestName}'.");
            var mostPlayed = tracks
                .Where(t => t.PlayCount > 0)
                .OrderByDescending(t => t.PlayCount)
                .ThenBy(t => t.Title, StringComparer.OrdinalIgnoreCase)
                .Take(3)
                .Select(t => $"{t.Title} ({t.PlayCount})");
            Console.WriteLine($"  most played: {string.Join(", ", mostPlayed.DefaultIfEmpty("nothing yet"))}");
            break;

        case "9":
            // S7: paging over name order with Skip/Take
            for (var skip = 0; ; skip += pageSize)
            {
                var page = library.Page(skip, pageSize).ToList();
                if (page.Count == 0) { Console.WriteLine("  (end)"); break; }
                foreach (var item in page)
                    Console.WriteLine($"  [{Kind(item)}] {item.DisplayName}");
                if (skip + pageSize >= library.Count) break;
                if (Ask("Enter = next page, q = stop").Equals("q", StringComparison.OrdinalIgnoreCase)) break;
            }
            break;

        default:
            Console.WriteLine("  choose 0-9 or s.");
            break;
    }
}

await LibraryFile.SaveAsync(library, savePath);
Console.WriteLine($"Saved to {savePath}.");

static string Ask(string prompt)
{
    Console.Write($"{prompt}: ");
    return Console.ReadLine()?.Trim() ?? "";
}

static void Show(IEnumerable<IItem> items)
{
    var list = items.ToList();
    if (list.Count == 0) Console.WriteLine("  no matches.");
    foreach (var item in list)
        Console.WriteLine($"  {item}");
}

Track? TrackById()
{
    var id = Ask("Track Id");
    if (library[id] is Track t) return t;
    Console.WriteLine("  no such track.");
    return null;
}

static string Kind(IItem item) => item switch   // S4: switch expression
{
    Track => "track",
    Album => "album",
    _ => "item",
};
